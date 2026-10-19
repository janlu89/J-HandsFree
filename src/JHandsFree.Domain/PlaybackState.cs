namespace JHandsFree.Domain;

public enum PlaybackState
{
    Idle,
    Loading,
    PlayingContent,
    PlayingAd,
    AdSkippable,
    Paused,
    Ended,
}
