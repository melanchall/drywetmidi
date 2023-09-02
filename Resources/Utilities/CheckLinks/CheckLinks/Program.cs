using System.Net;
using System.Text.RegularExpressions;

namespace CheckLinks
{
    internal class Program
    {
        private static readonly Regex LinkRegex = new(@"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9@:%_\+.~#?&\/=]*)");
        private static readonly Regex WikiRegex = new(@"melanchall\/drywetmidi\/wiki");
        private static readonly Regex GitHubUserContentRegex = new(@"https:\/\/raw\.githubusercontent\.com\/melanchall\/drywetmidi\/develop\/");

        private const int RetriesCount = 3;
        private static readonly TimeSpan RetryInterval = TimeSpan.FromSeconds(1);

        static void Main(string[] args)
        {
            if (args.Length < 2)
                WriteFatal("Invalid arguments count.", 0);

            var directoryPath = Path.GetFullPath(args[0]);
            var filter = args[1];

            if (!Directory.Exists(directoryPath))
                WriteFatal($"Directory '{directoryPath}' doesn't exist.", 0);

            var files = GetFiles(directoryPath, filter);
            var httpClient = GetHttpClient();
            var linksAreValid = CheckLinks(directoryPath, files, httpClient);

            if (!linksAreValid)
            {
                WriteError("There are failed checks.", 0);
                Environment.Exit(1);
            }
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
            WriteMessage("Checking links...", 0);

            var filesCount = files.Length;
            var result = true;

            for (var i = 0; i < filesCount; i++)
            {
                var filePath = files[i];
                WriteMessage($"[{i + 1}/{filesCount}] {filePath[directoryPath.Length..].Trim('/', '\\')}", 0);

                var fileText = File.ReadAllText(filePath);
                var matches = LinkRegex.Matches(fileText);
                if (!matches.Any())
                {
                    WriteMessage("no links", 1);
                    continue;
                }

                foreach (Match match in matches)
                {
                    var link = match.Value.Trim('.');
                    WriteMessage($"{link}...", 1);

                    if (WikiRegex.IsMatch(link))
                    {
                        WriteError("Wiki link is prohibited", 2);
                        continue;
                    }

                    var linkIsValid = false;
                    for (var j = 0; j < RetriesCount && !linkIsValid; j++)
                    {
                        linkIsValid |= CheckLink(link, httpClient);
                        if (!linkIsValid && j < RetriesCount - 1)
                        {
                            WriteMessage("retrying...", 2);
                            Thread.Sleep(RetryInterval);
                        }
                    }

                    if (linkIsValid)
                        WriteMessage("OK", 2);
                    else
                        WriteError("FAIL", 2);

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
                    WriteError($"failed with: {response.StatusCode}", 2);

                return response.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                WriteError($"failed with: {ex.Message}", 2);
                return false;
            }
        }

        private static string[] GetFiles(string directoryPath, string filter)
        {
            WriteMessage($"Searching files in the directory '{directoryPath}' by '{filter}' filter...", 0);

            var files = Directory.GetFiles(directoryPath, filter, SearchOption.AllDirectories);

            WriteMessage($"{files.Length} files found", 0);
            return files;
        }

        private static void WriteMessage(string message, int level) =>
            Console.WriteLine($"{AddLevel(message, level)}");

        private static void WriteFatal(string error, int level)
        {
            WriteError($"FATAL! {error}", level);
            Environment.Exit(1);
        }

        private static void WriteError(string error, int level) =>
            Console.WriteLine($"##[error]{AddLevel(error, level)}");

        private static void WriteWarning(string warning, int level) =>
            Console.WriteLine($"##[warning]{AddLevel(warning, level)}");

        private static string AddLevel(string message, int level) =>
            $"{new string(' ', level * 4)}{message}";
    }
}