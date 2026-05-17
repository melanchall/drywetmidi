namespace CITools.Tools
{
    [Tool("CreateOldApiRedirectionPages")]
    internal sealed class CreateOldApiRedirectionPagesTool : ITool
    {
        private static readonly Dictionary<string, string> NewToOldReplacements = new ()
        {
            ["Multimedia.EndpointsConnector"] = "Multimedia.DevicesConnector",
            ["Multimedia.EndpointsWatcher"] = "Multimedia.DevicesWatcher",
            ["Multimedia.EndpointAddedRemovedEventArgs"] = "Multimedia.DeviceAddedRemovedEventArgs",
            ["Multimedia.IInputEndpoint"] = "Multimedia.IInputDevice",
            ["Multimedia.InputEndpoint"] = "Multimedia.InputDevice",
            ["Multimedia.IOutputEndpoint"] = "Multimedia.IOutputDevice",
            ["Multimedia.OutputEndpoint"] = "Multimedia.OutputDevice",
            ["Multimedia.MidiEndpoint"] = "Multimedia.MidiDevice",

            ["Input-endpoint"] = "Input-device",
            ["Output-endpoint"] = "Output-device",
            ["Endpoints-watcher"] = "Devices-watcher",
            ["Endpoints-connector"] = "Devices-connector",
        };

        public void Execute(string[] args)
        {
            if (args.Length != 1)
                throw new InvalidOperationException("Not enough arguments. Usage: CreateOldApiRedirectionPages <ApiPagesDirectoryPath>");

            var pagesDirectoriesPaths = args[0].Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            var actualRedirections = new List<(string Path, string RedirectionPage)>();

            foreach (var pagesDirectoryPath in pagesDirectoriesPaths)
            {
                if (!Directory.Exists(pagesDirectoryPath))
                    throw new InvalidOperationException($"Directory '{pagesDirectoryPath}' does not exist.");

                Console.WriteLine($"Getting all pages in '{pagesDirectoryPath}'...");
                var pagesPaths = Directory.GetFiles(pagesDirectoryPath, "*.html", SearchOption.AllDirectories);
                Console.WriteLine($"Found {pagesPaths.Length} page(s).");

                foreach (var pagePath in pagesPaths)
                {
                    Console.WriteLine($"Processing '{pagePath}'...");

                    var replacement = NewToOldReplacements.FirstOrDefault(r => pagePath.Contains(r.Key));
                    if (string.IsNullOrWhiteSpace(replacement.Key))
                    {
                        Console.WriteLine($"- don't need redirection, skipping...");
                        continue;
                    }

                    var content = File.ReadAllText(pagePath);

                    var headStartLabel = "<head>";
                    var headEndLabel = "</head>";

                    var headStartIndex = content.IndexOf(headStartLabel, StringComparison.OrdinalIgnoreCase);
                    var headEndIndex = content.IndexOf(headEndLabel, StringComparison.OrdinalIgnoreCase);

                    if (headStartIndex < 0 || headEndIndex < 0)
                    {
                        Console.WriteLine($"- couldn't find <head> tags, skipping...");
                        continue;
                    }

                    Console.WriteLine("- modifying head...");

                    content = content.Remove(headStartIndex + headStartLabel.Length, headEndIndex - headStartIndex - headStartLabel.Length);
                    content = content.Insert(headStartIndex + headStartLabel.Length, @$"
                    <meta charset=""utf-8"">
                    <title>Redirecting...</title>
                    <meta http-equiv=""refresh"" content=""0; url={Path.GetFileName(pagePath)}"" />");

                    Console.WriteLine("- head modified");

                    var articleStartLabel = "<article";
                    var articleStartIndex = content.IndexOf(articleStartLabel, StringComparison.OrdinalIgnoreCase);

                    if (articleStartIndex < 0)
                    {
                        Console.WriteLine($"- couldn't find <article> tag, skipping...");
                        continue;
                    }

                    Console.WriteLine("- modifying content...");

                    content = content.Remove(articleStartIndex + articleStartLabel.Length, content.Length - articleStartIndex - articleStartLabel.Length);
                    content = content.Insert(articleStartIndex + articleStartLabel.Length, @$"
                    <h1>Document Moved</h1>
                    <p>This page has moved. <a href=""{Path.GetFileName(pagePath)}"">Click here if you are not redirected automatically</a>.</p>

                    <script>
                        window.location.replace(""{Path.GetFileName(pagePath)}"");
                    </script>");

                    Console.WriteLine("- content modified");

                    Console.WriteLine("- writing changes to old API file...");

                    var oldPagePath = pagePath.Replace(replacement.Key, replacement.Value);
                    File.WriteAllText(oldPagePath, content);

                    Console.WriteLine($"- redirection page created at '{oldPagePath}'");

                    actualRedirections.Add((pagePath, oldPagePath));
                }
            }

            Console.WriteLine("Redirections page created:");

            foreach (var (path, redirectionPage) in actualRedirections)
            {
                Console.WriteLine($"{redirectionPage}{Environment.NewLine}-> {path}");
            }
        }
    }
}
