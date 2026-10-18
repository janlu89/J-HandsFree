using Android.App;
using Android.Content.PM;

namespace JHandsFree.Mobile;

// .NET names Java wrappers after a hash of the namespace. A fixed name keeps launcher icons and shortcuts working if the namespace changes.
[Activity(
    Name = "com.janlu89.jhandsfree.MainActivity",
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public sealed class MainActivity : MauiAppCompatActivity
{
}
