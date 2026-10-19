namespace JHandsFree.UseCases.Player;

/// <summary>
/// Asks the page for the Skip button's rectangle, which the page reports with the <c>ad.skipTarget</c> event.
/// </summary>
public sealed record LocateSkipButton : PlayerCommand;
