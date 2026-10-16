using System.Text.Json;
using AndroidX.WebKit;
using AndroidUri = Android.Net.Uri;
using AndroidWebView = Android.Webkit.WebView;

namespace JHandsFree.Spike;

// Receives messages posted by the page through the injected "jhf" object. WebView only injects the object
// into frames whose origin matches the allowed rules; the origin and frame checks below are a second line of defence.
public sealed class PageBridge : Java.Lang.Object, WebViewCompat.IWebMessageListener
{
    public const string ObjectName = "jhf";
    public const string AllowedOrigin = "https://m.youtube.com";

    public void OnPostMessage(AndroidWebView? view, WebMessageCompat? message, AndroidUri? sourceOrigin, bool isMainFrame, JavaScriptReplyProxy? replyProxy)
    {
        var origin = sourceOrigin?.ToString();
        if (!isMainFrame || origin != AllowedOrigin)
        {
            SpikeLog.Info($"Bridge rejected a message from {origin}, main frame {isMainFrame}");
            return;
        }

        if (message?.Data is { } data && replyProxy is not null)
        {
            HandleMessage(data, replyProxy);
        }
    }

    private static void HandleMessage(string data, JavaScriptReplyProxy replyProxy)
    {
        using var document = JsonDocument.Parse(data);
        var root = document.RootElement;
        var type = root.GetProperty("type").GetString();
        var sentAtMs = root.GetProperty("sentAtMs").GetInt64();

        switch (type)
        {
            case "script.ready":
                SpikePlayback.ReplyProxy = replyProxy;
                SpikeLog.Info($"Bridge received script.ready after {SpikePlayback.ElapsedSince(sentAtMs)} ms");
                SpikePlayback.Echo(replyProxy, "script.ready", sentAtMs);
                break;
            case "bridge.ping":
                SpikePlayback.Echo(replyProxy, "bridge.ping", sentAtMs);
                break;
            case "player.state":
                var state = root.GetProperty("payload").GetProperty("state").GetString();
                SpikeLog.Info($"Bridge received player.state {state} after {SpikePlayback.ElapsedSince(sentAtMs)} ms");
                SpikePlayback.ReportPageState(state);
                break;
            default:
                SpikeLog.Info($"Bridge ignored message type {type}");
                break;
        }
    }
}
