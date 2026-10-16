using AndroidCookieManager = Android.Webkit.CookieManager;
using AndroidLog = Android.Util.Log;
using AndroidWebSettings = Android.Webkit.WebSettings;
using AndroidWebStorage = Android.Webkit.WebStorage;
using AndroidWebView = Android.Webkit.WebView;

namespace JHandsFree.Spike;

public partial class MainPage : ContentPage
{
    private const string LogTag = "JHFSpike";
    private const string StartUrl = "https://m.youtube.com";

    private AndroidWebView? _nativeWebView;
    private string _defaultUserAgent = string.Empty;
    private UserAgentMode _userAgentMode = UserAgentMode.Default;

    public MainPage()
    {
        InitializeComponent();
    }

    protected override bool OnBackButtonPressed()
    {
        if (!Browser.CanGoBack)
        {
            return base.OnBackButtonPressed();
        }

        Browser.GoBack();
        return true;
    }

    private void OnBrowserHandlerChanged(object? sender, EventArgs e)
    {
        if (Browser.Handler?.PlatformView is not AndroidWebView nativeWebView)
        {
            return;
        }

        _nativeWebView = nativeWebView;
        _defaultUserAgent = AndroidWebSettings.GetDefaultUserAgent(nativeWebView.Context) ?? string.Empty;
        ConfigureWebView(nativeWebView);
        ApplyUserAgent();
        Browser.Source = StartUrl;
    }

    private void OnNextUserAgentClicked(object? sender, EventArgs e)
    {
        _userAgentMode = UserAgents.After(_userAgentMode);
        ApplyUserAgent();
        Browser.Reload();
    }

    private void OnClearDataClicked(object? sender, EventArgs e)
    {
        AndroidCookieManager.Instance?.RemoveAllCookies(null);
        AndroidCookieManager.Instance?.Flush();
        AndroidWebStorage.Instance?.DeleteAllData();
        _nativeWebView?.ClearCache(true);
        AndroidLog.Info(LogTag, "Cookies, web storage and cache cleared");
        Browser.Source = StartUrl;
    }

    private void OnBrowserNavigating(object? sender, WebNavigatingEventArgs e)
    {
        AndroidLog.Info(LogTag, $"Navigating [{_userAgentMode}] {WithoutQuery(e.Url)}");
    }

    private void OnBrowserNavigated(object? sender, WebNavigatedEventArgs e)
    {
        var address = WithoutQuery(e.Url);
        AddressLabel.Text = address;
        AndroidLog.Info(LogTag, $"Navigated [{_userAgentMode}] {e.Result} {address}");
    }

    private static void ConfigureWebView(AndroidWebView nativeWebView)
    {
        nativeWebView.Settings.JavaScriptEnabled = true;
        nativeWebView.Settings.DomStorageEnabled = true;
        AndroidCookieManager.Instance?.SetAcceptCookie(true);
        AndroidCookieManager.Instance?.SetAcceptThirdPartyCookies(nativeWebView, true);
    }

    private void ApplyUserAgent()
    {
        if (_nativeWebView is null)
        {
            return;
        }

        var userAgent = UserAgents.For(_userAgentMode, _defaultUserAgent);
        _nativeWebView.Settings.UserAgentString = userAgent;
        UserAgentLabel.Text = $"UA: {_userAgentMode}";
        AndroidLog.Info(LogTag, $"User agent [{_userAgentMode}] {userAgent}");
    }

    // Sign-in URLs can carry account hints and tokens in the query string; logs keep only the path.
    private static string WithoutQuery(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri.GetLeftPart(UriPartial.Path) : url;
}
