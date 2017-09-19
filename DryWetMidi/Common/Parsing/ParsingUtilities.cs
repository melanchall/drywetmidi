using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Melanchall.DryWetMidi.Common
{
    internal static class ParsingUtilities
    {
        #region Constants

        private const NumberStyles NumberStyle = NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite;
        private const NumberStyles NegativeNumberStyle = NumberStyle | NumberStyles.AllowLeadingSign;

        #endregion

        #region Methods

        public static string GetNumberGroup(string groupName)
        {
            return $@"(?<{groupName}>\d+)";
        }

        public static Match Match(string input, IEnumerable<string> patterns)
        {
            return patterns.Select(p => Regex.Match(input.Trim(), $"^{p}$"))
                           .FirstOrDefault(m => m.Success);
        }

        public static bool ParseInt(Match match, string groupName, int defaultValue, out int value, bool allowNegative = false)
        {
            value = defaultValue;

            var group = match.Groups[groupName];
            return !group.Success || int.TryParse(group.Value,
                                                  allowNegative ? NegativeNumberStyle : NumberStyle,
                                                  null,
                                                  out value);
        }

        public static bool ParseLong(Match match, string groupName, long defaultValue, out long value, bool allowNegative = false)
        {
            value = defaultValue;

            var group = match.Groups[groupName];
            return !group.Success || long.TryParse(group.Value,
                                                   allowNegative ? NegativeNumberStyle : NumberStyle,
                                                   null,
                                                   out value);
        }

        #endregion
    }
}
