namespace JHandsFree.Domain;

/// <summary>
/// Whether an ad break can be skipped. It stays <see cref="Unknown"/> until the Skip button appears or the break ends.
/// </summary>
public enum AdKind
{
    Unknown,
    Skippable,
    NonSkippable,
}
