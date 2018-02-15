using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Melanchall.DryWetMidi.Common
{
    internal static class ParsingUtilities
    {
        #region Constants

        private const NumberStyles NonnegativeNumberStyle = NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite;
        private const NumberStyles NumberStyle = NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingSign;

        #endregion

        #region Methods

        public static string GetNonnegativeNumberGroup(string groupName)
        {
            return $@"(?<{groupName}>\d+)";
        }

        public static string GetNumberGroup(string groupName)
        {
            return $@"(?<{groupName}>[\+\-]?\d+)";
        }

        public static Match Match(string input, IEnumerable<string> patterns)
        {
            return patterns.Select(p => Regex.Match(input.Trim(), $"^{p}$", RegexOptions.IgnoreCase))
                           .FirstOrDefault(m => m.Success);
        }

        public static bool ParseNonnegativeInt(Match match, string groupName, int defaultValue, out int value)
        {
            return ParseInt(match, groupName, defaultValue, NonnegativeNumberStyle, out value);
        }

        public static bool ParseInt(Match match, string groupName, int defaultValue, out int value)
        {
            return ParseInt(match, groupName, defaultValue, NumberStyle, out value);
        }

        public static bool ParseNonnegativeLong(Match match, string groupName, long defaultValue, out long value)
        {
            value = defaultValue;

            var group = match.Groups[groupName];
            return !group.Success || long.TryParse(group.Value, NonnegativeNumberStyle, null, out value);
        }

        private static bool ParseInt(Match match, string groupName, int defaultValue, NumberStyles numberStyle, out int value)
        {
            value = defaultValue;

            var group = match.Groups[groupName];
            return !group.Success || int.TryParse(group.Value, numberStyle, null, out value);
        }

        #endregion
    }
}
