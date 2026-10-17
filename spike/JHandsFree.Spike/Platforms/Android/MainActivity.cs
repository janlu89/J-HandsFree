using Android.App;
using Android.Content.PM;
using Android.OS;

namespace JHandsFree.Spike;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SpikeJournal.Append("activity.created", $$"""{"restoredFromSavedState":{{(savedInstanceState is not null ? "true" : "false")}}}""");
    }

    protected override void OnStart()
    {
        base.OnStart();
        SpikeJournal.Append("activity.started");
    }

    protected override void OnStop()
    {
        SpikeJournal.Append("activity.stopped");
        base.OnStop();
    }

    protected override void OnDestroy()
    {
        var finishing = IsFinishing ? "true" : "false";
        var changingConfigurations = IsChangingConfigurations ? "true" : "false";
        SpikeJournal.Append("activity.destroyed", $$"""{"finishing":{{finishing}},"changingConfigurations":{{changingConfigurations}}}""");
        base.OnDestroy();
    }
}
