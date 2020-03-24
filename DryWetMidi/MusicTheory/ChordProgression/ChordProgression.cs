using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    /// <summary>
    /// Represents a chord progression as a set of chords.
    /// </summary>
    public sealed class ChordProgression
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ChordProgression"/> with the specified chords.
        /// </summary>
        /// <param name="chords">Chords of the chord progression.</param>
        /// <exception cref="ArgumentNullException"><paramref name="chords"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="chords"/> contains <c>null</c>.</exception>
        public ChordProgression(IEnumerable<Chord> chords)
        {
            ThrowIfArgument.IsNull(nameof(chords), chords);
            ThrowIfArgument.ContainsNull(nameof(chords), chords);

            Chords = chords;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChordProgression"/> with the specified chords.
        /// </summary>
        /// <param name="chords">Chords of the chord progression.</param>
        /// <exception cref="ArgumentNullException"><paramref name="chords"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="chords"/> contains <c>null</c>.</exception>
        public ChordProgression(params Chord[] chords)
            : this(chords as IEnumerable<Chord>)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the chords of the chord progression.
        /// </summary>
        public IEnumerable<Chord> Chords { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Converts the string representation of a chord progression to its <see cref="ChordProgression"/> equivalent.
        /// A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="input">A string containing a chord progression to convert.</param>
        /// <param name="scale">Scale to resolve chords.</param>
        /// <param name="chordProgression">When this method returns, contains the <see cref="ChordProgression"/>
        /// equivalent of the chord progression contained in <paramref name="input"/>, if the conversion succeeded,
        /// or <c>null</c> if the conversion failed. The conversion fails if the <paramref name="input"/> is <c>null</c> or
        /// <see cref="string.Empty"/>, or is not of the correct format. This parameter is passed uninitialized;
        /// any value originally supplied in result will be overwritten.</param>
        /// <returns><c>true</c> if <paramref name="input"/> was converted successfully; otherwise, <c>false</c>.</returns>
        public static bool TryParse(string input, Scale scale, out ChordProgression chordProgression)
        {
            return ParsingUtilities.TryParse(input, GetParsing(input, scale), out chordProgression);
        }

        /// <summary>
        /// Converts the string representation of a chord progression to its <see cref="ChordProgression"/> equivalent.
        /// </summary>
        /// <param name="input">A string containing a chord progression to convert.</param>
        /// <param name="scale">Scale to resolve chords.</param>
        /// <returns>A <see cref="ChordProgression"/> equivalent to the chord progression contained in
        /// <paramref name="input"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="input"/> is <c>null</c> or contains white-spaces only.</exception>
        /// <exception cref="FormatException"><paramref name="input"/> has invalid format.</exception>
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

        /// <summary>
        /// Determines if two <see cref="ChordProgression"/> objects are equal.
        /// </summary>
        /// <param name="chordProgression1">The first <see cref="ChordProgression"/> to compare.</param>
        /// <param name="chordProgression2">The second <see cref="ChordProgression"/> to compare.</param>
        /// <returns><c>true</c> if the chord progressions are equal, <c>false</c> otherwise.</returns>
        public static bool operator ==(ChordProgression chordProgression1, ChordProgression chordProgression2)
        {
            if (ReferenceEquals(chordProgression1, chordProgression2))
                return true;

            if (ReferenceEquals(null, chordProgression1) || ReferenceEquals(null, chordProgression2))
                return false;

            return chordProgression1.Chords.SequenceEqual(chordProgression2.Chords);
        }

        /// <summary>
        /// Determines if two <see cref="ChordProgression"/> objects are not equal.
        /// </summary>
        /// <param name="chordProgression1">The first <see cref="ChordProgression"/> to compare.</param>
        /// <param name="chordProgression2">The second <see cref="ChordProgression"/> to compare.</param>
        /// <returns><c>false</c> if the chord progressions are equal, <c>true</c> otherwise.</returns>
        public static bool operator !=(ChordProgression chordProgression1, ChordProgression chordProgression2)
        {
            return !(chordProgression1 == chordProgression2);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return string.Join("; ", Chords);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return this == (obj as ChordProgression);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
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
