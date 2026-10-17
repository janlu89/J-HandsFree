using Android.Content;

namespace JHandsFree.Spike;

public sealed class ScreenStateReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent) =>
        SpikeJournal.Append(intent?.Action == Intent.ActionScreenOff ? "screen.off" : "screen.on");
}
