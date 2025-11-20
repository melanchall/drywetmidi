using System.Net;
using System.Text.RegularExpressions;

namespace CITools.Tools
{
    [Tool("CheckLinks")]
    internal sealed class CheckLinksTool : ITool
    {
        private static readonly Regex LinkRegex = new(@"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9@:%_\+.~#?&\/=]*)");
        private static readonly Regex WikiRegex = new(@"melanchall\/drywetmidi\/wiki");

        private const int RetriesCount = 3;
        private static readonly TimeSpan RetryInterval = TimeSpan.FromSeconds(1);

        public void Execute(string[] args)
        {
            if (args.Length < 2)
                throw new InvalidOperationException("Invalid arguments count.");

            var directoryPath = Path.GetFullPath(args[0]);
            var filter = args[1];

            if (!Directory.Exists(directoryPath))
                throw new InvalidOperationException($"Directory '{directoryPath}' doesn't exist.");

            var files = GetFiles(directoryPath, filter);
            var httpClient = GetHttpClient();
            var linksAreValid = CheckLinks(directoryPath, files, httpClient);

            if (!linksAreValid)
                throw new InvalidOperationException("There are failed checks.");
        }

        private static HttpClient GetHttpClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "C# App");
            return client;
        }

        private static bool CheckLinks(
            string directoryPath,
            string[] files,
            HttpClient httpClient)
        {
            Console.WriteLine("Checking links...");

            var filesCount = files.Length;
            var result = true;

            for (var i = 0; i < filesCount; i++)
            {
                var filePath = files[i];
                Console.WriteLine($"[{i + 1}/{filesCount}] {filePath[directoryPath.Length..].Trim('/', '\\')}");

                var fileText = File.ReadAllText(filePath);
                var matches = LinkRegex.Matches(fileText);
                if (!matches.Any())
                {
                    Console.WriteLine("- no links");
                    continue;
                }

                foreach (Match match in matches)
                {
                    var link = match.Value.Trim('.');
                    Console.WriteLine($"- {link}...");

                    if (WikiRegex.IsMatch(link))
                    {
                        Console.WriteLine("-- FAIL: Wiki link is prohibited");
                        result = false;
                        continue;
                    }

                    var linkIsValid = false;
                    for (var j = 0; j < RetriesCount && !linkIsValid; j++)
                    {
                        linkIsValid |= CheckLink(link, httpClient);
                        if (!linkIsValid && j < RetriesCount - 1)
                        {
                            Console.WriteLine("-- retrying...");
                            Thread.Sleep(RetryInterval);
                        }
                    }

                    if (linkIsValid)
                        Console.WriteLine("-- OK");
                    else
                        Console.WriteLine("-- FAIL");

                    result &= linkIsValid;
                }
            }

            return result;
        }

        private static bool CheckLink(
            string link,
            HttpClient httpClient)
        {
            try
            {
                var response = httpClient.GetAsync(link).Result;
                if (response.StatusCode != HttpStatusCode.OK)
                    Console.WriteLine($"-- failed with: {response.StatusCode}");

                return response.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-- failed with: {ex.Message}");
                return false;
            }
        }

        private static string[] GetFiles(string directoryPath, string filter)
        {
            Console.WriteLine($"Searching files in the directory '{directoryPath}' by '{filter}' filter...");

            var files = Directory.GetFiles(directoryPath, filter, SearchOption.AllDirectories);

            Console.WriteLine($"- {files.Length} files found");
            return files;
        }

        private static void WriteMessage(string message, int level) =>
            Console.WriteLine($"{AddLevel(message, level)}");

        private static string AddLevel(string message, int level) =>
            $"{new string('-', level)}{(level > 0 ? " " : null)}{message}";
    }
}
