using AndroidWebView = Android.Webkit.WebView;

namespace JHandsFree.Spike;

// Spike shortcut: a static hand-off between the page and the session player. The real app routes this through the bridge and the mediator.
public static class SpikePlayback
{
    public static AndroidWebView? WebView { get; set; }

    public static void PlayVideo() => RunOnVideo("play");

    public static void PauseVideo() => RunOnVideo("pause");

    private static void RunOnVideo(string method) =>
        WebView?.EvaluateJavascript($"document.querySelector('video')?.{method}();", null);
}
