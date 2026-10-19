namespace JHandsFree.UseCases.Playback;

/// <summary>
/// The port that decides whether the player page keeps running in the background (ADR 0012).
/// </summary>
public interface IPlayerVisibility
{
    void KeepVisible();

    void AllowHidden();
}
