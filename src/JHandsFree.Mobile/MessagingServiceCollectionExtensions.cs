using JHandsFree.UseCases.Playback;
using Mediator;

namespace JHandsFree.Mobile;

internal static class MessagingServiceCollectionExtensions
{
    // The source generator reads these options at compile time, so every value must be a constant.
    // Messages and handlers live only in UseCases (ADR 0011); any of its types identifies the assembly.
    public static IServiceCollection AddMessaging(this IServiceCollection services) =>
        services.AddMediator((MediatorOptions options) =>
        {
            options.Assemblies = [typeof(PlayerStateChanged)];
            options.ServiceLifetime = ServiceLifetime.Singleton;
            options.GenerateTypesAsInternal = true;
        });
}
