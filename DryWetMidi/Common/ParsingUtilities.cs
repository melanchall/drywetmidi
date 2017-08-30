using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Melanchall.DryWetMidi.Common
{
    internal static class ParsingUtilities
    {
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

        public static bool ParseInt(Match match, string groupName, int defaultValue, out int value)
        {
            value = defaultValue;

            var group = match.Groups[groupName];
            return !group.Success || int.TryParse(group.Value, out value);
        }

        public static bool ParseLong(Match match, string groupName, long defaultValue, out long value)
        {
            value = defaultValue;

            var group = match.Groups[groupName];
            return !group.Success || long.TryParse(group.Value, out value);
        }

        #endregion
    }
}
