using System.Reflection;

namespace CITools
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No tool specified.");
                return;
            }

            var toolName = args[0];
            Console.WriteLine($"Searching for tool: '{toolName}'...");

            var toolType = typeof(Program)
                .Assembly
                .GetTypes()
                .Where(t =>
                    typeof(ITool).IsAssignableFrom(t) &&
                    !t.IsAbstract &&
                    !t.IsInterface)
                .FirstOrDefault(t =>
                {
                    var attribute = t.GetCustomAttributes<ToolAttribute>().Single();
                    return attribute.Name.Equals(toolName, StringComparison.OrdinalIgnoreCase);
                });

            if (toolType == null)
            {
                Console.WriteLine($"Tool '{toolName}' not found.");
                return;
            }

            var tool = (ITool)Activator.CreateInstance(toolType)!;

            Console.WriteLine($"Executing tool '{toolName}'...");
            tool.Execute(args.Skip(1).ToArray());

            Console.WriteLine("All done.");
        }
    }
}
