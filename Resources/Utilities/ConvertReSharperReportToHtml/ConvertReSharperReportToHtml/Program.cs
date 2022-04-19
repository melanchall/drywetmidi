using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

var inputFilePath = Path.GetFullPath(args[0]);
var outputFilePath = Path.GetFullPath(args[1]);
var branch = args[2];

Console.WriteLine($"Converting [{inputFilePath}] to HTML [{outputFilePath}] for [{branch}] branch...");

var xDocument = XDocument.Load(inputFilePath);
var issueTypes = GetIssueTypes(xDocument);
var projects = GetProjects(xDocument);

var stringBuilder = new StringBuilder().AppendLine(@"
<html style=""font-family: Georgia, serif;"">
    <body>
");

foreach (var project in projects)
{
    var allIssues = GetProjectIssues(xDocument, project, issueTypes);
    var issuesGroups = allIssues
        .GroupBy(i => i.IssueType)
        .ToArray();

    stringBuilder.AppendLine($@"
        <hr>
        <hr>
        <h1>{project} ({allIssues.Length} issues)</h1>
");

    foreach (var issues in issuesGroups)
    {
        stringBuilder.AppendLine($@"
        <hr>
        <h2>[{issues.Key.Severity}] {issues.Key.Description}</h2>
        <table>
");

        foreach (var issue in issues)
        {
            var gitHubPath = issue.FilePath.Replace('\\', '/');
            stringBuilder.AppendLine($@"
            <tr>
                <td><a href=""https://github.com/melanchall/drywetmidi/blob/{branch}/{gitHubPath}#L{issue.LineNumber}"">{issue.FilePath} <strong>({issue.LineNumber})</strong></a></td>
                <td>{issue.Message}</td>
            </tr>
");
        }

        stringBuilder.AppendLine($@"
        </table>
");
    }
}

var html = stringBuilder.ToString();
File.WriteAllText(outputFilePath, html);

Console.WriteLine("All done.");

//

Dictionary<string, IssueType> GetIssueTypes(XDocument xDocument) => xDocument
    .Root
    .XPathSelectElements("//IssueType")
    .ToDictionary(
        e => e.Attribute("Id").Value,
        e => new IssueType(e.Attribute("Description").Value, e.Attribute("Severity").Value));

string[] GetProjects(XDocument xDocument) => xDocument
    .Root
    .XPathSelectElements("//Issues//Project")
    .Select(e => e.Attribute("Name").Value)
    .ToArray();

Issue[] GetProjectIssues(XDocument xDocument, string project, Dictionary<string, IssueType> issueTypes) => xDocument
    .Root
    .XPathSelectElements($"//Issues//Project[@Name='{project}']//Issue")
    .Select(e => new Issue(
        e.Attribute("File").Value,
        issueTypes[e.Attribute("TypeId").Value],
        Convert.ToInt32(e.Attribute("Line")?.Value ?? "-1"),
        e.Attribute("Message").Value))
    .ToArray();

record IssueType(string Description, string Severity);

record Issue(string FilePath, IssueType IssueType, int LineNumber, string Message);