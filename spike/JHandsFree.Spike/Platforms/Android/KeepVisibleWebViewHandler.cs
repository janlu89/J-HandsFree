using Android.Views;
using Microsoft.Maui.Handlers;
using AndroidWebView = Android.Webkit.WebView;

namespace JHandsFree.Spike;

public sealed class KeepVisibleWebViewHandler : WebViewHandler
{
    protected override AndroidWebView CreatePlatformView()
    {
        var platformView = new KeepVisibleWebView(this, Context)
        {
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent),
        };
        platformView.Settings.JavaScriptEnabled = true;
        platformView.Settings.DomStorageEnabled = true;
        platformView.Settings.SetSupportMultipleWindows(true);
        return platformView;
    }
}
