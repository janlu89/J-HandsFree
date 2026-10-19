using JHandsFree.UseCases.Ads;

namespace JHandsFree.UseCases.Player;

/// <summary>
/// Presses Skip with a native tap inside the target, because the player ignores script clicks (ADR 0013).
/// </summary>
public sealed record TapSkipButton : PlayerCommand
{
    public TapSkipButton(SkipTarget target) => Target = target;

    public SkipTarget Target
    {
        get;
        init => field = value ?? throw new ArgumentNullException(nameof(Target));
    }
}
