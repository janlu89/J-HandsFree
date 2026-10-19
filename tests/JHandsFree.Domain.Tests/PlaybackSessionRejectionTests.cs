namespace JHandsFree.Domain.Tests;

public sealed class PlaybackSessionRejectionTests
{
    [Theory]
    [InlineData(PlaybackState.Loading)]
    [InlineData(PlaybackState.PlayingContent)]
    [InlineData(PlaybackState.PlayingAd)]
    [InlineData(PlaybackState.AdSkippable)]
    [InlineData(PlaybackState.Paused)]
    [InlineData(PlaybackState.Ended)]
    public void Open_WhenNotIdle_ReturnsInvalidTransition(PlaybackState state)
    {
        var session = new PlaybackSessionBuilder().InState(state).Build();

        var result = session.Open();

        result.ShouldBe(TransitionResult.Invalid(state));
        session.State.ShouldBe(state);
    }

    [Theory]
    [InlineData(PlaybackState.Idle)]
    [InlineData(PlaybackState.PlayingContent)]
    [InlineData(PlaybackState.PlayingAd)]
    [InlineData(PlaybackState.AdSkippable)]
    [InlineData(PlaybackState.Paused)]
    [InlineData(PlaybackState.Ended)]
    public void StartContent_WhenNotLoading_ReturnsInvalidTransition(PlaybackState state)
    {
        var session = new PlaybackSessionBuilder().InState(state).Build();

        var result = session.StartContent(PlaybackSessionBuilder.SomeVideo);

        result.ShouldBe(TransitionResult.Invalid(state));
        session.State.ShouldBe(state);
    }

    [Theory]
    [InlineData(PlaybackState.Idle)]
    [InlineData(PlaybackState.PlayingAd)]
    [InlineData(PlaybackState.AdSkippable)]
    [InlineData(PlaybackState.Paused)]
    [InlineData(PlaybackState.Ended)]
    public void StartAd_WhenNeitherLoadingNorPlayingContent_ReturnsInvalidTransition(PlaybackState state)
    {
        var session = new PlaybackSessionBuilder().InState(state).Build();

        var result = session.StartAd(PlaybackSessionBuilder.AdStartTime);

        result.ShouldBe(TransitionResult.Invalid(state));
        session.State.ShouldBe(state);
    }

    [Theory]
    [InlineData(PlaybackState.Idle)]
    [InlineData(PlaybackState.Loading)]
    [InlineData(PlaybackState.PlayingContent)]
    [InlineData(PlaybackState.AdSkippable)]
    [InlineData(PlaybackState.Paused)]
    [InlineData(PlaybackState.Ended)]
    public void MarkAdSkippable_WhenNoAdIsPlaying_ReturnsInvalidTransition(PlaybackState state)
    {
        var session = new PlaybackSessionBuilder().InState(state).Build();

        var result = session.MarkAdSkippable();

        result.ShouldBe(TransitionResult.Invalid(state));
        session.State.ShouldBe(state);
    }

    [Theory]
    [InlineData(PlaybackState.Idle)]
    [InlineData(PlaybackState.Loading)]
    [InlineData(PlaybackState.PlayingContent)]
    [InlineData(PlaybackState.Paused)]
    [InlineData(PlaybackState.Ended)]
    public void EndAd_WhenNoAdIsPlaying_ReturnsInvalidTransition(PlaybackState state)
    {
        var session = new PlaybackSessionBuilder().InState(state).Build();

        var result = session.EndAd();

        result.ShouldBe(TransitionResult.Invalid(state));
        session.State.ShouldBe(state);
    }

    [Theory]
    [InlineData(PlaybackState.Idle)]
    [InlineData(PlaybackState.Loading)]
    [InlineData(PlaybackState.PlayingAd)]
    [InlineData(PlaybackState.AdSkippable)]
    [InlineData(PlaybackState.Paused)]
    [InlineData(PlaybackState.Ended)]
    public void LoadNextItem_WhenNotPlayingContent_ReturnsInvalidTransition(PlaybackState state)
    {
        var session = new PlaybackSessionBuilder().InState(state).Build();

        var result = session.LoadNextItem();

        result.ShouldBe(TransitionResult.Invalid(state));
        session.State.ShouldBe(state);
    }

