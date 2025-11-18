using System.Text.RegularExpressions;

namespace CITools.Tools
{
    [Tool("GenerateObsoleteApiHints")]
    internal sealed class GenerateObsoleteApiHintsTool : ITool
    {
        public void Execute(string[] args)
        {
            var obsoleteDirectoryPath = args[0];
            if (!Directory.Exists(obsoleteDirectoryPath))
                throw new InvalidOperationException($"Obsolete API directory not found at path: '{obsoleteDirectoryPath}'.");

            var codeFilesRootDirectoryPath = args[1];
            if (!Directory.Exists(codeFilesRootDirectoryPath))
                throw new InvalidOperationException($"Code files root directory not found at path: '{codeFilesRootDirectoryPath}'.");

            var status = ObsoleteApi.GetStatus(obsoleteDirectoryPath);

            Console.WriteLine("Inserting hints into Obsolete declarations...");

            var codeFiles = Directory.GetFiles(codeFilesRootDirectoryPath, "*.cs", SearchOption.AllDirectories);
            Console.WriteLine($"Found {codeFiles.Length} code files to process.");

            foreach (var codeFilePath in codeFiles)
            {
                Console.WriteLine($"Searching for declaration in '{codeFilePath}'...");

                var content = File.ReadAllText(codeFilePath);

                if (!Regex.IsMatch(content, @"\[Obsolete\("".+""\)\]"))
                {
                    Console.WriteLine("- no Obsolete declarations found, skipping...");
                    continue;
                }

                foreach (var (id, entry) in status)
                {
                    var hint = entry.Hint;
                    var link = $"https://melanchall.github.io/drywetmidi/obsolete/obsolete.html#{id.ToLower()}";
                    var newContent = content.Replace($@"[Obsolete(""{id}"")]", $@"[Obsolete(""{id}: {hint} More info: {link}."")]");
                    if (newContent != content)
                    {
                        Console.WriteLine($"-- hint added: '{id}'");
                        content = newContent;
                    }
                }

                Console.WriteLine("- writing updated content...");
                File.WriteAllText(codeFilePath, content);
                Console.WriteLine("- done");
            }
        }
    }
}
