using AndroidX.WebKit;
using AndroidCookieManager = Android.Webkit.CookieManager;
using AndroidWebSettings = Android.Webkit.WebSettings;
using AndroidWebStorage = Android.Webkit.WebStorage;
using AndroidWebView = Android.Webkit.WebView;

namespace JHandsFree.Spike;

public partial class MainPage : ContentPage
{
    private const string StartUrl = "https://m.youtube.com";
    private static readonly string[] _scriptOrigins = ["https://m.youtube.com"];

    private AndroidWebView? _nativeWebView;
    private string _defaultUserAgent = string.Empty;
    private UserAgentMode _userAgentMode = UserAgentMode.Default;
    private bool _isDocumentStartScriptSupported;
    private string _shieldSource = string.Empty;
    private IScriptHandler? _shieldScript;

    public MainPage()
    {
        InitializeComponent();
    }

    private string ShieldState => _isDocumentStartScriptSupported ? (_shieldScript is null ? "off" : "on") : "unsupported";

    protected override bool OnBackButtonPressed()
    {
        if (!Browser.CanGoBack)
        {
            return base.OnBackButtonPressed();
        }

        Browser.GoBack();
        return true;
    }

    private async void OnBrowserHandlerChanged(object? sender, EventArgs e)
    {
        if (Browser.Handler?.PlatformView is not AndroidWebView nativeWebView)
        {
            return;
        }

        _nativeWebView = nativeWebView;
        _defaultUserAgent = AndroidWebSettings.GetDefaultUserAgent(nativeWebView.Context) ?? string.Empty;
        ConfigureWebView(nativeWebView);
        ApplyUserAgent();
        await PrepareDocumentStartScriptsAsync(nativeWebView);
        UpdateStatus();
        Browser.Source = StartUrl;
    }

    private void OnNextUserAgentClicked(object? sender, EventArgs e)
    {
        _userAgentMode = UserAgents.After(_userAgentMode);
        ApplyUserAgent();
        UpdateStatus();
        Browser.Reload();
    }

    private void OnShieldClicked(object? sender, EventArgs e)
    {
        if (_nativeWebView is null || !_isDocumentStartScriptSupported)
        {
            return;
        }

        if (_shieldScript is null)
        {
            _shieldScript = WebViewCompat.AddDocumentStartJavaScript(_nativeWebView, _shieldSource, _scriptOrigins);
        }
        else
        {
            _shieldScript.Remove();
            _shieldScript = null;
        }

        SpikeLog.Info($"Visibility shield {ShieldState}");
        UpdateStatus();
        Browser.Reload();
    }

    private void OnClearDataClicked(object? sender, EventArgs e)
    {
        AndroidCookieManager.Instance?.RemoveAllCookies(null);
        AndroidCookieManager.Instance?.Flush();
        AndroidWebStorage.Instance?.DeleteAllData();
        _nativeWebView?.ClearCache(true);
        SpikeLog.Info("Cookies, web storage and cache cleared");
        Browser.Source = StartUrl;
    }

    private void OnBrowserNavigating(object? sender, WebNavigatingEventArgs e)
    {
        SpikeLog.Info($"Navigating [{_userAgentMode}] {WithoutQuery(e.Url)}");
    }

    private void OnBrowserNavigated(object? sender, WebNavigatedEventArgs e)
    {
        var address = WithoutQuery(e.Url);
        AddressLabel.Text = address;
        SpikeLog.Info($"Navigated [{_userAgentMode}] {e.Result} {address}");
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
        SpikeLog.Info($"User agent [{_userAgentMode}] {userAgent}");
    }

    private async Task PrepareDocumentStartScriptsAsync(AndroidWebView nativeWebView)
    {
        _isDocumentStartScriptSupported = WebViewFeature.IsFeatureSupported(WebViewFeature.DocumentStartScript);
        SpikeLog.Info($"DOCUMENT_START_SCRIPT supported: {_isDocumentStartScriptSupported}");
        if (!_isDocumentStartScriptSupported)
        {
            return;
        }

        var probeSource = await ReadPackageFileAsync("media-probe.js");
        WebViewCompat.AddDocumentStartJavaScript(nativeWebView, probeSource, _scriptOrigins);
        _shieldSource = await ReadPackageFileAsync("visibility-shield.js");
    }

    private void UpdateStatus()
    {
        StatusLabel.Text = $"UA: {_userAgentMode}\nShield: {ShieldState}";
    }

    private static async Task<string> ReadPackageFileAsync(string fileName)
    {
        await using var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    // Sign-in URLs can carry account hints and tokens in the query string; logs keep only the path.
    private static string WithoutQuery(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri.GetLeftPart(UriPartial.Path) : url;
}
