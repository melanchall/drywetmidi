using System;
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

        private const string RootNoteNameGroupName = "n";
        private const string AccidentalGroupName = "a";
        private const string IntervalsGroupName = "is";
        private const string IntervalsMnemonicGroupName = "im";
        private const string IntervalGroupName = "i";

        private static readonly string RootNoteNameGroup = $"(?<{RootNoteNameGroupName}>C|D|E|F|G|A|B)";
        private static readonly string AccidentalGroup = $"(?<{AccidentalGroupName}>{Regex.Escape(Note.SharpShortString)}|{Note.SharpLongString})";
        private static readonly string IntervalsGroup = $"(?<{IntervalsGroupName}>({ParsingUtilities.GetNumberGroup(IntervalGroupName)}\\s*)+)";
        private static readonly string IntervalsMnemonicGroup = $"(?<{IntervalsMnemonicGroupName}>.+?)";

        private static readonly string[] Patterns = new[]
        {
            $@"{RootNoteNameGroup}\s*{AccidentalGroup}\s*({IntervalsGroup}|{IntervalsMnemonicGroup})",
            $@"{RootNoteNameGroup}\s*({IntervalsGroup}|{IntervalsMnemonicGroup})",
        };

        private const string RootNoteNameIsInvalid = "Root note's name is invalid.";
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
            var rootNoteNameString = rootNoteNameGroup.Value;

            var accidentalGroup = match.Groups[AccidentalGroupName];
            if (accidentalGroup.Success)
            {
                var accidental = accidentalGroup.Value;
                accidental = accidental.Replace(Note.SharpShortString, Note.SharpLongString);
                accidental = char.ToUpper(accidental[0]) + accidental.Substring(1);
                rootNoteNameString += accidental;
            }

            NoteName rootNoteName;
            if (!Enum.TryParse(rootNoteNameString, out rootNoteName))
                return ParsingResult.Error(RootNoteNameIsInvalid);

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

            scale = new Scale(intervals, rootNoteName);
            return ParsingResult.Parsed;
        }

        #endregion
    }
}
