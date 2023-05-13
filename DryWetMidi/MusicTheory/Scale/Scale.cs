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
        /// <exception cref="ArgumentNullException"><paramref name="intervals"/> is <c>null</c>.</exception>
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
        /// Gets intervals between adjacent notes of the current <see cref="Scale"/>.
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
        /// or <c>null</c> if the conversion failed. The conversion fails if the <paramref name="input"/> is <c>null</c> or
        /// <see cref="string.Empty"/>, or is not of the correct format. This parameter is passed uninitialized;
        /// any value originally supplied in result will be overwritten.</param>
        /// <returns><c>true</c> if <paramref name="input"/> was converted successfully; otherwise, <c>false</c>.</returns>
        public static bool TryParse(string input, out Scale scale)
        {
            return ParsingUtilities.TryParse(input, ScaleParser.TryParse, out scale);
        }

        /// <summary>
        /// Converts the string representation of a musical scale to its <see cref="Scale"/> equivalent.
        /// </summary>
        /// <param name="input">A string containing a scale to convert.</param>
        /// <returns>A <see cref="Scale"/> equivalent to the musical scale contained in <paramref name="input"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="input"/> is <c>null</c> or contains white-spaces only.</exception>
        /// <exception cref="FormatException"><paramref name="input"/> has invalid format.</exception>
        public static Scale Parse(string input)
        {
            return ParsingUtilities.Parse<Scale>(input, ScaleParser.TryParse);
        }

        #endregion

        #region Operators

        /// <summary>
        /// Determines if two <see cref="Scale"/> objects are equal.
        /// </summary>
        /// <param name="scale1">The first <see cref="Scale"/> to compare.</param>
        /// <param name="scale2">The second <see cref="Scale"/> to compare.</param>
        /// <returns><c>true</c> if the scales are equal, <c>false</c> otherwise.</returns>
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
        /// <returns><c>false</c> if the scales are equal, <c>true</c> otherwise.</returns>
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
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
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
            unchecked
            {
                var result = 17;
                result = result * 23 + RootNote.GetHashCode();
                result = result * 23 + Intervals.GetHashCode();
                return result;
            }
        }

        #endregion
    }
}
