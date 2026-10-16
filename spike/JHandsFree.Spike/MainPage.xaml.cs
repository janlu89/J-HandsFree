using Android.Content;
using Android.Runtime;
using AndroidX.Core.Content;
using AndroidX.Media3.Session;
using AndroidX.WebKit;
using Google.Common.Util.Concurrent;
using AndroidCookieManager = Android.Webkit.CookieManager;
using AndroidWebSettings = Android.Webkit.WebSettings;
using AndroidWebStorage = Android.Webkit.WebStorage;
using AndroidWebView = Android.Webkit.WebView;

namespace JHandsFree.Spike;

public partial class MainPage : ContentPage
{
    private const string StartUrl = "https://m.youtube.com";
    private const string OriginCheckUrl = "https://origin-check.invalid/";
    private const string OriginCheckPage = "<script>console.info('[jhf-origin] ' + location.origin + ' typeof jhf=' + typeof jhf);</script>";
    private static readonly string[] _scriptOrigins = [PageBridge.AllowedOrigin];

    private AndroidWebView? _nativeWebView;
    private string _defaultUserAgent = string.Empty;
    private UserAgentMode _userAgentMode = UserAgentMode.Default;
    private bool _isDocumentStartScriptSupported;
    private string _shieldSource = string.Empty;
    private IScriptHandler? _shieldScript;
    private MediaController? _mediaController;

    public MainPage()
    {
        InitializeComponent();
    }

    private string ShieldState => _isDocumentStartScriptSupported ? (_shieldScript is null ? "off" : "on") : "unsupported";

    private string NativeState => _nativeWebView is KeepVisibleWebView view ? (view.KeepWindowVisible ? "on" : "off") : "unavailable";

    private string SessionState => _mediaController is null ? "off" : (_mediaController.PlayWhenReady ? "playing" : "paused");

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
        AddPageBridge(nativeWebView);
        await PrepareDocumentStartScriptsAsync(nativeWebView);
        UpdateStatus();
        await CheckBridgeOriginRestrictionAsync(nativeWebView);
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

    private void OnNativeClicked(object? sender, EventArgs e)
    {
        if (_nativeWebView is not KeepVisibleWebView view)
        {
            return;
        }

        view.KeepWindowVisible = !view.KeepWindowVisible;
        SpikeLog.Info($"Native keep-visible {NativeState}");
        UpdateStatus();
    }

    private async void OnSessionClicked(object? sender, EventArgs e)
    {
        if (_mediaController is not null)
        {
            ToggleSessionPlayback(_mediaController);
            return;
        }

        var permission = await Permissions.RequestAsync<Permissions.PostNotifications>();
        SpikeLog.Info($"Notification permission: {permission}");
        ConnectMediaController();
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
        _nativeWebView?.EvaluateJavascript("location.origin + ' typeof jhf=' + typeof jhf", new BridgeAvailabilityLogger());
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

    private static void AddPageBridge(AndroidWebView nativeWebView)
    {
        var isSupported = WebViewFeature.IsFeatureSupported(WebViewFeature.WebMessageListener);
        SpikeLog.Info($"WEB_MESSAGE_LISTENER supported: {isSupported}");
        if (isSupported)
        {
            WebViewCompat.AddWebMessageListener(nativeWebView, PageBridge.ObjectName, _scriptOrigins, new PageBridge());
        }
    }

    // A page served from an origin outside the allowed rules must not see the injected bridge object.
    private static async Task CheckBridgeOriginRestrictionAsync(AndroidWebView nativeWebView)
    {
        nativeWebView.LoadDataWithBaseURL(OriginCheckUrl, OriginCheckPage, "text/html", "utf-8", null);
        await Task.Delay(TimeSpan.FromSeconds(1));
    }

    private async Task PrepareDocumentStartScriptsAsync(AndroidWebView nativeWebView)
    {
        _isDocumentStartScriptSupported = WebViewFeature.IsFeatureSupported(WebViewFeature.DocumentStartScript);
        SpikeLog.Info($"DOCUMENT_START_SCRIPT supported: {_isDocumentStartScriptSupported}");
        if (!_isDocumentStartScriptSupported)
        {
            return;
        }

        var bridgeSource = await ReadPackageFileAsync("bridge-client.js");
        WebViewCompat.AddDocumentStartJavaScript(nativeWebView, bridgeSource, _scriptOrigins);
        var probeSource = await ReadPackageFileAsync("media-probe.js");
        WebViewCompat.AddDocumentStartJavaScript(nativeWebView, probeSource, _scriptOrigins);
        _shieldSource = await ReadPackageFileAsync("visibility-shield.js");
    }

    private void ConnectMediaController()
    {
        var context = Platform.AppContext;
        var serviceComponent = new ComponentName(context, Java.Lang.Class.FromType(typeof(PlaybackService)));
        var controllerFuture = new MediaController.Builder(context, new SessionToken(context, serviceComponent)).BuildAsync();
        if (controllerFuture is null)
        {
            SpikeLog.Info("Media controller could not be built");
            return;
        }

        controllerFuture.AddListener(new Java.Lang.Runnable(() => OnMediaControllerConnected(controllerFuture)), ContextCompat.GetMainExecutor(context));
    }

    private void OnMediaControllerConnected(IListenableFuture controllerFuture)
    {
        var controller = controllerFuture.Get()?.JavaCast<MediaController>();
        if (controller is null)
        {
            SpikeLog.Info("Media controller connection failed");
            return;
        }

        _mediaController = controller;
        SpikeLog.Info("Media controller connected");
        controller.Play();
        UpdateStatus();
    }

    private void ToggleSessionPlayback(MediaController controller)
    {
        if (controller.PlayWhenReady)
        {
            controller.Pause();
        }
        else
        {
            controller.Play();
        }

        UpdateStatus();
    }

    private void UpdateStatus()
    {
        StatusLabel.Text = $"UA: {_userAgentMode} | Shield: {ShieldState} | Native: {NativeState} | Session: {SessionState}";
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

    private sealed class BridgeAvailabilityLogger : Java.Lang.Object, Android.Webkit.IValueCallback
    {
        public void OnReceiveValue(Java.Lang.Object? value) => SpikeLog.Info($"Bridge object check: {value}");
    }
}
