using JHandsFree.UseCases.Playback;
using JHandsFree.UseCases.Player;
using Microsoft.Extensions.DependencyInjection;

namespace JHandsFree.Infrastructure.Android;

public static class AndroidInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddAndroidInfrastructure(this IServiceCollection services) =>
        services
            .AddSingleton<IPlayerCommands, DisconnectedPlayerCommands>()
            .AddSingleton<ISystemMediaSession, DisconnectedMediaSession>();
}
