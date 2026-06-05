using System.Text.RegularExpressions;

namespace CITools.Tools
{
    [Tool("Setup404")]
    internal sealed class Setup404Tool : ITool
    {
        private const string BaseUrl = "https://melanchall.github.io/drywetmidi/";

        private static readonly Dictionary<string, string> Replacements = new()
        {
            ["href=\"images/"] = $"href=\"{BaseUrl}images/",
            ["href=\"styles/"] = $"href=\"{BaseUrl}styles/",
            ["src=\"styles/"] = $"src=\"{BaseUrl}styles/",
            ["src=\"images/"] = $"src=\"{BaseUrl}images/",
        };

        public void Execute(string[] args)
        {
            if (args.Length < 1)
                throw new ArgumentException("Invalid number of arguments. Expected 1 argument: path to the 404 page file.");

            var filePath = args[0];
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"404 page file not found at path: '{filePath}'.");

            Console.WriteLine($"Reading content of '{filePath}'...");
            var content = File.ReadAllText(filePath);
            Console.WriteLine($"- read");

            foreach (var replacement in Replacements)
            {
                Console.WriteLine($"Replacing '{replacement.Key}' with '{replacement.Value}'...");

                var newContent = content.Replace(replacement.Key, replacement.Value);
                if ( newContent != content)
                {
                    content = newContent;
                    Console.WriteLine($"- replaced");
                }
            }

            Console.WriteLine("Fixing docfx navigation...");
            content = Regex.Replace(content, @"\<meta property=""docfx:navrel.+?\>", $@"<meta property=""docfx:navrel"" content=""{BaseUrl}toc.html"">");
            Console.WriteLine("- fixed");

            Console.WriteLine("Removing docfx meta elements...");
            content = Regex.Replace(content, @"\<meta property=""docfx:tocrel.+?\>", string.Empty);
            content = Regex.Replace(content, @"\<meta property=""docfx:rel.+?\>", string.Empty);
            Console.WriteLine($"- removed");

            Console.WriteLine($"Writing content to '{filePath}'...");
            File.WriteAllText(filePath, content);
            Console.WriteLine($"- written");
        }
    }
}