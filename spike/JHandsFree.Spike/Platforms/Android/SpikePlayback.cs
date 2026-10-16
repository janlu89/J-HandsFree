using AndroidX.WebKit;

namespace JHandsFree.Spike;

// Spike shortcut: a static hand-off between the page bridge and the session player. The real app routes this through the mediator.
public static class SpikePlayback
{
    public static JavaScriptReplyProxy? ReplyProxy { get; set; }

    public static WebViewSessionPlayer? Player { get; set; }

    public static void PlayVideo() => SendCommand("player.play");

    public static void PauseVideo() => SendCommand("player.pause");

    public static void ReportPageState(string? state) => Player?.ReportPageState(state == "playing");

    public static void Echo(JavaScriptReplyProxy replyProxy, string echoOf, long echoOfSentAtMs) =>
        replyProxy.PostMessage($$$"""{"protocol":1,"type":"bridge.echo","sentAtMs":{{{NowMs()}}},"payload":{"echoOf":"{{{echoOf}}}","echoOfSentAtMs":{{{echoOfSentAtMs}}}}}""");

    public static long ElapsedSince(long sentAtMs) => NowMs() - sentAtMs;

    private static void SendCommand(string type)
    {
        if (ReplyProxy is null)
        {
            SpikeLog.Info($"No page bridge yet; {type} not sent");
            return;
        }

        ReplyProxy.PostMessage($$$"""{"protocol":1,"type":"{{{type}}}","sentAtMs":{{{NowMs()}}},"payload":{}}""");
        SpikeLog.Info($"Bridge sent {type}");
    }

    private static long NowMs() => TimeProvider.System.GetUtcNow().ToUnixTimeMilliseconds();
}
