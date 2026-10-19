namespace JHandsFree.Domain.Tests;

public sealed class PlaybackSessionTests
{
    private static readonly VideoMetadata _nextVideo =
        new() { Id = VideoId.Create("a-b_c-d_e-f"), Title = "Evening news", Channel = "Other Channel" };

    [Fact]
    public void New_Always_IsIdleWithoutVideoOrAdBreak()
    {
        var session = new PlaybackSession();

        session.ShouldSatisfyAllConditions(
            () => session.State.ShouldBe(PlaybackState.Idle),
            () => session.CurrentVideo.ShouldBeNull(),
            () => session.CurrentAdBreak.ShouldBeNull());
    }

    [Fact]
    public void Open_WhenIdle_MovesToLoading()
    {
        var session = new PlaybackSessionBuilder().InState(PlaybackState.Idle).Build();

        var result = session.Open();

        result.ShouldBe(TransitionResult.Valid(PlaybackState.Idle, PlaybackState.Loading));
        session.State.ShouldBe(PlaybackState.Loading);
    }

    [Fact]
    public void StartContent_WhenLoading_MovesToPlayingContentWithTheVideo()
    {
        var session = new PlaybackSessionBuilder().InState(PlaybackState.Loading).Build();

        var result = session.StartContent(_nextVideo);

        result.ShouldBe(TransitionResult.Valid(PlaybackState.Loading, PlaybackState.PlayingContent));
        session.ShouldSatisfyAllConditions(
            () => session.State.ShouldBe(PlaybackState.PlayingContent),
            () => session.CurrentVideo.ShouldBe(_nextVideo));
    }

    [Fact]
    public void StartAd_WhenLoading_MovesToPlayingAdWithAnUnknownAdBreak()
    {
        var session = new PlaybackSessionBuilder().InState(PlaybackState.Loading).Build();

        var result = session.StartAd(PlaybackSessionBuilder.AdStartTime);

        result.ShouldBe(TransitionResult.Valid(PlaybackState.Loading, PlaybackState.PlayingAd));
        session.ShouldSatisfyAllConditions(
            () => session.State.ShouldBe(PlaybackState.PlayingAd),
            () => session.CurrentAdBreak.ShouldBe(AdBreak.Start(PlaybackSessionBuilder.AdStartTime)));
    }

    [Fact]
    public void StartAd_WhenPlayingContent_MovesToPlayingAdAndKeepsTheVideo()
    {
        var session = new PlaybackSessionBuilder().InState(PlaybackState.PlayingContent).Build();

        var result = session.StartAd(PlaybackSessionBuilder.AdStartTime);

        result.ShouldBe(TransitionResult.Valid(PlaybackState.PlayingContent, PlaybackState.PlayingAd));
        session.ShouldSatisfyAllConditions(
            () => session.State.ShouldBe(PlaybackState.PlayingAd),
            () => session.CurrentAdBreak.ShouldBe(AdBreak.Start(PlaybackSessionBuilder.AdStartTime)),
            () => session.CurrentVideo.ShouldBe(PlaybackSessionBuilder.SomeVideo));
    }

    [Fact]
    public void MarkAdSkippable_WhenPlayingAd_MovesToAdSkippableWithASkippableAdBreak()
    {
        var session = new PlaybackSessionBuilder().InState(PlaybackState.PlayingAd).Build();

        var result = session.MarkAdSkippable();

        result.ShouldBe(TransitionResult.Valid(PlaybackState.PlayingAd, PlaybackState.AdSkippable));
        session.ShouldSatisfyAllConditions(
            () => session.State.ShouldBe(PlaybackState.AdSkippable),
            () => session.CurrentAdBreak.ShouldBe(AdBreak.Start(PlaybackSessionBuilder.AdStartTime).MarkSkippable()));
    }

    [Fact]
    public void EndAd_WhenAdSkippable_MovesToPlayingContentWithoutAdBreak()
    {
        var session = new PlaybackSessionBuilder().InState(PlaybackState.AdSkippable).Build();

        var result = session.EndAd();

        result.ShouldBe(TransitionResult.Valid(PlaybackState.AdSkippable, PlaybackState.PlayingContent));
        session.ShouldSatisfyAllConditions(
            () => session.State.ShouldBe(PlaybackState.PlayingContent),
            () => session.CurrentAdBreak.ShouldBeNull());
    }

    [Fact]
    public void EndAd_WhenPlayingAd_MovesToPlayingContentWithoutAdBreak()
    {
        var session = new PlaybackSessionBuilder().InState(PlaybackState.PlayingAd).Build();

        var result = session.EndAd();

        result.ShouldBe(TransitionResult.Valid(PlaybackState.PlayingAd, PlaybackState.PlayingContent));
        session.ShouldSatisfyAllConditions(
            () => session.State.ShouldBe(PlaybackState.PlayingContent),
            () => session.CurrentAdBreak.ShouldBeNull());
    }

