using System.Runtime.Versioning;
using System.Text.Json;
using Android.App;
using Android.Content;
using AndroidX.Core.Content;
using AndroidProcess = Android.OS.Process;

namespace JHandsFree.Spike;

// Step 0.7 evidence: process starts, why the previous process ended, screen changes and a heartbeat that exposes freezes as gaps.
public static class SpikeLifecycle
{
    private const int HeartbeatMinutes = 5;
    private const string LastRecordedExitPreference = "LastRecordedExitTimestamp";

    private static Timer? _heartbeat;
    private static int _webViewsCreated;
    private static int _webViewsDisconnected;
    private static long _processStartedAtMs;

    public static void OnProcessStarted(Context context)
    {
        _processStartedAtMs = NowMs();
        SpikeJournal.Append("process.started", $$"""{"pid":{{AndroidProcess.MyPid()}}}""");
        if (OperatingSystem.IsAndroidVersionAtLeast(30))
        {
            RecordPreviousExits(context);
        }

        ContextCompat.RegisterReceiver(context, new ScreenStateReceiver(), ScreenFilter(), ContextCompat.ReceiverNotExported);
        _heartbeat?.Dispose();
        _heartbeat = new Timer(_ => Beat(), null, TimeSpan.FromMinutes(HeartbeatMinutes), TimeSpan.FromMinutes(HeartbeatMinutes));
    }

    public static void OnWebViewCreated()
    {
        _webViewsCreated++;
        SpikeJournal.Append("webview.created", $$"""{"count":{{_webViewsCreated}}}""");
    }

    public static void OnWebViewDisconnected()
    {
        _webViewsDisconnected++;
        SpikeJournal.Append("webview.disconnected", $$"""{"count":{{_webViewsDisconnected}}}""");
    }

    private static void Beat()
    {
        var uptimeMinutes = (NowMs() - _processStartedAtMs) / 60000;
        var webViewsAlive = _webViewsCreated - _webViewsDisconnected;
        var sessionActive = SpikePlayback.Player is not null ? "true" : "false";
        SpikeJournal.Append("heartbeat", $$"""{"uptimeMinutes":{{uptimeMinutes}},"webViewsAlive":{{webViewsAlive}},"sessionActive":{{sessionActive}}}""");
    }

    // ApplicationExitInfo reports why each earlier process of the app ended: killed by the user, by the system, frozen, crashed.
    [SupportedOSPlatform("android30.0")]
    private static void RecordPreviousExits(Context context)
    {
        if (context.GetSystemService(Context.ActivityService) is not ActivityManager manager)
        {
            return;
        }

        var lastRecorded = Preferences.Default.Get(LastRecordedExitPreference, 0L);
        var newestRecorded = lastRecorded;
        foreach (var exit in manager.GetHistoricalProcessExitReasons(context.PackageName, 0, 10).Reverse())
        {
            if (exit.Timestamp <= lastRecorded)
            {
                continue;
            }

            var at = DateTimeOffset.FromUnixTimeMilliseconds(exit.Timestamp).ToLocalTime().ToString("O");
            var processName = JsonEncodedText.Encode(exit.ProcessName ?? string.Empty);
            var description = JsonEncodedText.Encode(exit.Description ?? string.Empty);
            SpikeJournal.Append("process.previousExit", $$"""{"exitedAt":"{{at}}","process":"{{processName}}","reason":{{exit.Reason}},"importance":{{exit.Importance}},"description":"{{description}}"}""");
            newestRecorded = Math.Max(newestRecorded, exit.Timestamp);
        }

        Preferences.Default.Set(LastRecordedExitPreference, newestRecorded);
    }

    private static IntentFilter ScreenFilter()
    {
        var filter = new IntentFilter(Intent.ActionScreenOn);
        filter.AddAction(Intent.ActionScreenOff);
        return filter;
    }

    private static long NowMs() => TimeProvider.System.GetUtcNow().ToUnixTimeMilliseconds();
}
