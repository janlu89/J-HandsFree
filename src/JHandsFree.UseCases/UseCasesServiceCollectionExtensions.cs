using JHandsFree.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace JHandsFree.UseCases;

public static class UseCasesServiceCollectionExtensions
{
    public static IServiceCollection AddUseCases(this IServiceCollection services) =>
        services
            .AddSingleton(TimeProvider.System)
            .AddSingleton(PauseTimeoutPolicy.Default);
}
