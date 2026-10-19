namespace JHandsFree.UseCases.Player;

/// <summary>
/// A command for the YouTube page, sent through the player commands port. It leaves the process, so it is not a
/// mediator message.
/// </summary>
public abstract record PlayerCommand;
