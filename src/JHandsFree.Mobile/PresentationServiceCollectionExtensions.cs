using JHandsFree.UseCases.Playback;

namespace JHandsFree.Mobile;

internal static class PresentationServiceCollectionExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services) =>
        services
            .AddSingleton<IPlayerVisibility, DisconnectedPlayerVisibility>()
            .AddTransient<MainPage>();
}
