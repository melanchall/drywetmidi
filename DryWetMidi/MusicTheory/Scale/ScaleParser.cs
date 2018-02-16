using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal static class ScaleParser
    {
        #region Constants

        private const string NoteNameGroupName = "n";
        private const string IntervalsGroupName = "is";
        private const string IntervalsMnemonicGroupName = "im";
        private const string IntervalGroupName = "i";

        private static readonly string IntervalsGroup = $"(?<{IntervalsGroupName}>({ParsingUtilities.GetNumberGroup(IntervalGroupName)}\\s*)+)";
        private static readonly string IntervalsMnemonicGroup = $"(?<{IntervalsMnemonicGroupName}>.+?)";

        private static readonly string[] Patterns = NoteNameParser.GetPatterns()
                                                                  .Select(p => $@"(?<{NoteNameGroupName}>{p})\s*({IntervalsGroup}|{IntervalsMnemonicGroup})")
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

            var noteNameGroup = match.Groups[NoteNameGroupName];

            NoteName noteName;
            var noteNameParsingResult = NoteNameParser.TryParse(noteNameGroup.Value, out noteName);
            if (noteNameParsingResult.Status != ParsingStatus.Parsed)
                return noteNameParsingResult;

            //

            IEnumerable<Interval> intervals = null;

            var intervalGroup = match.Groups[IntervalGroupName];
            if (intervalGroup.Success)
            {
                intervals = intervalGroup.Captures
                                         .OfType<Capture>()
                                         .Select(c =>
                                         {
                                             var halfSteps = int.Parse(c.Value, NumberStyles.AllowLeadingSign | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite);
                                             return Interval.FromHalfSteps(halfSteps);
                                         })
                                         .ToArray();
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

            scale = new Scale(intervals, noteName);
            return ParsingResult.Parsed;
        }

        #endregion
    }
}
