using Android.App;
using Android.Content.PM;
using AndroidX.Media3.Session;

namespace JHandsFree.Spike;

[Service(
    Name = "com.janlu89.jhandsfree.spike.PlaybackService",
    Exported = true,
    ForegroundServiceType = ForegroundService.TypeMediaPlayback)]
[IntentFilter(["androidx.media3.session.MediaSessionService"])]
public sealed class PlaybackService : MediaSessionService
{
    private MediaSession? _session;

    public override void OnCreate()
    {
        base.OnCreate();
        _session = new MediaSession.Builder(this, new WebViewSessionPlayer(this)).Build();
        SpikeLog.Info("Playback service created");
    }

    public override MediaSession? OnGetSession(MediaSession.ControllerInfo? controllerInfo) => _session;

    public override void OnDestroy()
    {
        _session?.Player?.Release();
        _session?.Release();
        _session = null;
        SpikeLog.Info("Playback service destroyed");
        base.OnDestroy();
    }
}
