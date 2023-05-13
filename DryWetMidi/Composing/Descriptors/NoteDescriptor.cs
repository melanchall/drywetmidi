using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Composing
{
    /// <summary>
    /// Represents an object that describes a note.
    /// </summary>
    public sealed class NoteDescriptor
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteDescriptor"/> with the specified note,
        /// velocity and length.
        /// </summary>
        /// <param name="note">Note.</param>
        /// <param name="velocity">Velocity of the note.</param>
        /// <param name="length">Length of the note.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="note"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public NoteDescriptor(MusicTheory.Note note, SevenBitNumber velocity, ITimeSpan length)
        {
            ThrowIfArgument.IsNull(nameof(note), note);
            ThrowIfArgument.IsNull(nameof(length), length);

            Note = note;
            Velocity = velocity;
            Length = length;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the note.
        /// </summary>
        public MusicTheory.Note Note { get; }

        /// <summary>
        /// Gets the velocity of the note.
        /// </summary>
        public SevenBitNumber Velocity { get; }

        /// <summary>
        /// Gets the length of the note.
        /// </summary>
        public ITimeSpan Length { get; }

        #endregion

        #region Operators

        /// <summary>
        /// Determines if two <see cref="NoteDescriptor"/> objects are equal.
        /// </summary>
        /// <param name="noteDescriptor1">The first <see cref="NoteDescriptor"/> to compare.</param>
        /// <param name="noteDescriptor2">The second <see cref="NoteDescriptor"/> to compare.</param>
        /// <returns><c>true</c> if the descriptors are equal, <c>false</c> otherwise.</returns>
        public static bool operator ==(NoteDescriptor noteDescriptor1, NoteDescriptor noteDescriptor2)
        {
            if (ReferenceEquals(noteDescriptor1, noteDescriptor2))
                return true;

            if (ReferenceEquals(null, noteDescriptor1) || ReferenceEquals(null, noteDescriptor2))
                return false;

            return noteDescriptor1.Note == noteDescriptor2.Note &&
                   noteDescriptor1.Velocity == noteDescriptor2.Velocity &&
                   noteDescriptor1.Length.Equals(noteDescriptor2.Length);
        }

        /// <summary>
        /// Determines if two <see cref="NoteDescriptor"/> objects are not equal.
        /// </summary>
        /// <param name="noteDescriptor1">The first <see cref="NoteDescriptor"/> to compare.</param>
        /// <param name="noteDescriptor2">The second <see cref="NoteDescriptor"/> to compare.</param>
        /// <returns><c>false</c> if the descriptors are equal, <c>true</c> otherwise.</returns>
        public static bool operator !=(NoteDescriptor noteDescriptor1, NoteDescriptor noteDescriptor2)
        {
            return !(noteDescriptor1 == noteDescriptor2);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{Note} [{Velocity}]: {Length}";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return this == (obj as NoteDescriptor);
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
                result = result * 23 + Note.GetHashCode();
                result = result * 23 + Velocity.GetHashCode();
                result = result * 23 + Length.GetHashCode();
                return result;
            }
        }

        #endregion
    }
}
