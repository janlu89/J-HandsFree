namespace JHandsFree.Domain;

/// <summary>
/// The playback in the page as a state machine. Player events can arrive out of order or twice, so a request that the
/// current state does not allow is not an error: it returns an invalid <see cref="TransitionResult"/> and changes nothing.
/// </summary>
public sealed class PlaybackSession
{
    private enum Trigger
    {
        Open,
        StartContent,
        StartAd,
        MarkAdSkippable,
        EndAd,
        LoadNextItem,
        Pause,
        Resume,
        ReleaseAfterPauseTimeout,
        FinishPlaylist,
    }

    public PlaybackState State { get; private set; } = PlaybackState.Idle;

    /// <summary>
    /// The video being played, or <see langword="null"/> before the page reports one, for example during a pre-roll ad.
    /// </summary>
    public VideoMetadata? CurrentVideo { get; private set; }

    public AdBreak? CurrentAdBreak { get; private set; }

    public bool HasSkippableAd => State == PlaybackState.AdSkippable;

    private bool HasActiveItem =>
        State is PlaybackState.PlayingContent or PlaybackState.PlayingAd or PlaybackState.AdSkippable or PlaybackState.Paused;

    private AdBreak ActiveAdBreak =>
        CurrentAdBreak ?? throw new InvalidOperationException($"The session is in {State} without an ad break.");

    public TransitionResult Open() => Fire(Trigger.Open);

    public TransitionResult StartContent(VideoMetadata video)
    {
        ArgumentNullException.ThrowIfNull(video);
        return Fire(Trigger.StartContent, () => CurrentVideo = video);
    }

    public TransitionResult StartAd(DateTimeOffset startedAt) =>
        Fire(Trigger.StartAd, () => CurrentAdBreak = AdBreak.Start(startedAt));

    public TransitionResult MarkAdSkippable() =>
        Fire(Trigger.MarkAdSkippable, () => CurrentAdBreak = ActiveAdBreak.MarkSkippable());

    public TransitionResult EndAd() => Fire(Trigger.EndAd, () => CurrentAdBreak = null);

    public TransitionResult LoadNextItem() => Fire(Trigger.LoadNextItem, () => CurrentVideo = null);

    public TransitionResult Pause() => Fire(Trigger.Pause);

    public TransitionResult Resume() => Fire(Trigger.Resume);

    public TransitionResult ReleaseAfterPauseTimeout() => Fire(Trigger.ReleaseAfterPauseTimeout, () => CurrentVideo = null);

    public TransitionResult FinishPlaylist() => Fire(Trigger.FinishPlaylist);

    /// <summary>
    /// Records the video the page reports while an item is active. It is not a transition: the state stays the same.
    /// </summary>
    public TransitionResult UpdateVideo(VideoMetadata video)
    {
        ArgumentNullException.ThrowIfNull(video);
        if (!HasActiveItem)
        {
            return TransitionResult.Invalid(State);
        }

        CurrentVideo = video;
        return TransitionResult.Valid(State, State);
    }

    private TransitionResult Fire(Trigger trigger, Action? apply = null)
    {
        if (TargetOf(State, trigger) is not { } target)
        {
            return TransitionResult.Invalid(State);
        }

        var from = State;
        apply?.Invoke();
        State = target;
        return TransitionResult.Valid(from, target);
    }

    // The edges of the state diagram in docs/architecture/ARCHITECTURE.md, section 6. Every other pair is rejected.
    private static PlaybackState? TargetOf(PlaybackState from, Trigger trigger) =>
        (from, trigger) switch
        {
            (PlaybackState.Idle, Trigger.Open) => PlaybackState.Loading,
            (PlaybackState.Loading, Trigger.StartContent) => PlaybackState.PlayingContent,
            (PlaybackState.Loading, Trigger.StartAd) => PlaybackState.PlayingAd,
            (PlaybackState.PlayingContent, Trigger.StartAd) => PlaybackState.PlayingAd,
            (PlaybackState.PlayingAd, Trigger.MarkAdSkippable) => PlaybackState.AdSkippable,
            (PlaybackState.AdSkippable, Trigger.EndAd) => PlaybackState.PlayingContent,
            (PlaybackState.PlayingAd, Trigger.EndAd) => PlaybackState.PlayingContent,
            (PlaybackState.PlayingContent, Trigger.LoadNextItem) => PlaybackState.Loading,
            (PlaybackState.PlayingContent, Trigger.Pause) => PlaybackState.Paused,
            (PlaybackState.Paused, Trigger.Resume) => PlaybackState.PlayingContent,
            (PlaybackState.Paused, Trigger.ReleaseAfterPauseTimeout) => PlaybackState.Idle,
            (PlaybackState.PlayingContent, Trigger.FinishPlaylist) => PlaybackState.Ended,
            _ => null,
        };
}
