using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal static class ScaleParser
    {
        #region Constants

        private const string RootNoteNameGroupName = "rn";
        private const string IntervalsMnemonicGroupName = "im";
        private const string IntervalGroupName = "i";

        private static readonly string IntervalGroup = $"(?<{IntervalGroupName}>({string.Join("|", IntervalParser.GetPatterns())})\\s*)+";
        private static readonly string IntervalsMnemonicGroup = $"(?<{IntervalsMnemonicGroupName}>.+?)";

        private static readonly string[] Patterns = NoteNameParser.GetPatterns()
                                                                  .Select(p => $@"(?<{RootNoteNameGroupName}>{p})\s*({IntervalGroup}|{IntervalsMnemonicGroup})")
                                                                  .ToArray();

        private const string ScaleIsUnknown = "Scale is unknown.";

        #endregion

        #region Methods

        internal static ParsingResult TryParse(string input, out Scale scale)
        {
            scale = null;

            if (string.IsNullOrWhiteSpace(input))
                return ParsingResult.EmptyInputString;

            var match = ParsingUtilities.Match(input, Patterns);
            if (match == null)
                return ParsingResult.NotMatched;

            var rootNoteNameGroup = match.Groups[RootNoteNameGroupName];

            NoteName rootNoteName;
            var rootNoteNameParsingResult = NoteNameParser.TryParse(rootNoteNameGroup.Value, out rootNoteName);
            if (rootNoteNameParsingResult.Status != ParsingStatus.Parsed)
                return rootNoteNameParsingResult;

            //

            IEnumerable<Interval> intervals;

            var intervalGroup = match.Groups[IntervalGroupName];
            if (intervalGroup.Success)
            {
                var intervalsParsingResults = intervalGroup
                    .Captures
                    .OfType<Capture>()
                    .Select(c =>
                    {
                        Interval interval;
                        var parsingResult = IntervalParser.TryParse(c.Value, out interval);

                        return new
                        {
                            Interval = interval,
                            ParsingResult = parsingResult
                        };
                    })
                    .ToArray();

                var notParsedResult = intervalsParsingResults.FirstOrDefault(r => r.ParsingResult.Status != ParsingStatus.Parsed);
                if (notParsedResult != null)
                    return notParsedResult.ParsingResult;

                intervals = intervalsParsingResults.Select(r => r.Interval).ToArray();
            }
            else
            {
                var intervalsMnemonicGroup = match.Groups[IntervalsMnemonicGroupName];
                var intervalsName = intervalsMnemonicGroup.Value;

                intervals = ScaleIntervals.GetByName(intervalsName);
            }

            if (intervals == null)
                return ParsingResult.Error(ScaleIsUnknown);

            //

            scale = new Scale(intervals, rootNoteName);
            return ParsingResult.Parsed;
        }

        #endregion
    }
}
