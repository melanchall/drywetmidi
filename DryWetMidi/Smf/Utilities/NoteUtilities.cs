using Melanchall.DryWetMidi.Common;
using System;
using System.ComponentModel;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Set of extension methods for <see cref="NoteOnEvent"/> and <see cref="NoteOffEvent"/> events.
    /// </summary>
    public static class NoteUtilities
    {
        #region Constants

        private const int OctaveSize = 12;
        private const int OctaveOffset = 1;

        #endregion

        #region Methods

        /// <summary>
        /// Gets name of the note presented by an instance of <see cref="NoteOnEvent"/>.
        /// </summary>
        /// <param name="noteOnEvent">Note On event to get note name of.</param>
        /// <returns>Note name of the <paramref name="noteOnEvent"/> event.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="noteOnEvent"/> is null.</exception>
        public static NoteName GetNoteName(this NoteOnEvent noteOnEvent)
        {
            if (noteOnEvent == null)
                throw new ArgumentNullException(nameof(noteOnEvent));

            return GetNoteName(noteOnEvent.NoteNumber);
        }

        /// <summary>
        /// Gets octave of the note presented by an instance of <see cref="NoteOnEvent"/>.
        /// </summary>
        /// <param name="noteOnEvent">Note On event to get note octave of.</param>
        /// <returns>Note octave of the <paramref name="noteOnEvent"/> event.</returns>
        /// <remarks>
        /// Octave number will be returned in scientific pitch notation which means
        /// that 4 will be returned for 60 note number.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="noteOnEvent"/> is null.</exception>
        public static int GetNoteOctave(this NoteOnEvent noteOnEvent)
        {
            if (noteOnEvent == null)
                throw new ArgumentNullException(nameof(noteOnEvent));

            return GetNoteOctave(noteOnEvent.NoteNumber);
        }

        /// <summary>
        /// Gets name of the note presented by an instance of <see cref="NoteOffEvent"/>.
        /// </summary>
        /// <param name="noteOffEvent">Note Off event to get note name of.</param>
        /// <returns>Note name of the <paramref name="noteOffEvent"/> event.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="noteOffEvent"/> is null.</exception>
        public static NoteName GetNoteName(this NoteOffEvent noteOffEvent)
        {
            if (noteOffEvent == null)
                throw new ArgumentNullException(nameof(noteOffEvent));

            return GetNoteName(noteOffEvent.NoteNumber);
        }

        /// <summary>
        /// Gets octave of the note presented by an instance of <see cref="NoteOffEvent"/>.
        /// </summary>
        /// <param name="noteOffEvent">Note Off event to get note octave of.</param>
        /// <returns>Note octave of the <paramref name="noteOffEvent"/> event.</returns>
        /// <remarks>
        /// Octave number will be returned in scientific pitch notation which means
        /// that 4 will be returned for 60 note number.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="noteOffEvent"/> is null.</exception>
        public static int GetNoteOctave(this NoteOffEvent noteOffEvent)
        {
            if (noteOffEvent == null)
                throw new ArgumentNullException(nameof(noteOffEvent));

            return GetNoteOctave(noteOffEvent.NoteNumber);
        }

        /// <summary>
        /// Sets the note number of the <see cref="NoteOnEvent"/> with the specified note name and octave.
        /// </summary>
        /// <param name="noteOnEvent">Note On event to set the note number of.</param>
        /// <param name="noteName">Name of the note.</param>
        /// <param name="octave">Number of the octave.</param>
        /// <remarks>
        /// Octave number is specified in scientific pitch notation which means that 4 must be
        /// passed to get the number of the middle C.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="noteOnEvent"/> is null.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="noteName"/> specified an
        /// invalid value.</exception>
        /// <exception cref="ArgumentException">Note number is out of range for the specified note
        /// name and octave.</exception>
        public static void SetNoteNumber(this NoteOnEvent noteOnEvent, NoteName noteName, int octave)
        {
            if (noteOnEvent == null)
                throw new ArgumentNullException(nameof(noteOnEvent));

            noteOnEvent.NoteNumber = GetNoteNumber(noteName, octave);
        }

        /// <summary>
        /// Sets the note number of the <see cref="NoteOffEvent"/> with the specified note name and octave.
        /// </summary>
        /// <param name="noteOffEvent">Note Off event to set the note number of.</param>
        /// <param name="noteName">Name of the note.</param>
        /// <param name="octave">Number of the octave.</param>
        /// <remarks>
        /// Octave number is specified in scientific pitch notation which means that 4 must be
        /// passed to get the number of the middle C.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="noteOffEvent"/> is null.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="noteName"/> specified an
        /// invalid value.</exception>
        /// <exception cref="ArgumentException">Note number is out of range for the specified note
        /// name and octave.</exception>
        public static void SetNoteNumber(this NoteOffEvent noteOffEvent, NoteName noteName, int octave)
        {
            if (noteOffEvent == null)
                throw new ArgumentNullException(nameof(noteOffEvent));

            noteOffEvent.NoteNumber = GetNoteNumber(noteName, octave);
        }

        /// <summary>
        /// Gets name of the note presented by note number.
        /// </summary>
        /// <param name="noteNumber">Note number to get note name of.</param>
        /// <returns>Name of the note presented by <paramref name="noteNumber"/>.</returns>
        public static NoteName GetNoteName(SevenBitNumber noteNumber)
        {
            return (NoteName)(noteNumber % OctaveSize);
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
            return noteNumber / OctaveSize - OctaveOffset;
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
            if (!Enum.IsDefined(typeof(NoteName), noteName))
                throw new InvalidEnumArgumentException(nameof(noteName), (int)noteName, typeof(NoteName));

            var noteNumber = (octave + OctaveOffset) * OctaveSize + (int)noteName;
            if (noteNumber < SevenBitNumber.MinValue || noteNumber > SevenBitNumber.MaxValue)
                throw new ArgumentException("Note number is out of range for the specified note name and octave.", nameof(octave));

            return (SevenBitNumber)noteNumber;
        }

        #endregion
    }
}
