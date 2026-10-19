using JHandsFree.Domain;
using JHandsFree.UseCases.Player;
using Mediator;
using Microsoft.Extensions.Logging;

namespace JHandsFree.UseCases.Playback;

/// <summary>
/// The one place that decides what the player does. It follows the state the page reports, keeps the system media
/// session in sync, forwards media buttons to the page and releases background playback after a long pause.
/// </summary>
/// <remarks>
/// Player events arrive on the UI thread and the pause timer fires on a pool thread, so one lock guards the session.
/// The ports are called after the lock is released, so an adapter that posts to the UI thread cannot deadlock.
/// </remarks>
public sealed partial class PlaybackCoordinator(
    IPlayerCommands playerCommands,
    ISystemMediaSession mediaSession,
    IPlayerVisibility visibility,
    TimeProvider timeProvider,
    PauseTimeoutPolicy pauseTimeoutPolicy,
    ILogger<PlaybackCoordinator> logger)
    : INotificationHandler<PlayerStateChanged>, ICommandHandler<HandleMediaButton>, IDisposable
{
    private readonly Lock _gate = new();
    private PlaybackSession _session = new();
    private TimeSpan _position;
    private DateTimeOffset _pausedAt;
    private ITimer? _pauseTimer;

    private enum VisibilityChange
    {
        None,
        KeepVisible,
        AllowHidden,
    }

    public ValueTask Handle(PlayerStateChanged notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);
        VisibilityChange change;
        PlaybackSnapshot snapshot;
        lock (_gate)
        {
            _position = notification.Position;
            change = Follow(notification);
            snapshot = Snapshot();
        }

        Apply(change, snapshot);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Forwards the button to the page and leaves the session alone: the page confirms the result with its next state.
    /// </summary>
    public async ValueTask<Unit> Handle(HandleMediaButton command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        await playerCommands.SendAsync(ToPlayerCommand(command.Button), cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }

    public void Dispose()
    {
        lock (_gate)
        {
            CancelPauseTimer();
        }
    }

    private static PlayerCommand ToPlayerCommand(MediaButton button) =>
        button switch
        {
            MediaButton.Play => new Play(),
            MediaButton.Pause => new Pause(),
            MediaButton.Next => new PlayNext(),
            MediaButton.Previous => new PlayPrevious(),
            _ => throw new ArgumentOutOfRangeException(nameof(button), button, "Unknown media button."),
        };

    private VisibilityChange Follow(PlayerStateChanged notification)
    {
        if (_session.State == PlaybackState.Ended && notification.Status is PlayerStatus.Playing or PlayerStatus.Buffering)
        {
            _session = new PlaybackSession();
        }

        return notification.Status switch
        {
            PlayerStatus.Buffering => FollowBuffering(),
            PlayerStatus.Playing => FollowPlaying(notification.Video),
            PlayerStatus.Paused => FollowPaused(),
            PlayerStatus.Ended => FollowEnded(),
            _ => Ignore(notification.Status),
        };
    }

    private VisibilityChange FollowBuffering() =>
        _session.State == PlaybackState.Idle ? OpenSession() : VisibilityChange.None;

    private VisibilityChange OpenSession()
    {
        _session.Open();
        return VisibilityChange.None;
    }

    private VisibilityChange FollowPlaying(VideoMetadata? video) =>
        _session.State switch
        {
            PlaybackState.Idle or PlaybackState.Loading => StartContent(video),
            PlaybackState.Paused => Resume(video),
            _ => UpdateVideo(video),
        };

    private VisibilityChange StartContent(VideoMetadata? video)
    {
        if (_session.State == PlaybackState.Idle)
        {
            _session.Open();
        }

        if (video is not null)
        {
            _session.StartContent(video);
        }

        return VisibilityChange.KeepVisible;
    }

    private VisibilityChange Resume(VideoMetadata? video)
    {
        _session.Resume();
        CancelPauseTimer();
        return UpdateVideo(video);
    }

    private VisibilityChange UpdateVideo(VideoMetadata? video)
    {
        if (video is not null)
        {
            _session.UpdateVideo(video);
        }

        return VisibilityChange.None;
    }

    private VisibilityChange FollowPaused()
    {
        if (!_session.Pause().IsValid)
        {
            return Ignore(PlayerStatus.Paused);
        }

        StartPauseTimer();
        return VisibilityChange.None;
    }

    private VisibilityChange FollowEnded()
    {
        if (!_session.FinishPlaylist().IsValid)
        {
            return Ignore(PlayerStatus.Ended);
        }

        return VisibilityChange.AllowHidden;
    }

    private VisibilityChange Ignore(PlayerStatus status)
    {
        LogStatusIgnored(status, _session.State);
        return VisibilityChange.None;
    }

    private void StartPauseTimer()
    {
        CancelPauseTimer();
        _pausedAt = timeProvider.GetUtcNow();
        _pauseTimer = timeProvider.CreateTimer(_ => OnPauseTimeout(), null, pauseTimeoutPolicy.Timeout, Timeout.InfiniteTimeSpan);
    }

    private void CancelPauseTimer()
    {
        _pauseTimer?.Dispose();
        _pauseTimer = null;
    }

    private void OnPauseTimeout()
    {
        PlaybackSnapshot snapshot;
        lock (_gate)
        {
            if (_session.State != PlaybackState.Paused || !pauseTimeoutPolicy.HasExpired(_pausedAt, timeProvider.GetUtcNow()))
            {
                return;
            }

            _session.ReleaseAfterPauseTimeout();
            CancelPauseTimer();
            snapshot = Snapshot();
        }

        Apply(VisibilityChange.AllowHidden, snapshot);
    }

    private PlaybackSnapshot Snapshot() => new(_session.State, _session.CurrentVideo, _position);

    private void Apply(VisibilityChange change, PlaybackSnapshot snapshot)
    {
        switch (change)
        {
            case VisibilityChange.KeepVisible:
                visibility.KeepVisible();
                break;
            case VisibilityChange.AllowHidden:
                visibility.AllowHidden();
                break;
        }

        mediaSession.Publish(snapshot);
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Player status {Status} ignored in playback state {State}")]
    private partial void LogStatusIgnored(PlayerStatus status, PlaybackState state);
}
