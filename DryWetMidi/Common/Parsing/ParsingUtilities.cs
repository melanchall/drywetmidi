using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Melanchall.DryWetMidi.Common
{
    internal static class ParsingUtilities
    {
        #region Constants

        private const NumberStyles NonnegativeIntegerNumberStyle = NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite;
        private const NumberStyles IntegerNumberStyle = NonnegativeIntegerNumberStyle | NumberStyles.AllowLeadingSign;
        private const NumberStyles NonnegativeDoubleNumberStyle = NonnegativeIntegerNumberStyle | NumberStyles.AllowDecimalPoint;

        #endregion

        #region Methods

        public static bool TryParse<T>(string input, Parsing<T> parsing, out T result)
        {
            return parsing(input, out result).Status == ParsingStatus.Parsed;
        }

        public static T Parse<T>(string input, Parsing<T> parsing)
        {
            T result;
            var parsingResult = parsing(input, out result);
            if (parsingResult.Status == ParsingStatus.Parsed)
                return result;

            throw parsingResult.Exception;
        }

        public static string GetNonnegativeIntegerNumberGroup(string groupName)
        {
            return $@"(?<{groupName}>\d+)";
        }

        public static string GetIntegerNumberGroup(string groupName)
        {
            return $@"(?<{groupName}>[\+\-]?\d+)";
        }

        public static string GetNonnegativeDoubleNumberGroup(string groupName)
        {
            return $@"(?<{groupName}>\d+(.\d+)?)";
        }

        public static Match Match(string input, IEnumerable<string> patterns, bool ignoreCase = true)
        {
            return patterns.Select(p => Regex.Match(input.Trim(), $"^{p}$", ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None))
                           .FirstOrDefault(m => m.Success);
        }

        public static Match[] Matches(string input, IEnumerable<string> patterns, bool ignoreCase = true)
        {
            return patterns.Select(p => Regex.Matches(input.Trim(), p, ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None).OfType<Match>().ToArray())
                           .FirstOrDefault(m => m.Any());
        }

        public static bool ParseNonnegativeInt(Match match, string groupName, int defaultValue, out int value)
        {
            return ParseInt(match, groupName, defaultValue, NonnegativeIntegerNumberStyle, out value);
        }

        public static bool ParseInt(Match match, string groupName, int defaultValue, out int value)
        {
            return ParseInt(match, groupName, defaultValue, IntegerNumberStyle, out value);
        }

        public static bool ParseNonnegativeDouble(Match match, string groupName, double defaultValue, out double value)
        {
            return ParseDouble(match, groupName, defaultValue, NonnegativeDoubleNumberStyle, out value);
        }

        public static bool ParseNonnegativeLong(Match match, string groupName, long defaultValue, out long value)
        {
            value = defaultValue;

            var group = match.Groups[groupName];
            return !group.Success || long.TryParse(group.Value, NonnegativeIntegerNumberStyle, null, out value);
        }

        private static bool ParseInt(Match match, string groupName, int defaultValue, NumberStyles numberStyle, out int value)
        {
            value = defaultValue;

            var group = match.Groups[groupName];
            return !group.Success || int.TryParse(group.Value, numberStyle, null, out value);
        }

        private static bool ParseDouble(Match match, string groupName, double defaultValue, NumberStyles numberStyle, out double value)
        {
            value = defaultValue;

            var group = match.Groups[groupName];
            return !group.Success || double.TryParse(group.Value, numberStyle, CultureInfo.InvariantCulture, out value);
        }

        #endregion
    }
}
