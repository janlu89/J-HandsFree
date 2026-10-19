using JHandsFree.UseCases.Playback;

namespace JHandsFree.Infrastructure.Android;

/// <summary>
/// Ignores every snapshot, because the app has no system media session yet.
/// </summary>
internal sealed class DisconnectedMediaSession : ISystemMediaSession
{
    public void Publish(PlaybackSnapshot snapshot)
    {
    }
}
