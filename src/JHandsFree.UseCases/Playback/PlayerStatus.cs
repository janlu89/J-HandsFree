namespace JHandsFree.UseCases.Playback;

/// <summary>
/// The state the YouTube player reports, as opposed to the <see cref="Domain.PlaybackState"/> the app derives from it.
/// </summary>
public enum PlayerStatus
{
    Playing,
    Paused,
    Buffering,
    Ended,
}
