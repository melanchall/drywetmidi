using System.Linq;
using System.Text.RegularExpressions;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal static class ChordParser
    {
        #region Constants

        private const string RootNoteNameGroupName = "rn";
        private const string IntervalGroupName = "i";

        private static readonly string IntervalGroup = $"(?<{IntervalGroupName}>({string.Join("|", IntervalParser.GetPatterns())})\\s*)+";

        private static readonly string[] Patterns = NoteNameParser.GetPatterns()
                                                                  .Select(p => $@"(?<{RootNoteNameGroupName}>{p})\s*{IntervalGroup}")
                                                                  .ToArray();

        #endregion

        #region Methods

        internal static ParsingResult TryParse(string input, out Chord chord)
        {
            chord = null;

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

            var intervalGroup = match.Groups[IntervalGroupName];
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

            chord = new Chord(rootNoteName, intervalsParsingResults.Select(r => r.Interval).ToArray());
            return ParsingResult.Parsed;
        }

        #endregion
    }
}
