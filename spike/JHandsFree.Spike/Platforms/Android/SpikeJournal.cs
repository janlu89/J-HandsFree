namespace JHandsFree.Spike;

// Persists spike events and markup snapshots on the device, so data can be collected over days without a cable.
public static class SpikeJournal
{
    private static readonly Lock _writeLock = new();

    private static string JournalPath => Path.Combine(FileSystem.AppDataDirectory, "spike-journal.jsonl");

    private static string SnapshotDirectory => Path.Combine(FileSystem.AppDataDirectory, "ad-snapshots");

    public static void Append(string type) => Append(type, "{}");

    public static void Append(string type, string payloadJson)
    {
        var line = $$"""{"at":"{{Now():O}}","type":"{{type}}","payload":{{payloadJson}}}""";
        lock (_writeLock)
        {
            File.AppendAllText(JournalPath, line + "\n");
        }
    }

    public static void SaveSnapshot(string reason, string html)
    {
        Directory.CreateDirectory(SnapshotDirectory);
        var fileName = $"{Now():yyyyMMdd-HHmmss-fff}-{reason}.html";
        File.WriteAllText(Path.Combine(SnapshotDirectory, fileName), html);
    }

    private static DateTimeOffset Now() => TimeProvider.System.GetLocalNow();
}
