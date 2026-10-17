using System.Globalization;
using System.Text.Json;
using AndroidX.WebKit;
using AndroidWebView = Android.Webkit.WebView;

namespace JHandsFree.Spike;

// Spike version of the AdSkipCoordinator: the page reports, the app decides whether and how to press Skip.
public static class AdSkipper
{
    public static event Action? Changed;

    public static SkipMode Mode { get; set; } = SkipMode.Tap;

    public static int AdsStarted { get; private set; }

    public static int SkipButtonsSeen { get; private set; }

    public static int SkipButtonsGone { get; private set; }

    public static void Handle(string type, JsonElement payload, AndroidWebView? view, JavaScriptReplyProxy replyProxy)
    {
        if (type == "spike.snapshot")
        {
            AdJournal.SaveSnapshot(payload.GetProperty("reason").GetString() ?? "unknown", payload.GetProperty("html").GetString() ?? string.Empty);
            return;
        }

        AdJournal.Append(type, payload.GetRawText());
        SpikeLog.Info($"Ad event {type} {payload.GetRawText()}");
        Apply(type, payload, view, replyProxy);
        Changed?.Invoke();
    }

    private static void Apply(string type, JsonElement payload, AndroidWebView? view, JavaScriptReplyProxy replyProxy)
    {
        switch (type)
        {
            case "ad.started":
                AdsStarted++;
                break;
            case "ad.skippable":
                SkipButtonsSeen++;
                RequestSkip(replyProxy);
                break;
            case "ad.skipTarget":
                TapSkipTarget(payload, view);
                break;
            case "spike.skipButtonGone":
                SkipButtonsGone++;
                break;
            case "spike.setSkipMode" when Enum.TryParse<SkipMode>(payload.GetProperty("mode").GetString(), out var mode):
                // Test hook for the simulated-ad check; the real app never lets the page change a setting.
                Mode = mode;
                break;
        }
    }

    private static void RequestSkip(JavaScriptReplyProxy replyProxy)
    {
        var modeArgument = Mode switch
        {
            SkipMode.Click => "click",
            SkipMode.Tap => "locate",
            _ => null,
        };
        if (modeArgument is null)
        {
            return;
        }

        SpikePlayback.Send(replyProxy, "ad.skip", $$"""{"mode":"{{modeArgument}}"}""");
        AdJournal.Append("app.skipRequested", $$"""{"mode":"{{modeArgument}}"}""");
    }

    private static void TapSkipTarget(JsonElement payload, AndroidWebView? view)
    {
        if (view is null)
        {
            return;
        }

        var pixelRatio = payload.GetProperty("devicePixelRatio").GetSingle();
        var x = (payload.GetProperty("x").GetSingle() + (payload.GetProperty("width").GetSingle() / 2)) * pixelRatio;
        var y = (payload.GetProperty("y").GetSingle() + (payload.GetProperty("height").GetSingle() / 2)) * pixelRatio;
        var handled = NativeTap.Tap(view, x, y);
        var position = string.Create(CultureInfo.InvariantCulture, $"\"x\":{x:F0},\"y\":{y:F0}");
        AdJournal.Append("app.nativeTap", $"{{{position},\"handled\":{(handled ? "true" : "false")}}}");
        SpikeLog.Info($"Native tap at {position}, handled {handled}");
    }
}
