using ConvertReSharperReportToHtml;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

var inputFilePath = Path.GetFullPath(args[0]);
var outputFilePath = Path.GetFullPath(args[1]);
var branch = args[2];

Console.WriteLine($"Converting [{inputFilePath}] to HTML [{outputFilePath}] for [{branch}] branch...");

var reportJson = File.ReadAllText(inputFilePath);
var xDocument = JsonSerializer.Deserialize<ReportDto>(reportJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
var issueTypes = GetIssueTypes(xDocument);
var projects = GetProjects(xDocument);

var stringBuilder = new StringBuilder().AppendLine(@"
<html style=""font-family: Georgia, serif;"">
    <body>
");

foreach (var project in projects)
{
    Console.WriteLine($"Processing [{project}] project...");
    if (project.Contains("Test"))
    {
        Console.WriteLine("    skip test project");
        continue;
    }

    var allIssues = GetProjectIssues(xDocument, project, issueTypes);
    Console.WriteLine($"    [{allIssues.Length}] issues found");

    var issuesGroups = allIssues
        .GroupBy(i => i.IssueType)
        .ToArray();

    Console.WriteLine($"    [{issuesGroups.Length}] issues groups built");

    stringBuilder.AppendLine($@"
        <hr>
        <hr>
        <h1>{project} ({allIssues.Length} issues)</h1>
");

    var i = 1;

    foreach (var issues in issuesGroups)
    {
        Console.WriteLine($"    [{i} / {issuesGroups.Length}] {issues.Key}");

        stringBuilder.AppendLine($@"
        <hr>
        <h2>[{issues.Key.Severity}] {issues.Key.Description}</h2>
        <table>
");

        var j = 1;
        var groupSize = issues.Count();

        foreach (var issue in issues)
        {
            Console.WriteLine($"        [{j} / {groupSize}] {issue.Message}");

            var gitHubPath = issue.FilePath.Replace('\\', '/');
            stringBuilder.AppendLine($@"
            <tr>
                <td><a href=""https://github.com/melanchall/drywetmidi/blob/{branch}/{gitHubPath}#L{issue.LineNumber}"">{issue.FilePath} <strong>({issue.LineNumber})</strong></a></td>
                <td>{issue.Message}</td>
            </tr>
");

            j++;
        }

        stringBuilder.AppendLine($@"
        </table>
");

        i++;
    }
}

var html = stringBuilder.ToString();
File.WriteAllText(outputFilePath, html);

Console.WriteLine("All done.");

//

Dictionary<string, IssueType> GetIssueTypes(ReportDto xDocument) => xDocument
    .Runs
    .First()
    .Tool
    .Driver
    .Rules
    .ToDictionary(
        e => e.Id,
        e => new IssueType(e.FullDescription?.Text ?? e.ShortDescription.Text, e.Relationships.First().Target.Id));

string[] GetProjects(ReportDto xDocument) => xDocument
    .Runs
    .First()
    .Results
    .Select(e => Regex.Match(e.Locations.First().PhysicalLocation.ArtifactLocation.Uri, @"(.+?)/").Groups[1].Value)
    .Distinct()
    .ToArray();

Issue[] GetProjectIssues(ReportDto xDocument, string project, Dictionary<string, IssueType> issueTypes) => xDocument
    .Runs
    .First()
    .Results
    .Where(e => e.Locations.First().PhysicalLocation.ArtifactLocation.Uri.StartsWith($"{project}/"))
    .Select(e => new Issue(
        e.Locations.First().PhysicalLocation.ArtifactLocation.Uri,
        issueTypes[e.RuleId],
        e.Locations.First().PhysicalLocation.Region.StartLine,
        e.Message.Text))
    .ToArray();

record IssueType(string Description, string Severity);

record Issue(string FilePath, IssueType IssueType, int LineNumber, string Message);