namespace JHandsFree.Mobile;

internal static class PresentationServiceCollectionExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services) =>
        services.AddTransient<MainPage>();
}
