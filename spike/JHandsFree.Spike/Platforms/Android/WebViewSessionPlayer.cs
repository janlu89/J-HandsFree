using Android.Content;
using Android.OS;
using AndroidX.Concurrent.Futures;
using AndroidX.Media3.Common;
using Google.Common.Util.Concurrent;
using PlayerConstants = AndroidX.Media3.Common.BasePlayer.InterfaceConsts;

// Media3's Java API has no nullability annotations, so the bindings mark every builder result as nullable,
// although Media3 builders always return themselves or a built object.
#nullable disable warnings

namespace JHandsFree.Spike;

// A player facade: it never decodes media. It describes the WebView's playback to Media3 and forwards play and pause back to the page.
public sealed class WebViewSessionPlayer(Context context) : SimpleBasePlayer(Looper.MainLooper)
{
    private const string WakeLockTag = "JHandsFreeSpike:playback";

    private readonly PowerManager.WakeLock _wakeLock = (context.GetSystemService(Context.PowerService) as PowerManager)?.NewWakeLock(WakeLockFlags.Partial, WakeLockTag);
    private bool _playWhenReady;

    protected override State GetState()
    {
        var commands = new PlayerCommands.Builder()
            .AddAll(PlayerConstants.CommandPlayPause, PlayerConstants.CommandGetCurrentMediaItem, PlayerConstants.CommandGetMetadata)
            .Build();
        var metadata = new MediaMetadata.Builder()
            .SetTitle("J-HandsFree spike")
            .SetArtist("m.youtube.com")
            .Build();
        var mediaItem = new MediaItemData.Builder("spike-session")
            .SetMediaMetadata(metadata)
            .Build();

        return new State.Builder()
            .SetAvailableCommands(commands)
            .SetPlaylist([mediaItem])
            .SetPlaybackState(PlayerConstants.StateReady)
            .SetPlayWhenReady(_playWhenReady, PlayerConstants.PlayWhenReadyChangeReasonUserRequest)
            .Build();
    }

    protected override IListenableFuture HandleSetPlayWhenReady(bool playWhenReady)
    {
        _playWhenReady = playWhenReady;
        if (playWhenReady)
        {
            SpikePlayback.PlayVideo();
            AcquireWakeLock();
        }
        else
        {
            SpikePlayback.PauseVideo();
            ReleaseWakeLock();
        }

        SpikeLog.Info($"Session player playWhenReady={playWhenReady}");
        return CompletedFuture();
    }

    protected override IListenableFuture HandleRelease()
    {
        ReleaseWakeLock();
        SpikeLog.Info("Session player released");
        return CompletedFuture();
    }

    private void AcquireWakeLock()
    {
        if (_wakeLock is { IsHeld: false })
        {
            _wakeLock.Acquire();
        }
    }

    private void ReleaseWakeLock()
    {
        if (_wakeLock is { IsHeld: true })
        {
            _wakeLock.Release();
        }
    }

    private static IListenableFuture CompletedFuture() => CallbackToFutureAdapter.GetFuture(new CompletedResolver());

    private sealed class CompletedResolver : Java.Lang.Object, CallbackToFutureAdapter.IResolver
    {
        public Java.Lang.Object AttachCompleter(CallbackToFutureAdapter.Completer completer)
        {
            completer.Set(null);
            return null;
        }
    }
}
