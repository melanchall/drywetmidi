using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Melanchall.DryWetMidi.Tools
{
    internal static class CsvUtilities
    {
        #region Constants

        private const char Quote = '"';
        private const string QuoteString = "\"";
        private const string DoubleQuote = "\"\"";

        #endregion

        #region Methods

        public static string EscapeString(string input)
        {
            return $"{Quote}{input.Replace(QuoteString, DoubleQuote)}{Quote}";
        }

        public static string UnescapeString(string input)
        {
            return input.Replace(DoubleQuote, QuoteString).Trim(Quote);
        }

        public static string MergeCsvValues(char delimiter, IEnumerable<object> values)
        {
            return string.Join(delimiter.ToString(), values);
        }

        public static string[] SplitCsvValues(string input, char delimiter, Func<string> nextLineGetter)
        {
            string[] parts;

            while (true)
            {
                parts = SplitCsvValues(input, delimiter).ToArray();
                if (parts.All(IsPartClosed))
                    break;

                var nextLine = nextLineGetter();
                if (nextLine == null)
                    break;

                input = input + Environment.NewLine + nextLine;
            }

            return parts;
        }

        private static IEnumerable<string> SplitCsvValues(string input, char delimiter)
        {
            var partBuilder = new StringBuilder();
            var escapedString = false;
            var possibleFinishedPart = false;

            foreach (var c in input)
            {
                if (c == delimiter && (!escapedString || possibleFinishedPart))
                {
                    yield return partBuilder.ToString().Trim();

                    partBuilder.Clear();
                    possibleFinishedPart = false;
                    escapedString = false;
                    continue;
                }

                if (c == Quote)
                {
                    if (!escapedString)
                        escapedString = true;
                    else
                        possibleFinishedPart = !possibleFinishedPart;
                }

                partBuilder.Append(c);
            }

            yield return partBuilder.ToString().Trim();
        }

        private static bool IsPartClosed(string part)
        {
            if (string.IsNullOrEmpty(part) || part[0] != Quote)
                return true;

            if (part.Length == 1)
                return false;

            return part.Skip(1).Reverse().TakeWhile(c => c == Quote).Count() % 2 == 1;
        }

        #endregion
    }
}
