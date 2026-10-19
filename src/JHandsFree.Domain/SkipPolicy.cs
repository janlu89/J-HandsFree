namespace JHandsFree.Domain;

public sealed record SkipPolicy
{
    public static TimeSpan MaximumDelay { get; } = TimeSpan.FromSeconds(10);

    public static SkipPolicy Default { get; } = new() { IsEnabled = true, Delay = TimeSpan.Zero };

    public required bool IsEnabled { get; init; }

    /// <summary>
    /// How long to wait after the Skip button appears before pressing it, from zero to <see cref="MaximumDelay"/>.
    /// </summary>
    public required TimeSpan Delay
    {
        get;
        init => field = value >= TimeSpan.Zero && value <= MaximumDelay
            ? value
            : throw new ArgumentOutOfRangeException(nameof(Delay), value, $"The delay is between zero and {MaximumDelay.TotalSeconds} seconds.");
    }

    /// <summary>
    /// Whether to press Skip in this ad break: only when the policy is enabled and the break is skippable.
    /// </summary>
    public bool ShouldSkip(AdBreak adBreak)
    {
        ArgumentNullException.ThrowIfNull(adBreak);
        return IsEnabled && adBreak.Kind == AdKind.Skippable;
    }
}
