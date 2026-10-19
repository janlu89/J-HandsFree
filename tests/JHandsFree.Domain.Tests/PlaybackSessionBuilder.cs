namespace JHandsFree.Domain.Tests;

/// <summary>
/// Builds a <see cref="PlaybackSession"/> in a given state by firing only valid transitions, the way the coordinators will.
/// An ad state is reached through a mid-roll ad, so the session already has a video.
/// </summary>
internal sealed class PlaybackSessionBuilder
{
    private PlaybackState _state = PlaybackState.Idle;

    public static VideoMetadata SomeVideo { get; } =
        new() { Id = VideoId.Create("M7lc1UVf-VE"), Title = "Road trip mix", Channel = "Example Channel" };

    public static DateTimeOffset AdStartTime { get; } = new(2026, 10, 19, 8, 30, 0, TimeSpan.Zero);

    public PlaybackSessionBuilder InState(PlaybackState state)
    {
        _state = state;
        return this;
    }

    public PlaybackSession Build()
    {
        var session = new PlaybackSession();
        foreach (var step in StepsTo(_state))
        {
            step(session);
        }

        return session.State == _state
            ? session
            : throw new InvalidOperationException($"The builder reached {session.State} instead of {_state}.");
    }

    private static Action<PlaybackSession>[] StepsTo(PlaybackState state) =>
        state switch
        {
            PlaybackState.Idle => [],
            PlaybackState.Loading => [Open],
            PlaybackState.PlayingContent => [Open, StartContent],
            PlaybackState.PlayingAd => [Open, StartContent, StartAd],
            PlaybackState.AdSkippable => [Open, StartContent, StartAd, MarkAdSkippable],
            PlaybackState.Paused => [Open, StartContent, Pause],
            PlaybackState.Ended => [Open, StartContent, FinishPlaylist],
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "Unknown playback state."),
        };

    private static void Open(PlaybackSession session) => session.Open();

    private static void StartContent(PlaybackSession session) => session.StartContent(SomeVideo);

    private static void StartAd(PlaybackSession session) => session.StartAd(AdStartTime);

    private static void MarkAdSkippable(PlaybackSession session) => session.MarkAdSkippable();

    private static void Pause(PlaybackSession session) => session.Pause();

    private static void FinishPlaylist(PlaybackSession session) => session.FinishPlaylist();
}
