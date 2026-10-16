using AndroidLog = Android.Util.Log;

namespace JHandsFree.Spike;

public static class SpikeLog
{
    public const string Tag = "JHFSpike";

    public static void Info(string message) => AndroidLog.Info(Tag, message);
}
