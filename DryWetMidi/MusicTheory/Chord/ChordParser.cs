using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal static class ChordParser
    {
        #region Enums

        private enum Quality
        {
            Major,
            Minor,
            Diminished,
            HalfDiminished,
            Augmented,
            Dominant
        }

        #endregion

        #region Constants

        private const string RootNoteNameGroupName = "rn";
        private const string IntervalGroupName = "i";

        private const string ChordQualityGroupName = "q";
        private const string MajorQualityGroupName = "maj";
        private const string MinorQualityGroupName = "min";
        private const string DiminishedQualityGroupName = "dim";
        private const string AugmentedQualityGroupName = "aug";
        private const string HalfDiminishedQualityGroupName = "hdim";
        private const string DominantQualityGroupName = "dom";

        private const string BassNoteNameGroupName = "bn";

        private const string ChordIntervalsGroupName = "ci";
        private const string ChordNameGroupName = "cn";

        private const string ExtensionQualityGroupName = "eq";
        private const string ExtensionMajorQualityGroupName = "emaj";
        private const string ExtensionMinorQualityGroupName = "emin";
        private const string ExtensionDiminishedQualityGroupName = "edim";
        private const string ExtensionAugmentedQualityGroupName = "eaug";
        private const string ExtensionGroupName = "ext";
        private const string ExtensionNumberGroupName = "en";

        private const string SuspendedNumberGroupName = "susn";
        private const string SuspendedGroupName = "sus";

        private static readonly string IntervalGroup = $"(?<{IntervalGroupName}>({string.Join("|", IntervalParser.GetPatterns())})\\s*)+";
        private static readonly string RootNoteNameGroup = $"(?<{RootNoteNameGroupName}>{string.Join("|", NoteNameParser.GetPatterns())})";
        private static readonly string BassNoteNameGroup = $"(?<{BassNoteNameGroupName}>{string.Join("|", NoteNameParser.GetPatterns())})";
        private static readonly string ChordQualityGroup = $@"(?<{ChordQualityGroupName}>(?<{MajorQualityGroupName}>(?i:maj)|M)|(?<{MinorQualityGroupName}>(?i:min)|m)|(?<{AugmentedQualityGroupName}>(?i:aug)|\+)|(?<{DiminishedQualityGroupName}>(?i:dim))|(?<{HalfDiminishedQualityGroupName}>ø)|(?<{DominantQualityGroupName}>(?i:dom)))";
        private static readonly string ChordExtensionQualityGroup = $@"(?<{ExtensionQualityGroupName}>(?<{ExtensionMajorQualityGroupName}>(?i:maj)|M)|(?<{ExtensionMinorQualityGroupName}>(?i:min)|m)|(?<{ExtensionAugmentedQualityGroupName}>(?i:(aug|a)))|(?<{ExtensionDiminishedQualityGroupName}>(?i:(dim|d))))";
        private static readonly string ChordExtensionGroup = $@"(?<{ExtensionGroupName}>{ChordExtensionQualityGroup}?\s*(?<{ExtensionNumberGroupName}>\d+))";
        private static readonly string SuspendedGroup = $@"(?<{SuspendedGroupName}>(?i:sus)(?<{SuspendedNumberGroupName}>\d))";

        private static readonly string[] Patterns = new[]
        {
            $@"(?<{ChordNameGroupName}>(?i:{RootNoteNameGroup})\s*{ChordQualityGroup}?\s*({ChordExtensionGroup}|\(\s*{ChordExtensionGroup}\s*\)|/\s*{ChordExtensionGroup})?\s*{SuspendedGroup}?\s*(/(?i:{BassNoteNameGroup}))?)",
            $@"(?<{ChordIntervalsGroupName}>(?i:{RootNoteNameGroup})\s*(?i:{IntervalGroup}))"
        };

        private static readonly Dictionary<string, Quality> GroupsQualities = new Dictionary<string, Quality>
        {
            [MajorQualityGroupName] = Quality.Major,
            [MinorQualityGroupName] = Quality.Minor,
            [DiminishedQualityGroupName] = Quality.Diminished,
            [AugmentedQualityGroupName] = Quality.Augmented,
            [HalfDiminishedQualityGroupName] = Quality.HalfDiminished,
            [DominantQualityGroupName] = Quality.Dominant
        };

        private static readonly Dictionary<string, IntervalQuality> GroupsExtensionQualities = new Dictionary<string, IntervalQuality>
        {
            [ExtensionMajorQualityGroupName] = IntervalQuality.Major,
            [ExtensionMinorQualityGroupName] = IntervalQuality.Minor,
            [ExtensionDiminishedQualityGroupName] = IntervalQuality.Diminished,
            [ExtensionAugmentedQualityGroupName] = IntervalQuality.Augmented
        };

        private static readonly Dictionary<Quality, ChordQuality> ChordQualities = new Dictionary<Quality, ChordQuality>
        {
            [Quality.Major] = ChordQuality.Major,
            [Quality.Minor] = ChordQuality.Minor,
            [Quality.Diminished] = ChordQuality.Diminished,
            [Quality.Augmented] = ChordQuality.Augmented
        };

        private static readonly Dictionary<Quality, IntervalQuality> ChordToIntervalQualities = new Dictionary<Quality, IntervalQuality>
        {
            [Quality.Major] = IntervalQuality.Major,
            [Quality.Minor] = IntervalQuality.Minor,
            [Quality.Diminished] = IntervalQuality.Diminished,
            [Quality.Augmented] = IntervalQuality.Augmented
        };

        private const string ExtensionNumberIsOutOfRange = "Extension number is out of range.";
        private const string HalfDiminishedOrDominantIsNotSeventh = "Half-diminished or dominant chord is not seventh one.";

        #endregion

        #region Methods

        internal static ParsingResult TryParse(string input, out Chord chord)
        {
            chord = null;

            if (string.IsNullOrWhiteSpace(input))
                return ParsingResult.EmptyInputString;

            var match = ParsingUtilities.Match(input, Patterns, ignoreCase: false);
            if (match == null)
                return ParsingResult.NotMatched;

            var rootNoteNameGroup = match.Groups[RootNoteNameGroupName];

            NoteName rootNoteName;
            var rootNoteNameParsingResult = NoteNameParser.TryParse(rootNoteNameGroup.Value, out rootNoteName);
            if (rootNoteNameParsingResult.Status != ParsingStatus.Parsed)
                return rootNoteNameParsingResult;

            if (match.Groups[ChordNameGroupName].Success)
                return TryParseChordName(match, rootNoteName, out chord);
            else if (match.Groups[ChordIntervalsGroupName].Success)
                return TryParseChordIntervals(match, rootNoteName, out chord);

            return ParsingResult.Parsed;
        }

        private static ParsingResult TryParseChordName(Match match, NoteName rootNoteName, out Chord chord)
        {
            chord = null;

            //

            Quality? quality = null;

            var qualityGroup = match.Groups[ChordQualityGroupName];
            if (qualityGroup.Success)
            {
                quality = GroupsQualities.FirstOrDefault(gq => match.Groups[gq.Key].Success).Value;
            }

            //

            var extensionIntervalNumber = -1;
            IntervalQuality? extensionIntervalQuality = null;

            var extensionGroup = match.Groups[ExtensionGroupName];
            if (extensionGroup.Success && !string.IsNullOrWhiteSpace(extensionGroup.Value))
            {
                if (match.Groups[ExtensionQualityGroupName].Success)
                    extensionIntervalQuality = GroupsExtensionQualities.FirstOrDefault(gq => match.Groups[gq.Key].Success).Value;

                if (!ParsingUtilities.ParseInt(match, ExtensionNumberGroupName, -1, out extensionIntervalNumber) ||
                    extensionIntervalNumber < 5)
                    return ParsingResult.Error(ExtensionNumberIsOutOfRange);
            }

            if (quality == Quality.HalfDiminished || quality == Quality.Dominant)
            {
                if (extensionIntervalNumber >= 0 && extensionIntervalNumber != 7)
                    return ParsingResult.Error(HalfDiminishedOrDominantIsNotSeventh);

                if (extensionIntervalNumber < 0)
                    extensionIntervalNumber = 7;
            }

            var extensionNotes = new List<NoteName>();

            if (extensionIntervalNumber >= 0)
            {
                var intervalQualities = Enumerable
                    .Range(0, extensionIntervalNumber + 1)
                    .Select(i => i > 2 && Interval.IsPerfect(i) ? IntervalQuality.Perfect : IntervalQuality.Major)
                    .ToArray();

                if (extensionIntervalNumber >= 7)
                {
                    if (extensionIntervalQuality == null && quality == null)
                        intervalQualities[7] = IntervalQuality.Minor;
                    else if (extensionIntervalQuality != null)
                    {
                        intervalQualities[7] = extensionIntervalQuality.Value;
                        intervalQualities[extensionIntervalNumber] = extensionIntervalQuality.Value;
                    }
                    else
                    {
                        intervalQualities[7] = quality == Quality.HalfDiminished || quality == Quality.Dominant || quality == Quality.Augmented
                            ? IntervalQuality.Minor
                            : ChordToIntervalQualities[quality.Value];
                    }

                    for (var i = 7; i <= extensionIntervalNumber; i += 2)
                    {
                        extensionNotes.Add(rootNoteName.Transpose(Interval.Get(intervalQualities[i], i)));
                    }
                }
                else
                {
                    if (extensionIntervalQuality != null)
                        intervalQualities[extensionIntervalNumber] = extensionIntervalQuality.Value;

                    extensionNotes.Add(rootNoteName.Transpose(Interval.Get(intervalQualities[extensionIntervalNumber], extensionIntervalNumber)));
                }
            }

            if (quality == Quality.HalfDiminished)
                quality = Quality.Diminished;
            else if (quality == Quality.Dominant)
                quality = Quality.Major;

            if (quality == null)
                quality = Quality.Major;

            //

            var notes = Chord.GetByTriad(rootNoteName, ChordQualities[quality.Value]).NotesNames.Concat(extensionNotes).ToArray();
            if (extensionIntervalNumber == 5)
                notes = new[] { rootNoteName, extensionNotes.First() };

            //

            var bassNoteGroup = match.Groups[BassNoteNameGroupName];
            if (bassNoteGroup.Success)
            {
            }

            //

            chord = new Chord(notes);
            return ParsingResult.Parsed;
        }

        private static ParsingResult TryParseChordIntervals(Match match, NoteName rootNoteName, out Chord chord)
        {
            chord = null;

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

            var intervals = intervalsParsingResults.Select(r => r.Interval).ToArray();

            chord = new Chord(rootNoteName, intervals);
            return ParsingResult.Parsed;
        }

        #endregion
    }
}
