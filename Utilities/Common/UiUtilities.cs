using System.Reflection;
using System.Text.RegularExpressions;

namespace Melanchall.Common
{
    public static class UiUtilities
    {
        #region Methods

        public static void WriteHello() => WriteLines(
            Assembly.GetCallingAssembly().GetName().Name,
            "2022",
            "Powered by Melanchall's DryWetMIDI library",
            "====================================================",
            string.Empty);

        public static void WriteUtilityDescription(string description) => WriteLines(
            description.Trim(),
            "----------------------------------------------------",
            string.Empty);

        public static void WriteLine(string line) => WriteLines(
            line.Trim());

        public static void Write(string line) => Console.Write(
            line);

        public static void WriteLine() =>
            Console.WriteLine();

        public static void WriteNumberedList<TElement>(ICollection<TElement> elements, Func<TElement, string> toString) => WriteLines(
            elements.Select((e, i) => $"{i}: {toString(e)}").ToArray());

        public static string[] ReadArray() =>
            Console.ReadLine()?.Trim().Split(' ');

        public static TElement SelectElementByNumber<TElement>(string title, ICollection<TElement> elements)
        {
            while (true)
            {
                Console.Write($"{title}: ");

                var numberString = Console.ReadLine()?.Trim();
                if (!int.TryParse(numberString, out var number))
                {
                    WriteLine($"Invalid number '{numberString}'. Try again");
                    continue;
                }

                if (number >= elements.Count)
                {
                    WriteLine($"Number is out of valid range [0, {elements.Count - 1}]. Try again");
                    continue;
                }

                return elements.ElementAt(number);
            }
        }

        public static ConsoleKey WaitForOneOfKeys(params ConsoleKey[] keys)
        {
            while (true)
            {
                var key = Console.ReadKey(true);
                if (keys.Contains(key.Key))
                    return key.Key;
            }
        }

        private static void WriteLines(params string[] lines) =>
            lines.ToList().ForEach(Console.WriteLine);

        private static void WriteColoredLine(string text, ConsoleColor? foreground, ConsoleColor? background)
        {
            var oldForeground = Console.ForegroundColor;
            var oldBackground = Console.BackgroundColor;

            try
            {
                if (foreground != null)
                    Console.ForegroundColor = foreground.Value;

                if (background != null)
                    Console.BackgroundColor = background.Value;

                Console.WriteLine(text);
            }
            finally
            {
                Console.ForegroundColor = oldForeground;
                Console.BackgroundColor = oldBackground;
            }
        }

        #endregion
    }
}
