using System;

namespace Melanchall.DryMidi
{
    /// <summary>
    /// Set of extension methods for <see cref="NoteOnMessage"/> and <see cref="NoteOffMessage"/>
    /// messages.
    /// </summary>
    public static class NoteUtilities
    {
        private const int OctaveSize = 12;

        /// <summary>
        /// Gets letter of the note presented by an instance of <see cref="NoteOnMessage"/>.
        /// </summary>
        /// <param name="noteOnMessage">Note On message to get note letter of.</param>
        /// <returns>Note letter of the <paramref name="noteOnMessage"/> message.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="noteOnMessage"/> is null.</exception>
        public static NoteLetter GetNoteLetter(this NoteOnMessage noteOnMessage)
        {
            if (noteOnMessage == null)
                throw new ArgumentNullException(nameof(noteOnMessage));

            return GetNoteLetter(noteOnMessage.NoteNumber);
        }

        /// <summary>
        /// Gets octave of the note presented by an instance of <see cref="NoteOnMessage"/>.
        /// </summary>
        /// <param name="noteOnMessage">Note On message to get note octave of.</param>
        /// <returns>Note octave of the <paramref name="noteOnMessage"/> message.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="noteOnMessage"/> is null.</exception>
        public static int GetNoteOctave(this NoteOnMessage noteOnMessage)
        {
            if (noteOnMessage == null)
                throw new ArgumentNullException(nameof(noteOnMessage));

            return GetNoteOctave(noteOnMessage.NoteNumber);
        }

        /// <summary>
        /// Gets letter of the note presented by an instance of <see cref="NoteOffMessage"/>.
        /// </summary>
        /// <param name="noteOffMessage">Note Off message to get note letter of.</param>
        /// <returns>Note letter of the <paramref name="noteOffMessage"/> message.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="noteOffMessage"/> is null.</exception>
        public static NoteLetter GetNoteLetter(this NoteOffMessage noteOffMessage)
        {
            if (noteOffMessage == null)
                throw new ArgumentNullException(nameof(noteOffMessage));

            return GetNoteLetter(noteOffMessage.NoteNumber);
        }

        /// <summary>
        /// Gets octave of the note presented by an instance of <see cref="NoteOffMessage"/>.
        /// </summary>
        /// <param name="noteOffMessage">Note Off message to get note octave of.</param>
        /// <returns>Note octave of the <paramref name="noteOffMessage"/> message.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="noteOffMessage"/> is null.</exception>
        public static int GetNoteOctave(this NoteOffMessage noteOffMessage)
        {
            if (noteOffMessage == null)
                throw new ArgumentNullException(nameof(noteOffMessage));

            return GetNoteOctave(noteOffMessage.NoteNumber);
        }

        /// <summary>
        /// Gets letter of the note presented by note number.
        /// </summary>
        /// <param name="noteNumber">Note number to get note letter of.</param>
        /// <returns>Letter of the note presented by <paramref name="noteNumber"/>.</returns>
        private static NoteLetter GetNoteLetter(SevenBitNumber noteNumber)
        {
            return (NoteLetter)(noteNumber % OctaveSize);
        }

        /// <summary>
        /// Gets octave number of the note presented by note number.
        /// </summary>
        /// <param name="noteNumber">Note number to get octave of.</param>
        /// <returns>Octave of the note presented by <paramref name="noteNumber"/>.</returns>
        private static int GetNoteOctave(SevenBitNumber noteNumber)
        {
            return noteNumber / OctaveSize;
        }
    }
}
