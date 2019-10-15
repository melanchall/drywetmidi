using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    public sealed class ChordProgression
    {
        #region Constructor

        public ChordProgression(IEnumerable<Chord> chords)
        {
            ThrowIfArgument.IsNull(nameof(chords), chords);
            ThrowIfArgument.ContainsNull(nameof(chords), chords);

            Chords = chords;
        }

        public ChordProgression(params Chord[] chords)
            : this(chords as IEnumerable<Chord>)
        {
        }

        #endregion

        #region Properties

        public IEnumerable<Chord> Chords { get; }

        #endregion

        #region Methods

        public static bool TryParse(string input, Scale scale, out ChordProgression chordProgression)
        {
            return ParsingUtilities.TryParse(input, GetParsing(input, scale), out chordProgression);
        }

        public static ChordProgression Parse(string input, Scale scale)
        {
            return ParsingUtilities.Parse(input, GetParsing(input, scale));
        }

        private static Parsing<ChordProgression> GetParsing(string input, Scale scale)
        {
            ChordProgression chordProgression;
            var result = ChordProgressionParser.TryParse(input, scale, out chordProgression);
            return (string i, out ChordProgression cp) =>
            {
                cp = chordProgression;
                return result;
            };
        }

        #endregion

        #region Operators

        public static bool operator ==(ChordProgression chordProgression1, ChordProgression chordProgression2)
        {
            if (ReferenceEquals(chordProgression1, chordProgression2))
                return true;

            if (ReferenceEquals(null, chordProgression1) || ReferenceEquals(null, chordProgression2))
                return false;

            return chordProgression1.Chords.SequenceEqual(chordProgression2.Chords);
        }

        public static bool operator !=(ChordProgression chordProgression1, ChordProgression chordProgression2)
        {
            return !(chordProgression1 == chordProgression2);
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return string.Join("; ", Chords);
        }

        public override bool Equals(object obj)
        {
            return this == (obj as ChordProgression);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = 17;

                foreach (var chord in Chords)
                {
                    result = result * 23 + chord.GetHashCode();
                }

                return result;
            }
        }

        #endregion
    }
}
