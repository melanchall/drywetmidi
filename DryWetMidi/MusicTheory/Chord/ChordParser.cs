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

        private const string AddedToneNumberGroupName = "addn";
        private const string AddedToneGroupName = "add";

        private const string AlteredToneNumberGroupName = "altn";
        private const string AlteredToneAccidentalGroupName = "alta";
        private const string AlteredToneGroupName = "alt";

        private static readonly string IntervalGroup = $"(?<{IntervalGroupName}>({string.Join("|", IntervalParser.GetPatterns())})\\s*)+";
        private static readonly string RootNoteNameGroup = $"(?<{RootNoteNameGroupName}>{string.Join("|", NoteNameParser.GetPatterns())})";
        private static readonly string BassNoteNameGroup = $"(?<{BassNoteNameGroupName}>{string.Join("|", NoteNameParser.GetPatterns())})";
        private static readonly string ChordQualityGroup = $@"(?<{ChordQualityGroupName}>(?<{MajorQualityGroupName}>(?i:maj)|M)|(?<{MinorQualityGroupName}>(?i:min)|m)|(?<{AugmentedQualityGroupName}>(?i:aug)|\+)|(?<{DiminishedQualityGroupName}>(?i:dim))|(?<{HalfDiminishedQualityGroupName}>ø)|(?<{DominantQualityGroupName}>(?i:dom)))";
        private static readonly string ChordExtensionQualityGroup = $@"(?<{ExtensionQualityGroupName}>(?<{ExtensionMajorQualityGroupName}>(?i:maj)|M)|(?<{ExtensionMinorQualityGroupName}>(?i:min)|m)|(?<{ExtensionAugmentedQualityGroupName}>(?i:(aug|a)))|(?<{ExtensionDiminishedQualityGroupName}>(?i:(dim|d))))";
        private static readonly string ChordExtensionGroup = $@"(?<{ExtensionGroupName}>{ChordExtensionQualityGroup}?\s*(?<{ExtensionNumberGroupName}>\d+))";
        private static readonly string SuspendedGroup = $@"(?<{SuspendedGroupName}>(?i:sus)(?<{SuspendedNumberGroupName}>\d))";
        private static readonly string AddedToneGroup = $@"(?<{AddedToneGroupName}>(?i:add)(?<{AddedToneNumberGroupName}>\d+))";
        private static readonly string AlteredToneGroup = $@"(?<{AlteredToneGroupName}>(?<{AlteredToneAccidentalGroupName}>#|\+|b|\-)(?<{AlteredToneNumberGroupName}>\d+))";

        internal static readonly string ChordCharacteristicsGroup = $@"{ChordQualityGroup}?\s*({ChordExtensionGroup}|\(\s*{ChordExtensionGroup}\s*\)|/\s*{ChordExtensionGroup})?\s*{AlteredToneGroup}?\s*{SuspendedGroup}?\s*{AddedToneGroup}?\s*(/(?i:{BassNoteNameGroup}))?";

        private static readonly string[] Patterns = new[]
        {
            $@"(?<{ChordNameGroupName}>(?i:{RootNoteNameGroup})\s*{ChordCharacteristicsGroup})",
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
        private const string SuspensionNumberIsOutOfRange = "Suspended chord is not sus2 or sus4.";
        private const string AddedToneNumberIsOutOfRange = "Added tone number is out of range.";
        private const string AlteredToneNumberIsOutOfRange = "Altered tone number is out of range.";

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

        internal static ParsingResult TryParseChordName(Match match, NoteName rootNoteName, out Chord chord)
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
            var extensionNumbers = new List<int>();

            if (extensionIntervalNumber >= 0)
            {
                var extensionNotesDictionary = GetExtensionNotes(quality, rootNoteName, extensionIntervalNumber, extensionIntervalQuality);
                extensionNotes.AddRange(extensionNotesDictionary.Keys);
                extensionNumbers.AddRange(extensionNotesDictionary.Values);
            }

            if (quality == Quality.HalfDiminished)
                quality = Quality.Diminished;
            else if (quality == Quality.Dominant)
                quality = Quality.Major;

            if (quality == null)
                quality = Quality.Major;

            //

            var notes = new List<NoteName>(Chord.GetByTriad(rootNoteName, ChordQualities[quality.Value]).NotesNames);
            notes.AddRange(extensionNotes);
            extensionNumbers.InsertRange(0, new[] { 1, 3, 5 });
            if (extensionIntervalNumber == 5)
            {
                notes.Clear();
                notes.AddRange(new[] { rootNoteName, extensionNotes.First() });

                extensionNumbers.Clear();
                extensionNumbers.AddRange(new[] { 1, 5 });
            }

            //

            var alteredToneGroup = match.Groups[AlteredToneGroupName];
            if (alteredToneGroup.Success)
            {
                int alteredToneNumber;
                if (!ParsingUtilities.ParseInt(match, AlteredToneNumberGroupName, -1, out alteredToneNumber))
                    return ParsingResult.Error(AlteredToneNumberIsOutOfRange);

                var transposeBy = 0;

                var accidental = match.Groups[AlteredToneAccidentalGroupName].Value;
                switch (accidental)
                {
                    case "#":
                    case "+":
                        transposeBy = 1;
                        break;
                    case "b":
                    case "-":
                        transposeBy = -1;
                        break;
                }

                var maxExtensionNumber = extensionNumbers.Max();
                if (maxExtensionNumber < alteredToneNumber)
                {
                    var extensionNotesDictionary = GetExtensionNotes(quality, rootNoteName, alteredToneNumber, null)
                        .Where(kv => kv.Value > maxExtensionNumber);

                    notes.AddRange(extensionNotesDictionary.Select(kv => kv.Key));
                    extensionNumbers.AddRange(extensionNotesDictionary.Select(kv => kv.Value));
                }

                var index = extensionNumbers.IndexOf(alteredToneNumber);
                if (index >= 0)
                    notes[index] = notes[index].Transpose(Interval.FromHalfSteps(transposeBy));
            }

            //

            var suspendedGroup = match.Groups[SuspendedGroupName];
            if (suspendedGroup.Success)
            {
                int suspensionNumber;
                if (!ParsingUtilities.ParseInt(match, SuspendedNumberGroupName, -1, out suspensionNumber) ||
                    suspensionNumber != 2 && suspensionNumber != 4)
                    return ParsingResult.Error(SuspensionNumberIsOutOfRange);

                var interval = suspensionNumber == 2
                    ? Interval.Get(IntervalQuality.Major, 2)
                    : Interval.Get(IntervalQuality.Perfect, 4);
                notes[1] = rootNoteName.Transpose(interval);
            }

            //

            var addedToneGroup = match.Groups[AddedToneGroupName];
            if (addedToneGroup.Success)
            {
                int addedToneNumber;
                if (!ParsingUtilities.ParseInt(match, AddedToneNumberGroupName, -1, out addedToneNumber))
                    return ParsingResult.Error(AddedToneNumberIsOutOfRange);

                var interval = Interval.IsPerfect(addedToneNumber)
                    ? Interval.Get(IntervalQuality.Perfect, addedToneNumber)
                    : Interval.Get(IntervalQuality.Major, addedToneNumber);
                notes.Add(rootNoteName.Transpose(interval));
            }

            //

            var bassNoteNameGroup = match.Groups[BassNoteNameGroupName];
            if (bassNoteNameGroup.Success)
            {
                NoteName bassNoteName;
                var bassNoteNameParsingResult = NoteNameParser.TryParse(bassNoteNameGroup.Value, out bassNoteName);
                if (bassNoteNameParsingResult.Status != ParsingStatus.Parsed)
                    return bassNoteNameParsingResult;

                notes.Insert(0, bassNoteName);
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

        private static IDictionary<NoteName, int> GetExtensionNotes(
            Quality? quality,
            NoteName rootNoteName,
            int extensionIntervalNumber,
            IntervalQuality? extensionIntervalQuality)
        {
            var result = new Dictionary<NoteName, int>();

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
                    result.Add(rootNoteName.Transpose(Interval.Get(intervalQualities[i], i)), i);
                }
            }
            else
            {
                if (extensionIntervalQuality != null)
                    intervalQualities[extensionIntervalNumber] = extensionIntervalQuality.Value;

                result.Add(
                    rootNoteName.Transpose(Interval.Get(intervalQualities[extensionIntervalNumber], extensionIntervalNumber)),
                    extensionIntervalNumber);
            }

            return result;
        }

        #endregion
    }
}
