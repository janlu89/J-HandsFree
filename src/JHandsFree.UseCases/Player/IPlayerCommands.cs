namespace JHandsFree.UseCases.Player;

/// <summary>
/// The port to the YouTube page. The adapter sends each command to the page, or performs it natively when the page
/// cannot, as for the Skip tap.
/// </summary>
public interface IPlayerCommands
{
    ValueTask SendAsync(PlayerCommand command, CancellationToken cancellationToken);
}
