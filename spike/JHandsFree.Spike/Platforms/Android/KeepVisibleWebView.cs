using Android.Content;
using Android.Views;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace JHandsFree.Spike;

// Chromium pauses media when the WebView's window turns invisible, before any page script can react,
// so keeping playback alive in the background has to happen at the native view level.
public sealed class KeepVisibleWebView(WebViewHandler handler, Context context) : MauiWebView(handler, context)
{
    public bool KeepWindowVisible { get; set; }

    protected override void OnWindowVisibilityChanged(ViewStates visibility)
    {
        var reportedVisibility = KeepWindowVisible ? ViewStates.Visible : visibility;
        SpikeLog.Info($"Window visibility {visibility}, reported to Chromium as {reportedVisibility}");
        base.OnWindowVisibilityChanged(reportedVisibility);
    }
}
