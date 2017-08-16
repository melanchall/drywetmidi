using Melanchall.DryWetMidi.Common;
using System;
using System.ComponentModel;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Represents a note definition, i.e. note name and octave.
    /// </summary>
    public sealed class NoteDefinition
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteDefinition"/> with the
        /// specified note name and octave number.
        /// </summary>
        /// <param name="noteName">The name of a note.</param>
        /// <param name="octave">The octave number.</param>
        /// <remarks>
        /// Octave number is specified in scientific pitch notation which means that 4 must be
        /// passed to <paramref name="octave"/> to get the middle C definition.
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="noteName"/> specified an
        /// invalid value.</exception>
        /// <exception cref="ArgumentException">Note number is out of range for the specified note
        /// name and octave.</exception>
        public NoteDefinition(NoteName noteName, int octave)
            : this(NoteUtilities.GetNoteNumber(noteName, octave))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteDefinition"/> with the
        /// specified note number.
        /// </summary>
        /// <param name="noteNumber">The number of a note (60 is middle C).</param>
        public NoteDefinition(SevenBitNumber noteNumber)
        {
            NoteNumber = noteNumber;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the note number.
        /// </summary>
        public SevenBitNumber NoteNumber { get; }

        /// <summary>
        /// Gets the note name.
        /// </summary>
        public NoteName NoteName => NoteUtilities.GetNoteName(NoteNumber);

        /// <summary>
        /// Gets the octave number of a note.
        /// </summary>
        public int Octave => NoteUtilities.GetNoteOctave(NoteNumber);

        #endregion

        #region Operators

        /// <summary>
        /// Determines if two <see cref="NoteDefinition"/> objects are equal.
        /// </summary>
        /// <param name="noteDefinition1">The first <see cref="NoteDefinition"/> to compare.</param>
        /// <param name="noteDefinition2">The second <see cref="NoteDefinition"/> to compare.</param>
        /// <returns>true if the note definitions are equal, false otherwise.</returns>
        public static bool operator ==(NoteDefinition noteDefinition1, NoteDefinition noteDefinition2)
        {
            if (ReferenceEquals(noteDefinition1, noteDefinition2))
                return true;

            if (ReferenceEquals(null, noteDefinition1) || ReferenceEquals(null, noteDefinition2))
                return false;

            return noteDefinition1.NoteNumber == noteDefinition2.NoteNumber;
        }

        /// <summary>
        /// Determines if two <see cref="NoteDefinition"/> objects are not equal.
        /// </summary>
        /// <param name="noteDefinition1">The first <see cref="NoteDefinition"/> to compare.</param>
        /// <param name="noteDefinition2">The second <see cref="NoteDefinition"/> to compare.</param>
        /// <returns>false if the note definitions are equal, true otherwise.</returns>
        public static bool operator !=(NoteDefinition noteDefinition1, NoteDefinition noteDefinition2)
        {
            return !(noteDefinition1 == noteDefinition2);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{NoteName.ToString().Replace("Sharp", "#")}{Octave}";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return this == (obj as NoteDefinition);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return NoteNumber.GetHashCode();
        }

        #endregion
    }
}
