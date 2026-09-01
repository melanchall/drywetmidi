using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Melanchall.DryWetMidi.Common
{
    internal abstract class Parser
    {
        private const NumberStyles NonnegativeIntegerNumberStyle = NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite;
        private const NumberStyles IntegerNumberStyle = NonnegativeIntegerNumberStyle | NumberStyles.AllowLeadingSign;
        private const NumberStyles NonnegativeDoubleNumberStyle = NonnegativeIntegerNumberStyle | NumberStyles.AllowDecimalPoint;

        private Regex[]? _regexes = null;

        [DoesNotReturn]
        protected void ThrowError(string error)
        {
            throw new FormatException(error);
        }

        [DoesNotReturn]
        protected void ThrowInvalidFormatError()
        {
            throw new FormatException("Input string has invalid format.");
        }

        protected Match? Match(string input)
        {
            if (_regexes == null)
                _regexes = GetRegexes();

            foreach (var regex in _regexes)
            {
                var match = regex.Match(input.Trim());
                if (match.Success)
                    return match;
            }

            return null;
        }

        internal abstract Regex[] GetRegexes();

        protected static string GetNonnegativeIntegerNumberGroup(string groupName)
        {
            return $@"(?<{groupName}>\d+)";
        }

        protected static string GetIntegerNumberGroup(string groupName)
        {
            return $@"(?<{groupName}>[\+\-]?\d+)";
        }

        protected static string GetNonnegativeDoubleNumberGroup(string groupName, params char[] decimalSeparators)
        {
            return $@"(?<{groupName}>\d+({(decimalSeparators?.Any() == true ? $"[{string.Join(string.Empty, decimalSeparators.Select(s => Regex.Escape(s.ToString())))}]" : @"\.")}\d+)?)";
        }

        protected static Match? Match(string input, IEnumerable<string> patterns, bool ignoreCase = true)
        {
            return patterns
                .Select(p => Regex.Match(input.Trim(), $"^{p}$", ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None))
                .FirstOrDefault(m => m.Success);
        }

        protected static Match[]? Matches(string input, IEnumerable<string> patterns, bool ignoreCase = true)
        {
            return patterns
                .Select(p => Regex.Matches(input.Trim(), p, ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None).OfType<Match>().ToArray())
                .FirstOrDefault(m => m.Any());
        }

        protected static bool ParseNonnegativeInt(Match match, string groupName, int defaultValue, out int value)
        {
            return ParseInt(match, groupName, defaultValue, NonnegativeIntegerNumberStyle, out value);
        }

        protected static bool ParseInt(Match match, string groupName, int defaultValue, out int value)
        {
            return ParseInt(match, groupName, defaultValue, IntegerNumberStyle, out value);
        }

        protected static bool ParseNonnegativeDouble(Match match, string groupName, double defaultValue, char[] decimalSeparators, out double value)
        {
            return ParseDouble(
                match,
                groupName,
                defaultValue,
                NonnegativeDoubleNumberStyle,
                decimalSeparators,
                out value);
        }

        protected static bool ParseNonnegativeLong(Match match, string groupName, long defaultValue, out long value)
        {
            value = defaultValue;

            var group = match.Groups[groupName];
            return !group.Success || long.TryParse(group.Value, NonnegativeIntegerNumberStyle, null, out value);
        }

        protected static bool ParseInt(Match match, string groupName, int defaultValue, NumberStyles numberStyle, out int value)
        {
            value = defaultValue;

            var group = match.Groups[groupName];
            return !group.Success || int.TryParse(group.Value, numberStyle, null, out value);
        }

        protected static bool ParseDouble(
            Match match,
            string groupName,
            double defaultValue,
            NumberStyles numberStyle,
            char[] decimalSeparators,
            out double value)
        {
            value = defaultValue;

            var group = match.Groups[groupName];
            if (!group.Success)
                return true;

            foreach (var c in decimalSeparators)
            {
                var numberFormat = new NumberFormatInfo { NumberDecimalSeparator = c.ToString() };
                if (double.TryParse(group.Value, numberStyle, numberFormat, out value))
                    return true;
            }

            return false;
        }
    }
}
