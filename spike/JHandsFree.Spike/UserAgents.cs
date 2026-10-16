using System.Text.RegularExpressions;

namespace JHandsFree.Spike;

public static partial class UserAgents
{
    public static string For(UserAgentMode mode, string defaultUserAgent) => mode switch
    {
        UserAgentMode.Default => defaultUserAgent,
        UserAgentMode.WithoutWebViewMarker => WithoutWebViewMarker(defaultUserAgent),
        UserAgentMode.WithoutWebViewAndVersionMarkers => WithoutVersionMarker(WithoutWebViewMarker(defaultUserAgent)),
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null),
    };

    public static UserAgentMode After(UserAgentMode mode) => mode switch
    {
        UserAgentMode.Default => UserAgentMode.WithoutWebViewMarker,
        UserAgentMode.WithoutWebViewMarker => UserAgentMode.WithoutWebViewAndVersionMarkers,
        _ => UserAgentMode.Default,
    };

    private static string WithoutWebViewMarker(string userAgent) =>
        userAgent.Replace("; wv)", ")", StringComparison.Ordinal);

    private static string WithoutVersionMarker(string userAgent) =>
        VersionMarker().Replace(userAgent, string.Empty);

    [GeneratedRegex(@"Version/\d+(\.\d+)* ")]
    private static partial Regex VersionMarker();
}
