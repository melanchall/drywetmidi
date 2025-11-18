using System.Text.Json;

namespace CITools.Tools
{
    internal static class ObsoleteApi
    {
        public record ObsoleteApiEntry(
            string Hint,
            string ObsoleteFromVersion,
            bool InLibrary,
            string RemovedFromVersion);

        public static Dictionary<string, ObsoleteApiEntry> GetStatus(string obsoleteDirectoryPath)
        {
            Console.WriteLine($"Reading obsolete API status...");
            var statusFilePath = Path.Combine(obsoleteDirectoryPath, "status.json");
            var json = File.ReadAllText(statusFilePath);
            var status = JsonSerializer.Deserialize<Dictionary<string, ObsoleteApiEntry>>(json);
            Console.WriteLine($"- found {status?.Count ?? 0} obsolete API entries");
            return status ?? [];
        }
    }
}