    [Fact]
    public void LoadNextItem_WhenPlayingContent_MovesToLoadingWithoutVideo()
    {
        var session = new PlaybackSessionBuilder().InState(PlaybackState.PlayingContent).Build();

        var result = session.LoadNextItem();

        result.ShouldBe(TransitionResult.Valid(PlaybackState.PlayingContent, PlaybackState.Loading));
        session.ShouldSatisfyAllConditions(
            () => session.State.ShouldBe(PlaybackState.Loading),
            () => session.CurrentVideo.ShouldBeNull());
    }

    [Fact]
    public void Pause_WhenPlayingContent_MovesToPausedAndKeepsTheVideo()
    {
        var session = new PlaybackSessionBuilder().InState(PlaybackState.PlayingContent).Build();

        var result = session.Pause();

        result.ShouldBe(TransitionResult.Valid(PlaybackState.PlayingContent, PlaybackState.Paused));
        session.ShouldSatisfyAllConditions(
            () => session.State.ShouldBe(PlaybackState.Paused),
            () => session.CurrentVideo.ShouldBe(PlaybackSessionBuilder.SomeVideo));
    }

    [Fact]
    public void Resume_WhenPaused_MovesToPlayingContent()
    {
        var session = new PlaybackSessionBuilder().InState(PlaybackState.Paused).Build();

        var result = session.Resume();

        result.ShouldBe(TransitionResult.Valid(PlaybackState.Paused, PlaybackState.PlayingContent));
        session.State.ShouldBe(PlaybackState.PlayingContent);
    }

    [Fact]
    public void ReleaseAfterPauseTimeout_WhenPaused_MovesToIdleWithoutVideo()
    {
        var session = new PlaybackSessionBuilder().InState(PlaybackState.Paused).Build();

        var result = session.ReleaseAfterPauseTimeout();

        result.ShouldBe(TransitionResult.Valid(PlaybackState.Paused, PlaybackState.Idle));
        session.ShouldSatisfyAllConditions(
            () => session.State.ShouldBe(PlaybackState.Idle),
            () => session.CurrentVideo.ShouldBeNull());
    }

    [Fact]
    public void FinishPlaylist_WhenPlayingContent_MovesToEnded()
    {
        var session = new PlaybackSessionBuilder().InState(PlaybackState.PlayingContent).Build();

        var result = session.FinishPlaylist();

        result.ShouldBe(TransitionResult.Valid(PlaybackState.PlayingContent, PlaybackState.Ended));
        session.State.ShouldBe(PlaybackState.Ended);
    }

    [Fact]
    public void EndAd_AfterPreRollAd_PlaysContentWithoutVideoYet()
    {
        var session = new PlaybackSessionBuilder().InState(PlaybackState.Loading).Build();
        session.StartAd(PlaybackSessionBuilder.AdStartTime);

        session.EndAd();

        session.ShouldSatisfyAllConditions(
            () => session.State.ShouldBe(PlaybackState.PlayingContent),
            () => session.CurrentVideo.ShouldBeNull());
    }

    [Theory]
    [InlineData(PlaybackState.PlayingContent)]
    [InlineData(PlaybackState.PlayingAd)]
    [InlineData(PlaybackState.AdSkippable)]
    [InlineData(PlaybackState.Paused)]
    public void UpdateVideo_WhileAnItemIsActive_ReplacesTheVideoAndKeepsTheState(PlaybackState state)
    {
        var session = new PlaybackSessionBuilder().InState(state).Build();

        var result = session.UpdateVideo(_nextVideo);

        result.ShouldBe(TransitionResult.Valid(state, state));
        session.ShouldSatisfyAllConditions(
            () => session.State.ShouldBe(state),
            () => session.CurrentVideo.ShouldBe(_nextVideo));
    }

    [Fact]
    public void HasSkippableAd_WhenAdSkippable_ReturnsTrue()
    {
        var session = new PlaybackSessionBuilder().InState(PlaybackState.AdSkippable).Build();

        session.HasSkippableAd.ShouldBeTrue();
    }

    [Theory]
    [InlineData(PlaybackState.Idle)]
    [InlineData(PlaybackState.Loading)]
    [InlineData(PlaybackState.PlayingContent)]
    [InlineData(PlaybackState.PlayingAd)]
    [InlineData(PlaybackState.Paused)]
    [InlineData(PlaybackState.Ended)]
    public void HasSkippableAd_InAnyOtherState_ReturnsFalse(PlaybackState state)
    {
        var session = new PlaybackSessionBuilder().InState(state).Build();

        session.HasSkippableAd.ShouldBeFalse();
    }

    // The null-forgiving operators below pass null on purpose, as untyped input at a boundary could.
    [Fact]
    public void StartContent_WithNullVideo_ThrowsArgumentNullException()
    {
        var session = new PlaybackSessionBuilder().InState(PlaybackState.Loading).Build();

        Action startContent = () => session.StartContent(null!);

        startContent.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void UpdateVideo_WithNullVideo_ThrowsArgumentNullException()
    {
        var session = new PlaybackSessionBuilder().InState(PlaybackState.PlayingContent).Build();

        Action updateVideo = () => session.UpdateVideo(null!);

        updateVideo.ShouldThrow<ArgumentNullException>();
    }
}
