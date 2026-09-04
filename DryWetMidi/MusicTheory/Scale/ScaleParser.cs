using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal sealed class ScaleParser : SimpleParser<Scale>
    {
        #region Constants

        private const string IntervalsMnemonicGroupName = "im";
        private const string IntervalGroupName = "i";

        private static readonly string IntervalGroup = @$"(?<{IntervalGroupName}>([pmda]\d+|[\-\+]?\d+)\s*)+";
        private static readonly string IntervalsMnemonicGroup = $"(?<{IntervalsMnemonicGroupName}>.+?)";

        private const string ScaleIsUnknown = "Scale is unknown.";

        #endregion

        #region Methods

        internal override Regex[] GetRegexes() => new[]
        {
            new Regex($@"^({IntervalGroup}|{IntervalsMnemonicGroup})$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };

        protected override Scale ParseInternal(string input)
        {
            var span = input.AsSpan();
            var (rootNoteName, rootNoteNamePartLength) = MusicTheoryParsers.NoteNameParser.TryReadNoteName(span);
            if (rootNoteName == null)
                ThrowInvalidFormatError();

            var slice = span.Slice(0, rootNoteNamePartLength);
            if (slice.EndsWith("b", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var n in ScaleIntervals.BNames)
                {
                    if (span.Slice(rootNoteNamePartLength - 1).StartsWith(n, StringComparison.InvariantCultureIgnoreCase))
                    {
                        rootNoteName = (NoteName)(((int)rootNoteName.Value + 1) % Octave.OctaveSize);
                        rootNoteNamePartLength--;
                        break;
                    }
                }
            }
            else if (slice.EndsWith("flat", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var n in ScaleIntervals.FlatNames)
                {
                    if (span.Slice(rootNoteNamePartLength - 4).StartsWith(n, StringComparison.InvariantCultureIgnoreCase))
                    {
                        rootNoteName = (NoteName)(((int)rootNoteName.Value + 1) % Octave.OctaveSize);
                        rootNoteNamePartLength -= 4;
                        break;
                    }
                }
            }

            var match = Match(span.Slice(rootNoteNamePartLength).Trim().ToString());
            if (match == null)
                ThrowInvalidFormatError();

            //

            IEnumerable<Interval>? intervals;

            var intervalGroup = match.Groups[IntervalGroupName];
            if (intervalGroup.Success)
            {
                var intervalsParsingResults = intervalGroup
                    .Captures
                    .OfType<Capture>()
                    .Select(c =>
                    {
                        var success = MusicTheoryParsers.IntervalParser.TryParse(c.Value, out var interval);
                        return new
                        {
                            Interval = interval,
                            Success = success
                        };
                    })
                    .ToArray();

                // TODO: maybe error???
                if (intervalsParsingResults.Any(r => !r.Success))
                    ThrowInvalidFormatError();

                intervals = intervalsParsingResults.Select(r => r.Interval).ToArray()!;
            }
            else
            {
                var intervalsMnemonicGroup = match.Groups[IntervalsMnemonicGroupName];
                var intervalsName = intervalsMnemonicGroup.Value;

                intervals = ScaleIntervals.GetByName(intervalsName);
            }

            if (intervals == null)
                ThrowError(ScaleIsUnknown);

            //

            return new Scale(intervals, rootNoteName.Value);
        }

        #endregion
    }
}
