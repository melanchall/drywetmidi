using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal sealed class ScaleParser : SimpleParser<Scale>
    {
        #region Constants

        private const string RootNoteNameGroupName = "rn";
        private const string IntervalsMnemonicGroupName = "im";
        private const string IntervalGroupName = "i";

        private static readonly string IntervalGroup = @$"(?<{IntervalGroupName}>([pmda]\d+|[\-\+]?\d+)\s*)+";
        private static readonly string IntervalsMnemonicGroup = $"(?<{IntervalsMnemonicGroupName}>.+?)";

        private const string ScaleIsUnknown = "Scale is unknown.";

        #endregion

        #region Methods

        internal override Regex[] GetRegexes() => MusicTheoryParsers
            .NoteNameParser
            .GetPatterns()
            .Select(p => new Regex($@"^(?<{RootNoteNameGroupName}>{p})\s*({IntervalGroup}|{IntervalsMnemonicGroup})$", RegexOptions.Compiled | RegexOptions.IgnoreCase))
            .ToArray();

        protected override Scale ParseInternal(string input)
        {
            var match = Match(input);
            if (match == null)
                ThrowInvalidFormatError();

            var rootNoteNameGroup = match.Groups[RootNoteNameGroupName];

            var rootNoteName = MusicTheoryParsers.NoteNameParser.Parse(rootNoteNameGroup.Value);

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

            return new Scale(intervals, rootNoteName);
        }

        #endregion
    }
}
