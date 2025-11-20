namespace CITools.Tools
{
    [Tool("AddGoogleAnalyticsTagForDocs")]
    internal sealed class AddGoogleAnalyticsTagForDocsTool : ITool
    {
        private static readonly string[] FilesToExclude = new[]
        {
            "toc",
        };

        public void Execute(string[] args)
        {
            if (args.Length != 2)
                throw new InvalidOperationException("Not enough arguments. Usage: AddGoogleAnalyticsTagForDocs <DocsRootPath> <GoogleAnalyticsTag>");

            var measurementId = args[0];

            var googleAnalyticsTag = $@"
<!-- Google tag (gtag.js) -->
<script async src=""https://www.googletagmanager.com/gtag/js?id={measurementId}""></script>
<script>
  window.dataLayer = window.dataLayer || [];
  function gtag(){{dataLayer.push(arguments);}}
  gtag('js', new Date());

  gtag('config', '{measurementId}');
</script>".Trim();

            var docsDirectories = args[1].Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            const string headTag = "<head>";

            foreach (var docsDirectory in docsDirectories)
            {
                Console.WriteLine($"Processing docs directory: '{docsDirectory}'...");

                var htmlFiles = Directory.GetFiles(
                    docsDirectory,
                    "*.html",
                    SearchOption.AllDirectories);
                Console.WriteLine($"Found {htmlFiles.Length} HTML file(s).");

                foreach (var htmlFile in htmlFiles)
                {
                    Console.WriteLine($"- adding GA tag to file: '{htmlFile}'...");

                    if (FilesToExclude.Any(exclude => Path.GetFileNameWithoutExtension(htmlFile).Equals(exclude, StringComparison.OrdinalIgnoreCase)))
                    {
                        Console.WriteLine($"-- skipped");
                        continue;
                    }

                    var content = File.ReadAllText(htmlFile);
                    var headIndex = content.IndexOf(headTag, StringComparison.OrdinalIgnoreCase);

                    content = content.Insert(
                        headIndex + headTag.Length,
                        $"{Environment.NewLine}{googleAnalyticsTag}{Environment.NewLine}");

                    File.WriteAllText(htmlFile, content);
                    Console.WriteLine($"-- GA tag added");
                }
            }
        }
    }
}
