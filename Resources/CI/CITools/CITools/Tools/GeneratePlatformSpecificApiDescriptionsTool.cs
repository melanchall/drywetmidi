using System.Text.RegularExpressions;

namespace CITools.Tools
{
    [Tool("GeneratePlatformSpecificApiDescriptions")]
    internal sealed class GeneratePlatformSpecificApiDescriptionsTool : ITool
    {
        private const string AdvancedWindowsApiMinVersion = "11 xxxx";

        public void Execute(string[] args)
        {
            if (args.Length < 2)
                throw new ArgumentException("Invalid number of arguments. Expected 2 arguments: path to the MD files directory and path to the CS files directory.");

            var mdFilesDirectoryPath = args[0];
            if (!Directory.Exists(mdFilesDirectoryPath))
                throw new DirectoryNotFoundException($"MD files directory not found at path: '{mdFilesDirectoryPath}'.");

            var csFilesDirectoryPath = args[1];
            if (!Directory.Exists(csFilesDirectoryPath))
                throw new DirectoryNotFoundException($"CS files directory not found at path: '{csFilesDirectoryPath}'.");

            ProcessMdFiles(mdFilesDirectoryPath);
            ProcessCsFiles(csFilesDirectoryPath);
        }

        private static void ProcessMdFiles(string mdFilesDirectoryPath)
        {
            Console.WriteLine($"Processing MD files in directory '{mdFilesDirectoryPath}'...");

            var mdFilesPaths = Directory.GetFiles(mdFilesDirectoryPath, "*.md", SearchOption.AllDirectories);
            Console.WriteLine($"    found {mdFilesPaths.Length} files");

            foreach (var mdFile in mdFilesPaths)
            {
                Console.WriteLine($"Processing '{mdFile}'...");

                Console.WriteLine("    reading content...");
                var content = File.ReadAllText(mdFile);
                Console.WriteLine("        read");

                Console.WriteLine("    generating OS-specific API descriptions...");
                var osSpecificGeneratedContent = Regex.Replace(
                    content,
                    @"\<os\-specific\-api (.+?)\/\>",
                    m =>
                    {
                        var parts = m.Groups[1].Value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        var partsString = parts.Length == 1
                            ? parts[0]
                            : (parts.Length == 2 ? $"{parts[0]} and {parts[1]}" : string.Join(", ", parts.Take(parts.Length - 1)) + $" and {parts.Last()}");
                        return $"{partsString} can be used on macOS and Windows only. Attempt to use this API on other systems will result in an exception. Also please note that this API is not available in the [nativeless version](xref:a_develop_nativeless) of the library. You may also want to see the [Supported OS](xref:a_develop_supported_os) article.";
                    });

                var osSpecificApiDescriptionsGenerated = osSpecificGeneratedContent != content;
                if (osSpecificApiDescriptionsGenerated)
                    Console.WriteLine("        generated");
                else
                    Console.WriteLine("        no changes");

                Console.WriteLine("    generating advanced Windows API descriptions...");
                var advancedWindowsGeneratedContent = Regex.Replace(
                    osSpecificGeneratedContent,
                    @"\<advanced\-windows\-api\/\>",
                    m => $"To use this API on Windows you need Windows {AdvancedWindowsApiMinVersion} or higher.");

                var advancedWindowsApiDescriptionsGenerated = advancedWindowsGeneratedContent != osSpecificGeneratedContent;
                if (advancedWindowsApiDescriptionsGenerated)
                    Console.WriteLine("        generated");
                else
                    Console.WriteLine("        no changes");

                if (osSpecificApiDescriptionsGenerated || advancedWindowsApiDescriptionsGenerated)
                {
                    Console.WriteLine($"    writing content...");
                    File.WriteAllText(mdFile, advancedWindowsGeneratedContent);
                    Console.WriteLine("        written");
                }
            }
        }

        private static void ProcessCsFiles(string csFilesDirectoryPath)
        {
            Console.WriteLine($"Processing CS files in directory '{csFilesDirectoryPath}'...");

            var csFilesPaths = Directory.GetFiles(csFilesDirectoryPath, "*.cs", SearchOption.AllDirectories);
            Console.WriteLine($"    found {csFilesPaths.Length} files");

            foreach (var csFile in csFilesPaths)
            {
                Console.WriteLine($"Processing '{csFile}'...");

                Console.WriteLine("    reading content...");
                var content = File.ReadAllText(csFile);
                Console.WriteLine("        read");

                Console.WriteLine("    generating OS-specific API descriptions...");
                var osSpecificGeneratedContent = Regex.Replace(
                    content,
                    @"\<os\-specific\-api\/\>",
                    m => "<p>This API can be used on macOS and Windows only. Attempt to use it on other systems will result in an exception. Also please note that this API is not available in the <see href=\"xref:a_develop_nativeless\">nativeless version</see> of the library. You may also want to see the <see href=\"xref:a_develop_supported_os\">Supported OS</see> article.</p>");

                var osSpecificApiDescriptionsGenerated = osSpecificGeneratedContent != content;
                if (osSpecificApiDescriptionsGenerated)
                    Console.WriteLine("        generated");
                else
                    Console.WriteLine("        no changes");

                Console.WriteLine("    generating advanced Windows API descriptions...");
                var advancedWindowsGeneratedContent = Regex.Replace(
                    osSpecificGeneratedContent,
                    @"\<advanced\-windows\-api\/\>",
                    m => $"<p>To use this API on Windows you need Windows {AdvancedWindowsApiMinVersion} or higher.</p>");

                var advancedWindowsApiDescriptionsGenerated = advancedWindowsGeneratedContent != osSpecificGeneratedContent;
                if (advancedWindowsApiDescriptionsGenerated)
                    Console.WriteLine("        generated");
                else
                    Console.WriteLine("        no changes");

                if (osSpecificApiDescriptionsGenerated || advancedWindowsApiDescriptionsGenerated)
                {
                    Console.WriteLine($"    writing content...");
                    File.WriteAllText(csFile, advancedWindowsGeneratedContent);
                    Console.WriteLine("        written");
                }
            }
        }
    }
}
