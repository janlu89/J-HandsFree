namespace JHandsFree.Domain;

public sealed record VideoMetadata
{
    public required VideoId Id
    {
        get;
        init => field = value ?? throw new ArgumentNullException(nameof(Id));
    }

    public required string Title
    {
        get;
        init => field = value ?? throw new ArgumentNullException(nameof(Title));
    }

    public required string Channel
    {
        get;
        init => field = value ?? throw new ArgumentNullException(nameof(Channel));
    }

    /// <summary>
    /// The length of the video, or <see langword="null"/> when it is unknown, for example during a live stream.
    /// </summary>
    public TimeSpan? Duration
    {
        get;
        init => field = value is null || value > TimeSpan.Zero
            ? value
            : throw new ArgumentOutOfRangeException(nameof(Duration), value, "A known duration is longer than zero.");
    }
}
