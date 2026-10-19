using JHandsFree.Domain;
using JHandsFree.UseCases.Playback;
using JHandsFree.UseCases.Player;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace JHandsFree.UseCases.Tests;

public sealed class PlaybackCoordinatorTests : IDisposable
{
    private static readonly VideoMetadata _firstVideo =
        new() { Id = VideoId.Create("M7lc1UVf-VE"), Title = "Road trip mix", Channel = "Example Channel" };

    private static readonly VideoMetadata _secondVideo =
        new() { Id = VideoId.Create("a-b_c-d_e-f"), Title = "Evening news", Channel = "Other Channel" };

    private readonly IPlayerCommands _playerCommands = Substitute.For<IPlayerCommands>();
    private readonly ISystemMediaSession _mediaSession = Substitute.For<ISystemMediaSession>();
    private readonly IPlayerVisibility _visibility = Substitute.For<IPlayerVisibility>();
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 10, 19, 8, 30, 0, TimeSpan.Zero));
    private readonly PlaybackCoordinator _coordinator;
    private PlaybackSnapshot? _lastSnapshot;

    public PlaybackCoordinatorTests()
    {
        _mediaSession.When(session => session.Publish(Arg.Any<PlaybackSnapshot>()))
            .Do(call => _lastSnapshot = call.Arg<PlaybackSnapshot>());
        _coordinator = new PlaybackCoordinator(
            _playerCommands,
            _mediaSession,
            _visibility,
            _time,
            PauseTimeoutPolicy.Default,
            NullLogger<PlaybackCoordinator>.Instance);
    }

    public void Dispose() => _coordinator.Dispose();

    [Fact]
    public async Task Buffering_WhenIdle_PublishesLoading()
    {
        await Report(PlayerStatus.Buffering);

        _lastSnapshot.ShouldBe(new PlaybackSnapshot(PlaybackState.Loading, null, TimeSpan.Zero));
    }

    [Fact]
    public async Task Buffering_WhilePlayingContent_KeepsPlayingContent()
    {
        await Report(PlayerStatus.Playing, _firstVideo);

        await Report(PlayerStatus.Buffering, _firstVideo);

        _lastSnapshot.ShouldBe(new PlaybackSnapshot(PlaybackState.PlayingContent, _firstVideo, TimeSpan.Zero));
    }

    [Fact]
    public async Task Playing_WhenIdle_PublishesPlayingContentWithTheVideo()
    {
        await Report(PlayerStatus.Playing, _firstVideo);

        _lastSnapshot.ShouldBe(new PlaybackSnapshot(PlaybackState.PlayingContent, _firstVideo, TimeSpan.Zero));
    }

    [Fact]
    public async Task Playing_WhenIdle_KeepsThePageVisible()
    {
        await Report(PlayerStatus.Playing, _firstVideo);

        _visibility.Received(1).KeepVisible();
    }

    [Fact]
    public async Task Playing_WhenLoading_PublishesPlayingContentWithTheVideo()
    {
        await Report(PlayerStatus.Buffering);

        await Report(PlayerStatus.Playing, _firstVideo);

        _lastSnapshot.ShouldBe(new PlaybackSnapshot(PlaybackState.PlayingContent, _firstVideo, TimeSpan.Zero));
    }

    [Fact]
    public async Task Playing_WithoutVideoWhenIdle_PublishesLoadingAndKeepsThePageVisible()
    {
        await Report(PlayerStatus.Playing);

        _lastSnapshot.ShouldBe(new PlaybackSnapshot(PlaybackState.Loading, null, TimeSpan.Zero));
        _visibility.Received(1).KeepVisible();
    }

    [Fact]
    public async Task Playing_WithAnotherVideo_PublishesTheNextVideo()
    {
        await Report(PlayerStatus.Playing, _firstVideo);

        await Report(PlayerStatus.Playing, _secondVideo);

        _lastSnapshot.ShouldBe(new PlaybackSnapshot(PlaybackState.PlayingContent, _secondVideo, TimeSpan.Zero));
    }

    [Fact]
    public async Task Playing_WithNewMetadataForTheSameVideo_PublishesTheNewMetadata()
    {
        var retitled = _firstVideo with { Title = "Road trip mix, part 2" };
        await Report(PlayerStatus.Playing, _firstVideo);

        await Report(PlayerStatus.Playing, retitled);

        _lastSnapshot.ShouldBe(new PlaybackSnapshot(PlaybackState.PlayingContent, retitled, TimeSpan.Zero));
    }

    [Fact]
    public async Task Playing_AtAPosition_PublishesThePosition()
    {
        await Report(PlayerStatus.Playing, _firstVideo, positionSeconds: 42);

        _lastSnapshot.ShouldBe(new PlaybackSnapshot(PlaybackState.PlayingContent, _firstVideo, TimeSpan.FromSeconds(42)));
    }

    [Fact]
    public async Task Paused_WhilePlayingContent_PublishesPaused()
    {
        await Report(PlayerStatus.Playing, _firstVideo);

        await Report(PlayerStatus.Paused, _firstVideo);

        _lastSnapshot.ShouldBe(new PlaybackSnapshot(PlaybackState.Paused, _firstVideo, TimeSpan.Zero));
    }

    [Fact]
    public async Task Playing_WhenPaused_PublishesPlayingContent()
    {
        await Report(PlayerStatus.Playing, _firstVideo);
        await Report(PlayerStatus.Paused, _firstVideo);

        await Report(PlayerStatus.Playing, _firstVideo);

        _lastSnapshot.ShouldBe(new PlaybackSnapshot(PlaybackState.PlayingContent, _firstVideo, TimeSpan.Zero));
    }

    [Fact]
    public async Task PauseTimeout_AfterTenMinutes_PublishesIdleAndAllowsHidingThePage()
    {
        await Report(PlayerStatus.Playing, _firstVideo);
        await Report(PlayerStatus.Paused, _firstVideo);

        _time.Advance(TimeSpan.FromMinutes(10));

        _lastSnapshot.ShouldBe(new PlaybackSnapshot(PlaybackState.Idle, null, TimeSpan.Zero));
        _visibility.Received(1).AllowHidden();
    }

    [Fact]
    public async Task PauseTimeout_BeforeTenMinutes_StaysPaused()
    {
        await Report(PlayerStatus.Playing, _firstVideo);
        await Report(PlayerStatus.Paused, _firstVideo);

        _time.Advance(TimeSpan.FromMinutes(10) - TimeSpan.FromSeconds(1));

        _lastSnapshot.ShouldBe(new PlaybackSnapshot(PlaybackState.Paused, _firstVideo, TimeSpan.Zero));
        _visibility.DidNotReceive().AllowHidden();
    }

    [Fact]
    public async Task PauseTimeout_AfterAResume_DoesNotRelease()
    {
        await Report(PlayerStatus.Playing, _firstVideo);
        await Report(PlayerStatus.Paused, _firstVideo);
        await Report(PlayerStatus.Playing, _firstVideo);

        _time.Advance(TimeSpan.FromMinutes(10));

        _lastSnapshot.ShouldBe(new PlaybackSnapshot(PlaybackState.PlayingContent, _firstVideo, TimeSpan.Zero));
        _visibility.DidNotReceive().AllowHidden();
    }

    [Fact]
    public async Task Paused_WhenIdle_StartsNoPauseTimeout()
    {
        await Report(PlayerStatus.Paused);

        _time.Advance(TimeSpan.FromMinutes(10));

        _lastSnapshot.ShouldBe(new PlaybackSnapshot(PlaybackState.Idle, null, TimeSpan.Zero));
        _visibility.DidNotReceive().AllowHidden();
    }

    [Fact]
    public async Task Ended_WhilePlayingContent_PublishesEndedAndAllowsHidingThePage()
    {
        await Report(PlayerStatus.Playing, _firstVideo);

        await Report(PlayerStatus.Ended, _firstVideo);

        _lastSnapshot.ShouldBe(new PlaybackSnapshot(PlaybackState.Ended, _firstVideo, TimeSpan.Zero));
        _visibility.Received(1).AllowHidden();
    }

    [Fact]
    public async Task Playing_AfterEnded_StartsANewSessionWithTheVideo()
    {
        await Report(PlayerStatus.Playing, _firstVideo);
        await Report(PlayerStatus.Ended, _firstVideo);

        await Report(PlayerStatus.Playing, _secondVideo);

        _lastSnapshot.ShouldBe(new PlaybackSnapshot(PlaybackState.PlayingContent, _secondVideo, TimeSpan.Zero));
        _visibility.Received(2).KeepVisible();
    }

    [Theory]
    [InlineData(MediaButton.Play, typeof(Play))]
    [InlineData(MediaButton.Pause, typeof(Pause))]
    [InlineData(MediaButton.Next, typeof(PlayNext))]
    [InlineData(MediaButton.Previous, typeof(PlayPrevious))]
    public async Task HandleMediaButton_Always_SendsTheMatchingCommandToThePage(MediaButton button, Type commandType)
    {
        await _coordinator.Handle(new HandleMediaButton(button), TestContext.Current.CancellationToken);

        await _playerCommands.Received(1).SendAsync(
            Arg.Is<PlayerCommand>(command => command.GetType() == commandType),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleMediaButton_Always_LeavesTheStateToThePage()
    {
        await Report(PlayerStatus.Playing, _firstVideo);
        _mediaSession.ClearReceivedCalls();

        await _coordinator.Handle(new HandleMediaButton(MediaButton.Pause), TestContext.Current.CancellationToken);

        _mediaSession.DidNotReceive().Publish(Arg.Any<PlaybackSnapshot>());
    }

    private async Task Report(PlayerStatus status, VideoMetadata? video = null, int positionSeconds = 0) =>
        await _coordinator.Handle(
            new PlayerStateChanged(status, video, TimeSpan.FromSeconds(positionSeconds)),
            TestContext.Current.CancellationToken);
}
