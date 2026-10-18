namespace JHandsFree.Domain;

/// <summary>
/// One ad break, whatever the number of ads in it.
/// </summary>
public sealed record AdBreak
{
    private AdBreak(DateTimeOffset startedAt, AdKind kind)
    {
        StartedAt = startedAt;
        Kind = kind;
    }

    public DateTimeOffset StartedAt { get; }

    public AdKind Kind { get; }

    public static AdBreak Start(DateTimeOffset startedAt) => new(startedAt, AdKind.Unknown);

    public AdBreak MarkSkippable() =>
        Kind switch
        {
            AdKind.Unknown => new AdBreak(StartedAt, AdKind.Skippable),
            AdKind.Skippable => this,
            _ => throw new InvalidOperationException("An ad break that ended without a Skip button cannot become skippable."),
        };

    /// <summary>
    /// Returns the break with its final kind: a break whose kind is still unknown when it ends had no Skip button.
    /// </summary>
    public AdBreak End() => Kind == AdKind.Unknown ? new AdBreak(StartedAt, AdKind.NonSkippable) : this;
}
