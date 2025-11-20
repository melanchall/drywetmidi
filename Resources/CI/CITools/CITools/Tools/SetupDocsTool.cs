namespace CITools.Tools
{
    [Tool("SetupDocs")]
    internal sealed class SetupDocsTool : ITool
    {
        public void Execute(string[] args)
        {
            var readmePath = args[0];
            if (!File.Exists(readmePath))
                throw new InvalidOperationException($"README file not found at path: '{readmePath}'.");

            var docsRootPath = args[1];
            if (!Directory.Exists(docsRootPath))
                throw new InvalidOperationException($"Docs root directory not found at path: '{docsRootPath}'.");

            var newIndexPath = Path.Combine(docsRootPath, "index.md");
            
            Console.WriteLine($"Copying README from '{readmePath}' to '{newIndexPath}'...");
            File.Copy(readmePath, newIndexPath, true);
            Console.WriteLine($"- copied");
            
            Console.WriteLine($"Updating overview section in '{newIndexPath}'...");
            var content = File.ReadAllText(newIndexPath);
            content = content.Replace("<!--OVERVIEW-->", "# Overview");
            File.WriteAllText(newIndexPath, content);
            Console.WriteLine($"- updated");
        }
    }
}
