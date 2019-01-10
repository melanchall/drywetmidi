using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    /// <summary>
    /// Represents a musical scale.
    /// </summary>
    public sealed class Scale
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Scale"/> with the
        /// specified intervals and root note.
        /// </summary>
        /// <param name="intervals">Intervals between adjacent notes of the scale.</param>
        /// <param name="rootNote">Root note (tonic) of the scale.</param>
        /// <exception cref="ArgumentNullException"><paramref name="intervals"/> is null.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="rootNote"/> specified an
        /// invalid value.</exception>
        public Scale(IEnumerable<Interval> intervals, NoteName rootNote)
        {
            ThrowIfArgument.IsNull(nameof(intervals), intervals);
            ThrowIfArgument.IsInvalidEnumValue(nameof(rootNote), rootNote);

            Intervals = intervals;
            RootNote = rootNote;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets inetrvals between adjacent notes of the current <see cref="Scale"/>.
        /// </summary>
        public IEnumerable<Interval> Intervals { get; }

        /// <summary>
        /// Gets root note (tonic) of the current <see cref="Scale"/>.
        /// </summary>
        public NoteName RootNote { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Converts the string representation of a musical scale to its <see cref="Scale"/>
        /// equivalent. A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="input">A string containing a scale to convert.</param>
        /// <param name="scale">When this method returns, contains the <see cref="Scale"/>
        /// equivalent of the musical scale contained in <paramref name="input"/>, if the conversion succeeded,
        /// or null if the conversion failed. The conversion fails if the <paramref name="input"/> is null or
        /// <see cref="string.Empty"/>, or is not of the correct format. This parameter is passed uninitialized;
        /// any value originally supplied in result will be overwritten.</param>
        /// <returns>true if <paramref name="input"/> was converted successfully; otherwise, false.</returns>
        public static bool TryParse(string input, out Scale scale)
        {
            return ScaleParser.TryParse(input, out scale).Status == ParsingStatus.Parsed;
        }

        /// <summary>
        /// Converts the string representation of a musical scale to its <see cref="Scale"/> equivalent.
        /// </summary>
        /// <param name="input">A string containing a scale to convert.</param>
        /// <returns>A <see cref="Scale"/> equivalent to the musical scale contained in <paramref name="input"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="input"/> is null or contains white-spaces only.</exception>
        /// <exception cref="FormatException"><paramref name="input"/> has invalid format.</exception>
        public static Scale Parse(string input)
        {
            Scale scale;
            var parsingResult = ScaleParser.TryParse(input, out scale);
            if (parsingResult.Status == ParsingStatus.Parsed)
                return scale;

            throw parsingResult.Exception;
        }

        #endregion

        #region Operators

        /// <summary>
        /// Determines if two <see cref="Scale"/> objects are equal.
        /// </summary>
        /// <param name="scale1">The first <see cref="Scale"/> to compare.</param>
        /// <param name="scale2">The second <see cref="Scale"/> to compare.</param>
        /// <returns>true if the scales are equal, false otherwise.</returns>
        public static bool operator ==(Scale scale1, Scale scale2)
        {
            if (ReferenceEquals(scale1, scale2))
                return true;

            if (ReferenceEquals(null, scale1) || ReferenceEquals(null, scale2))
                return false;

            return scale1.RootNote == scale2.RootNote &&
                   scale1.Intervals.SequenceEqual(scale2.Intervals);
        }

        /// <summary>
        /// Determines if two <see cref="Scale"/> objects are not equal.
        /// </summary>
        /// <param name="scale1">The first <see cref="Scale"/> to compare.</param>
        /// <param name="scale2">The second <see cref="Scale"/> to compare.</param>
        /// <returns>false if the scales are equal, true otherwise.</returns>
        public static bool operator !=(Scale scale1, Scale scale2)
        {
            return !(scale1 == scale2);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{RootNote} {string.Join(" ", Intervals)}";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return this == (obj as Scale);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return RootNote.GetHashCode() ^ Intervals.GetHashCode();
        }

        #endregion
    }
}
