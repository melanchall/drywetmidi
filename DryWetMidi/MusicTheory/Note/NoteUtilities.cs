using System;
using System.ComponentModel;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    /// <summary>
    /// Provides utilities for working with the <see cref="Note"/>.
    /// </summary>
    public static class NoteUtilities
    {
        #region Constants

        private const int OctaveOffset = 1;

        #endregion

        #region Methods

        /// <summary>
        /// Transposes note name by the specified interval.
        /// </summary>
        /// <param name="noteName"><see cref="NoteName"/> to transpose.</param>
        /// <param name="interval">Interval to transpose by.</param>
        /// <returns>Note name which is <paramref name="noteName"/> transposed by <paramref name="interval"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="noteName"/> specified an invalid value.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="interval"/> is <c>null</c>.</exception>
        public static NoteName Transpose(this NoteName noteName, Interval interval)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(noteName), noteName);
            ThrowIfArgument.IsNull(nameof(interval), interval);

            var noteNumber = ((int)noteName + interval) % Octave.OctaveSize;
            if (noteNumber < 0)
                noteNumber += Octave.OctaveSize;

            return (NoteName)noteNumber;
        }

        /// <summary>
        /// Gets name of the note presented by note number.
        /// </summary>
        /// <param name="noteNumber">Note number to get note name of.</param>
        /// <returns>Name of the note presented by <paramref name="noteNumber"/>.</returns>
        public static NoteName GetNoteName(SevenBitNumber noteNumber)
        {
            return (NoteName)(noteNumber % Octave.OctaveSize);
        }

        /// <summary>
        /// Gets octave number of the note presented by note number in scientific pitch notation.
        /// </summary>
        /// <param name="noteNumber">Note number to get octave of.</param>
        /// <returns>Octave of the note presented by <paramref name="noteNumber"/>.</returns>
        /// <remarks>
        /// Octave number will be returned in scientific pitch notation which means
        /// that 4 will be returned for 60 note number.
        /// </remarks>
        public static int GetNoteOctave(SevenBitNumber noteNumber)
        {
            return noteNumber / Octave.OctaveSize - OctaveOffset;
        }

        /// <summary>
        /// Gets the note number for the specified note name and octave.
        /// </summary>
        /// <param name="noteName">Name of the note.</param>
        /// <param name="octave">Number of the octave in scientific pitch notation.</param>
        /// <returns>Number of the note represented by specified name and octave.</returns>
        /// <remarks>
        /// Octave number is specified in scientific pitch notation which means that 4 must be
        /// passed to <paramref name="octave"/> to get the number of the middle C.
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="noteName"/> specified an
        /// invalid value.</exception>
        /// <exception cref="ArgumentException">Note number is out of range for the specified note
        /// name and octave.</exception>
        public static SevenBitNumber GetNoteNumber(NoteName noteName, int octave)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(noteName), noteName);

            var noteNumber = CalculateNoteNumber(noteName, octave);
            if (!IsNoteNumberValid(noteNumber))
                throw new ArgumentException("Note number is out of range for the specified note name and octave.", nameof(octave));

            return (SevenBitNumber)noteNumber;
        }

        internal static bool IsNoteValid(NoteName noteName, int octave)
        {
            return IsNoteNumberValid(CalculateNoteNumber(noteName, octave));
        }

        internal static bool IsNoteNumberValid(int noteNumber)
        {
            return noteNumber >= SevenBitNumber.MinValue && noteNumber <= SevenBitNumber.MaxValue;
        }

        private static int CalculateNoteNumber(NoteName noteName, int octave)
        {
            return (octave + OctaveOffset) * Octave.OctaveSize + (int)noteName;
        }

        #endregion
    }
}
