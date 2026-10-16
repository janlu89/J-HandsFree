namespace JHandsFree.Spike;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());
        window.Activated += (_, _) => SpikeLog.Info("Window activated");
        window.Deactivated += (_, _) => SpikeLog.Info("Window deactivated");
        window.Stopped += (_, _) => SpikeLog.Info("Window stopped: app in background");
        window.Resumed += (_, _) => SpikeLog.Info("Window resumed: app in foreground");
        return window;
    }
}
