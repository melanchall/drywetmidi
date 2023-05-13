using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Composing
{
    /// <summary>
    /// Represents an object that describes a chord.
    /// </summary>
    public sealed class ChordDescriptor
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ChordDescriptor"/> with the specified notes,
        /// velocity and length.
        /// </summary>
        /// <param name="notes">Notes of the chord.</param>
        /// <param name="velocity">Velocity of the chord's notes.</param>
        /// <param name="length">Length of the chord.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="notes"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public ChordDescriptor(IEnumerable<MusicTheory.Note> notes, SevenBitNumber velocity, ITimeSpan length)
        {
            ThrowIfArgument.IsNull(nameof(notes), notes);
            ThrowIfArgument.IsNull(nameof(length), length);

            Notes = notes;
            Velocity = velocity;
            Length = length;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the notes of the chord.
        /// </summary>
        public IEnumerable<MusicTheory.Note> Notes { get; }

        /// <summary>
        /// Gets the velocity of the chord.
        /// </summary>
        public SevenBitNumber Velocity { get; }

        /// <summary>
        /// Gets the length of the chord.
        /// </summary>
        public ITimeSpan Length { get; }

        #endregion

        #region Operators

        /// <summary>
        /// Determines if two <see cref="ChordDescriptor"/> objects are equal.
        /// </summary>
        /// <param name="chordDescriptor1">The first <see cref="ChordDescriptor"/> to compare.</param>
        /// <param name="chordDescriptor2">The second <see cref="ChordDescriptor"/> to compare.</param>
        /// <returns><c>true</c> if the descriptors are equal, <c>false</c> otherwise.</returns>
        public static bool operator ==(ChordDescriptor chordDescriptor1, ChordDescriptor chordDescriptor2)
        {
            if (ReferenceEquals(chordDescriptor1, chordDescriptor2))
                return true;

            if (ReferenceEquals(null, chordDescriptor1) || ReferenceEquals(null, chordDescriptor2))
                return false;

            return chordDescriptor1.Notes.SequenceEqual(chordDescriptor2.Notes) &&
                   chordDescriptor1.Velocity == chordDescriptor2.Velocity &&
                   chordDescriptor1.Length.Equals(chordDescriptor2.Length);
        }

        /// <summary>
        /// Determines if two <see cref="ChordDescriptor"/> objects are not equal.
        /// </summary>
        /// <param name="chordDescriptor1">The first <see cref="ChordDescriptor"/> to compare.</param>
        /// <param name="chordDescriptor2">The second <see cref="ChordDescriptor"/> to compare.</param>
        /// <returns><c>false</c> if the descriptors are equal, <c>true</c> otherwise.</returns>
        public static bool operator !=(ChordDescriptor chordDescriptor1, ChordDescriptor chordDescriptor2)
        {
            return !(chordDescriptor1 == chordDescriptor2);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{string.Join(" ", Notes)} [{Velocity}]: {Length}";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return this == (obj as ChordDescriptor);
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
                result = result * 23 + Notes.GetHashCode();
                result = result * 23 + Velocity.GetHashCode();
                result = result * 23 + Length.GetHashCode();
                return result;
            }
        }

        #endregion
    }
}