    [Theory]
    [InlineData(PlaybackState.Idle)]
    [InlineData(PlaybackState.Loading)]
    [InlineData(PlaybackState.PlayingAd)]
    [InlineData(PlaybackState.AdSkippable)]
    [InlineData(PlaybackState.Paused)]
    [InlineData(PlaybackState.Ended)]
    public void Pause_WhenNotPlayingContent_ReturnsInvalidTransition(PlaybackState state)
    {
        var session = new PlaybackSessionBuilder().InState(state).Build();

        var result = session.Pause();

        result.ShouldBe(TransitionResult.Invalid(state));
        session.State.ShouldBe(state);
    }

    [Theory]
    [InlineData(PlaybackState.Idle)]
    [InlineData(PlaybackState.Loading)]
    [InlineData(PlaybackState.PlayingContent)]
    [InlineData(PlaybackState.PlayingAd)]
    [InlineData(PlaybackState.AdSkippable)]
    [InlineData(PlaybackState.Ended)]
    public void Resume_WhenNotPaused_ReturnsInvalidTransition(PlaybackState state)
    {
        var session = new PlaybackSessionBuilder().InState(state).Build();

        var result = session.Resume();

        result.ShouldBe(TransitionResult.Invalid(state));
        session.State.ShouldBe(state);
    }

    [Theory]
    [InlineData(PlaybackState.Idle)]
    [InlineData(PlaybackState.Loading)]
    [InlineData(PlaybackState.PlayingContent)]
    [InlineData(PlaybackState.PlayingAd)]
    [InlineData(PlaybackState.AdSkippable)]
    [InlineData(PlaybackState.Ended)]
    public void ReleaseAfterPauseTimeout_WhenNotPaused_ReturnsInvalidTransition(PlaybackState state)
    {
        var session = new PlaybackSessionBuilder().InState(state).Build();

        var result = session.ReleaseAfterPauseTimeout();

        result.ShouldBe(TransitionResult.Invalid(state));
        session.State.ShouldBe(state);
    }

    [Theory]
    [InlineData(PlaybackState.Idle)]
    [InlineData(PlaybackState.Loading)]
    [InlineData(PlaybackState.PlayingAd)]
    [InlineData(PlaybackState.AdSkippable)]
    [InlineData(PlaybackState.Paused)]
    [InlineData(PlaybackState.Ended)]
    public void FinishPlaylist_WhenNotPlayingContent_ReturnsInvalidTransition(PlaybackState state)
    {
        var session = new PlaybackSessionBuilder().InState(state).Build();

        var result = session.FinishPlaylist();

        result.ShouldBe(TransitionResult.Invalid(state));
        session.State.ShouldBe(state);
    }

    [Theory]
    [InlineData(PlaybackState.Idle)]
    [InlineData(PlaybackState.Loading)]
    [InlineData(PlaybackState.Ended)]
    public void UpdateVideo_WhenNoItemIsActive_ReturnsInvalidTransition(PlaybackState state)
    {
        var session = new PlaybackSessionBuilder().InState(state).Build();

        var result = session.UpdateVideo(PlaybackSessionBuilder.SomeVideo);

        result.ShouldBe(TransitionResult.Invalid(state));
        session.State.ShouldBe(state);
    }

    [Fact]
    public void RejectedTransition_Always_LeavesVideoAndAdBreakUnchanged()
    {
        var session = new PlaybackSessionBuilder().InState(PlaybackState.AdSkippable).Build();
        var video = session.CurrentVideo;
        var adBreak = session.CurrentAdBreak;

        session.LoadNextItem();

        session.ShouldSatisfyAllConditions(
            () => session.CurrentVideo.ShouldBe(video),
            () => session.CurrentAdBreak.ShouldBe(adBreak));
    }
}
