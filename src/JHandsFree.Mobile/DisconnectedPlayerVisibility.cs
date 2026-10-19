using JHandsFree.UseCases.Playback;

namespace JHandsFree.Mobile;

/// <summary>
/// Ignores every request, because the app has no player page whose visibility it could change yet.
/// </summary>
internal sealed class DisconnectedPlayerVisibility : IPlayerVisibility
{
    public void KeepVisible()
    {
    }

    public void AllowHidden()
    {
    }
}
