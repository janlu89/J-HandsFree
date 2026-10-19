namespace JHandsFree.Domain;

/// <summary>
/// The answer of a <see cref="PlaybackSession"/> to a request: the states it moved between, or a rejection that changed nothing.
/// </summary>
public readonly record struct TransitionResult
{
    private TransitionResult(PlaybackState from, PlaybackState to, bool isValid)
    {
        From = from;
        To = to;
        IsValid = isValid;
    }

    public PlaybackState From { get; }

    public PlaybackState To { get; }

    public bool IsValid { get; }

    public static TransitionResult Valid(PlaybackState from, PlaybackState to) => new(from, to, true);

    public static TransitionResult Invalid(PlaybackState state) => new(state, state, false);
}
