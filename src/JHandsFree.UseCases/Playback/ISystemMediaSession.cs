namespace JHandsFree.UseCases.Playback;

/// <summary>
/// The port to the system media session: the notification, the lock screen and connected devices.
/// </summary>
public interface ISystemMediaSession
{
    void Publish(PlaybackSnapshot snapshot);
}
