using JHandsFree.UseCases.Player;

namespace JHandsFree.Infrastructure.Android;

/// <summary>
/// Drops every command, because the app has no player page to send it to yet.
/// </summary>
internal sealed class DisconnectedPlayerCommands : IPlayerCommands
{
    public ValueTask SendAsync(PlayerCommand command, CancellationToken cancellationToken) => ValueTask.CompletedTask;
}
