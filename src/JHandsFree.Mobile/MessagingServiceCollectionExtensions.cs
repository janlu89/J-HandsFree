using Mediator;

namespace JHandsFree.Mobile;

internal static class MessagingServiceCollectionExtensions
{
    // The source generator reads these options at compile time, so every value must be a constant.
    public static IServiceCollection AddMessaging(this IServiceCollection services) =>
        services.AddMediator((MediatorOptions options) =>
        {
            options.ServiceLifetime = ServiceLifetime.Singleton;
            options.GenerateTypesAsInternal = true;
        });
}
