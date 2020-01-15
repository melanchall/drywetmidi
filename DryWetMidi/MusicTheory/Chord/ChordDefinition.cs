using System.Collections.Generic;
using System.Linq;
using System.Text;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    public sealed class ChordDefinition
    {
        #region Nested classes

        public sealed class IntervalAlteration
        {
            #region Constructor

            public IntervalAlteration(int intervalNumber, Accidental alteration)
            {
                IntervalNumber = intervalNumber;
                Alteration = alteration;
            }

            #endregion

            #region Properties

            public int IntervalNumber { get; }

            public Accidental Alteration { get; }

            #endregion

            #region Operators

            public static bool operator ==(IntervalAlteration intervalAlteration1, IntervalAlteration intervalAlteration2)
            {
                if (ReferenceEquals(intervalAlteration1, intervalAlteration2))
                    return true;

                if (ReferenceEquals(null, intervalAlteration1) || ReferenceEquals(null, intervalAlteration2))
                    return false;

                return intervalAlteration1.IntervalNumber == intervalAlteration2.IntervalNumber &&
                       intervalAlteration1.Alteration == intervalAlteration2.Alteration;
            }

            public static bool operator !=(IntervalAlteration intervalAlteration1, IntervalAlteration intervalAlteration2)
            {
                return !(intervalAlteration1 == intervalAlteration2);
            }

            #endregion

            #region Overrides

            public override string ToString()
            {
                return $"{(Alteration == Accidental.Sharp ? Note.SharpShortString : Note.FlatShortString)}{IntervalNumber}";
            }

            public override bool Equals(object obj)
            {
                return this == (obj as IntervalAlteration);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var result = 17;
                    result = result * 23 + IntervalNumber.GetHashCode();
                    result = result * 23 + Alteration.GetHashCode();
                    return result;
                }
            }

            #endregion
        }

        #endregion

        #region Constants

        private static readonly Dictionary<ChordQuality, string> QualitiesSymbols = new Dictionary<ChordQuality, string>
        {
            [ChordQuality.Minor] = "m",
            [ChordQuality.Major] = "M",
            [ChordQuality.Augmented] = "aug",
            [ChordQuality.Diminished] = "dim"
        };

        internal static readonly Dictionary<IntervalQuality, string> IntervalQualitiesSymbols = new Dictionary<IntervalQuality, string>
        {
            [IntervalQuality.Perfect] = string.Empty,
            [IntervalQuality.Minor] = "m",
            [IntervalQuality.Major] = "M",
            [IntervalQuality.Augmented] = "aug",
            [IntervalQuality.Diminished] = "dim"
        };

        #endregion

        #region Fields

        private int? _suspensionIntervalNumber;

        #endregion

        #region Constructor

        public ChordDefinition(NoteName rootNoteName)
        {
            RootNoteName = rootNoteName;
        }

        #endregion

        #region Properties

        public NoteName RootNoteName { get; }

        public ChordQuality? Quality { get; set; }

        public NoteName? BassNoteName { get; set; }

        public IntervalDefinition ExtensionInterval { get; set; }

        public Accidental? ExtensionAlteration { get; set; }

        public ICollection<IntervalDefinition> AddedToneIntervals { get; private set; } = new List<IntervalDefinition>();

        public ICollection<IntervalAlteration> AlteredIntervals { get; private set; } = new List<IntervalAlteration>();

        public IntervalQuality SeventhQuality { get; set; }

        public int? SuspensionIntervalNumber
        {
            get { return _suspensionIntervalNumber; }
            set
            {
                if (value != null)
                    ThrowIfArgument.IsOutOfRange(nameof(value), value.Value, "Value is neither 2 nor 4.", 2, 4);

                _suspensionIntervalNumber = value;
            }
        }

        #endregion

        #region Operators

        public static bool operator ==(ChordDefinition chordDefinition1, ChordDefinition chordDefinition2)
        {
            if (ReferenceEquals(chordDefinition1, chordDefinition2))
                return true;

            if (ReferenceEquals(null, chordDefinition1) || ReferenceEquals(null, chordDefinition2))
                return false;

            return chordDefinition1.RootNoteName == chordDefinition2.RootNoteName &&
                   chordDefinition1.Quality == chordDefinition2.Quality &&
                   chordDefinition1.BassNoteName == chordDefinition2.BassNoteName &&
                   chordDefinition1.ExtensionInterval == chordDefinition2.ExtensionInterval &&
                   chordDefinition1.AddedToneIntervals.SequenceEqual(chordDefinition2.AddedToneIntervals) &&
                   chordDefinition1.AlteredIntervals.SequenceEqual(chordDefinition2.AlteredIntervals) &&
                   chordDefinition1.SuspensionIntervalNumber == chordDefinition2.SuspensionIntervalNumber &&
                   chordDefinition1.SeventhQuality == chordDefinition2.SeventhQuality;
        }

        public static bool operator !=(ChordDefinition chordDefinition1, ChordDefinition chordDefinition2)
        {
            return !(chordDefinition1 == chordDefinition2);
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            var stringBuilder = new StringBuilder(RootNoteName.ToString());

            var quality = Quality;
            if (quality != null)
            {
                var qualityValue = quality.Value;
                stringBuilder.Append(qualityValue != ChordQuality.Major ? QualitiesSymbols[qualityValue] : string.Empty);
            }

            var extensionInterval = ExtensionInterval;
            if (extensionInterval != null)
            {
                var extensionQuality = extensionInterval.Quality;
                var extensionNumber = extensionInterval.Number;
                var extensionAlteration = ExtensionAlteration;

                if (extensionAlteration == null)
                {
                    var printQuality = (extensionNumber != 7 && extensionQuality != IntervalQuality.Major) ||
                                       (extensionNumber == 7 && extensionQuality != IntervalQuality.Minor);
                    stringBuilder.Append($"{(printQuality ? IntervalQualitiesSymbols[extensionQuality] : string.Empty)}{extensionNumber}");
                }
                else
                {
                    stringBuilder.Append($"7{(extensionAlteration == Accidental.Sharp ? Note.SharpShortString : Note.FlatShortString)}{extensionNumber}");
                }
            }

            var suspensionIntervalNumber = SuspensionIntervalNumber;
            if (suspensionIntervalNumber != null)
                stringBuilder.Append($"sus{suspensionIntervalNumber}");

            stringBuilder.Append(string.Join(string.Empty, AddedToneIntervals.Where(i => !AlteredIntervals.Any(t => t.IntervalNumber == i.Number)).Select(i => $"add{i.Number}")));

            stringBuilder.Append(string.Join(string.Empty, AlteredIntervals));

            var bassNoteName = BassNoteName;
            if (bassNoteName != null)
                stringBuilder.Append($"/{bassNoteName.Value.ToString().Replace(Note.SharpLongString, Note.SharpShortString)}");

            return stringBuilder.ToString();
        }

        public override bool Equals(object obj)
        {
            return this == (obj as ChordDefinition);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = 17;
                result = result * 23 + RootNoteName.GetHashCode();
                result = result * 23 + (Quality?.GetHashCode() ?? 0);
                result = result * 23 + (BassNoteName?.GetHashCode() ?? 0);
                result = result * 23 + (ExtensionInterval?.GetHashCode() ?? 0);
                result = result * 23 + (AddedToneIntervals.FirstOrDefault()?.GetHashCode() ?? 0);
                result = result * 23 + (AlteredIntervals.FirstOrDefault()?.GetHashCode() ?? 0);
                result = result * 23 + (SuspensionIntervalNumber?.GetHashCode() ?? 0);
                result = result * 23 + SeventhQuality.GetHashCode();
                return result;
            }
        }

        #endregion
    }
}
