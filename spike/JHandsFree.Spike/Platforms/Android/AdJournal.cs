namespace JHandsFree.Spike;

// Persists every ad event and markup snapshot on the device, so ads can be collected over days without a cable.
public static class AdJournal
{
    private static string JournalPath => Path.Combine(FileSystem.AppDataDirectory, "ad-journal.jsonl");

    private static string SnapshotDirectory => Path.Combine(FileSystem.AppDataDirectory, "ad-snapshots");

    public static void Append(string type, string payloadJson)
    {
        var line = $$"""{"at":"{{Now():O}}","type":"{{type}}","payload":{{payloadJson}}}""";
        File.AppendAllText(JournalPath, line + "\n");
    }

    public static void SaveSnapshot(string reason, string html)
    {
        Directory.CreateDirectory(SnapshotDirectory);
        var fileName = $"{Now():yyyyMMdd-HHmmss-fff}-{reason}.html";
        File.WriteAllText(Path.Combine(SnapshotDirectory, fileName), html);
    }

    private static DateTimeOffset Now() => TimeProvider.System.GetLocalNow();
}
