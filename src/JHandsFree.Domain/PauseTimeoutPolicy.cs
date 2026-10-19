namespace JHandsFree.Domain;

/// <summary>
/// How long a pause may last before background playback is released. The default matches the ten minutes for which
/// Media3 keeps the foreground service after a pause.
/// </summary>
public sealed record PauseTimeoutPolicy
{
    public static PauseTimeoutPolicy Default { get; } = new() { Timeout = TimeSpan.FromMinutes(10) };

    public required TimeSpan Timeout
    {
        get;
        init => field = value > TimeSpan.Zero
            ? value
            : throw new ArgumentOutOfRangeException(nameof(Timeout), value, "The timeout is longer than zero.");
    }

    public bool HasExpired(DateTimeOffset pausedAt, DateTimeOffset now) => now - pausedAt >= Timeout;
}
