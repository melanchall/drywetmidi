using System.Text;
using System.Text.Json;

namespace CITools.Tools
{
    [Tool("GenerateDocsObsoleteApiSection")]
    internal sealed class GenerateDocsObsoleteApiSectionTool : ITool
    {
        private record ObsoleteApiEntry(
            string Hint,
            string ObsoleteFromVersion,
            bool InLibrary,
            string RemovedFromVersion);

        public void Execute(string[] args)
        {
            var obsoleteDirectoryPath = args[0];
            if (!Directory.Exists(obsoleteDirectoryPath))
                throw new InvalidOperationException($"Obsolete API directory not found at path: '{obsoleteDirectoryPath}'.");

            var status = ObsoleteApi.GetStatus(obsoleteDirectoryPath);

            Console.WriteLine($"Reading template file...");
            var templateFilePath = Path.Combine(obsoleteDirectoryPath, "template.md");
            var template = File.ReadAllText(templateFilePath);
            Console.WriteLine($"- read");

            var obsoleteApiSectionBuilder = new StringBuilder();

            foreach (var (id, entry) in status)
            {
                Console.WriteLine($"Generating info for '{id}'...");

                var idDirectory = Path.Combine(obsoleteDirectoryPath, id);
                Console.WriteLine($"- ID directory '{idDirectory}'");

                var section = template.Replace("$ID$", id);

                var inLibrary = entry.InLibrary;
                if (!inLibrary)
                {
                    var removedFromVersion = entry.RemovedFromVersion;
                    section = section.Replace("$REMOVED$", $"> [!WARNING]{Environment.NewLine}> API removed from the library by {removedFromVersion} release.");
                    Console.WriteLine($"- removed by {removedFromVersion}");
                }
                else
                {
                    section = section.Replace("$REMOVED$", string.Empty);
                }

                var obsoleteFromVersion = entry.ObsoleteFromVersion;
                section = section.Replace("$OBSOLETE_FROM_VERSION$", obsoleteFromVersion);
                Console.WriteLine($"- obsolete from {obsoleteFromVersion}");

                var description = File.ReadAllText(Path.Combine(idDirectory, "description.md"));
                section = section.Replace("$DESCRIPTION$", description);
                Console.WriteLine($"- description added");

                var oldApiContent = File.ReadAllText(Path.Combine(idDirectory, "old.md"));
                section = section.Replace("$OLD_API$", oldApiContent);
                Console.WriteLine($"- old API added");

                var newApiContent = File.ReadAllText(Path.Combine(idDirectory, "new.md"));
                section = section.Replace("$NEW_API$", newApiContent);
                Console.WriteLine($"- new API added");

                obsoleteApiSectionBuilder.AppendLine();
                obsoleteApiSectionBuilder.AppendLine(section);

                Console.WriteLine($"- generated");
            }

            Console.WriteLine($"Writing overview file...");
            var overviewFilePath = Path.Combine(obsoleteDirectoryPath, "obsolete.md");
            var overviewContent = File.ReadAllText(overviewFilePath);
            overviewContent = overviewContent.Replace("$OBSOLETE_API$", obsoleteApiSectionBuilder.ToString());
            File.WriteAllText(overviewFilePath, overviewContent.Trim());
            Console.WriteLine($"- written");
        }
    }
}
