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
        var player = new WebViewSessionPlayer(this);
        SpikePlayback.Player = player;
        _session = new MediaSession.Builder(this, player).Build();
        SpikeLog.Info("Playback service created");
        SpikeJournal.Append("service.created");
    }

    public override MediaSession? OnGetSession(MediaSession.ControllerInfo? controllerInfo) => _session;

    public override void OnTaskRemoved(Android.Content.Intent? rootIntent)
    {
        SpikeJournal.Append("service.taskRemoved");
        base.OnTaskRemoved(rootIntent);
    }

    public override void OnDestroy()
    {
        _session?.Player?.Release();
        _session?.Release();
        _session = null;
        SpikePlayback.Player = null;
        SpikeLog.Info("Playback service destroyed");
        SpikeJournal.Append("service.destroyed");
        base.OnDestroy();
    }
}
