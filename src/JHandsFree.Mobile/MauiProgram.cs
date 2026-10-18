using JHandsFree.Infrastructure.Android;
using JHandsFree.Infrastructure.PlayerProtocol;
using JHandsFree.UseCases;

namespace JHandsFree.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services
            .AddUseCases()
            .AddPlayerProtocol()
            .AddAndroidInfrastructure()
            .AddMessaging()
            .AddPresentation();

        // A wiring error stops the app at startup instead of at first use, also in trimmed Release builds.
        builder.ConfigureContainer(new DefaultServiceProviderFactory(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true,
        }));

        return builder.Build();
    }
}
