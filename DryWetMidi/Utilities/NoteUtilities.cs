using System;

namespace Melanchall.DryWetMidi
{
    /// <summary>
    /// Set of extension methods for <see cref="NoteOnEvent"/> and <see cref="NoteOffEvent"/> events.
    /// </summary>
    public static class NoteUtilities
    {
        private const int OctaveSize = 12;

        /// <summary>
        /// Gets letter of the note presented by an instance of <see cref="NoteOnEvent"/>.
        /// </summary>
        /// <param name="noteOnEvent">Note On event to get note letter of.</param>
        /// <returns>Note letter of the <paramref name="noteOnEvent"/> event.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="noteOnEvent"/> is null.</exception>
        public static NoteLetter GetNoteLetter(this NoteOnEvent noteOnEvent)
        {
            if (noteOnEvent == null)
                throw new ArgumentNullException(nameof(noteOnEvent));

            return GetNoteLetter(noteOnEvent.NoteNumber);
        }

        /// <summary>
        /// Gets octave of the note presented by an instance of <see cref="NoteOnEvent"/>.
        /// </summary>
        /// <param name="noteOnEvent">Note On event to get note octave of.</param>
        /// <returns>Note octave of the <paramref name="noteOnEvent"/> event.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="noteOnEvent"/> is null.</exception>
        public static int GetNoteOctave(this NoteOnEvent noteOnEvent)
        {
            if (noteOnEvent == null)
                throw new ArgumentNullException(nameof(noteOnEvent));

            return GetNoteOctave(noteOnEvent.NoteNumber);
        }

        /// <summary>
        /// Gets letter of the note presented by an instance of <see cref="NoteOffEvent"/>.
        /// </summary>
        /// <param name="noteOffEvent">Note Off event to get note letter of.</param>
        /// <returns>Note letter of the <paramref name="noteOffEvent"/> event.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="noteOffEvent"/> is null.</exception>
        public static NoteLetter GetNoteLetter(this NoteOffEvent noteOffEvent)
        {
            if (noteOffEvent == null)
                throw new ArgumentNullException(nameof(noteOffEvent));

            return GetNoteLetter(noteOffEvent.NoteNumber);
        }

        /// <summary>
        /// Gets octave of the note presented by an instance of <see cref="NoteOffEvent"/>.
        /// </summary>
        /// <param name="noteOffEvent">Note Off event to get note octave of.</param>
        /// <returns>Note octave of the <paramref name="noteOffEvent"/> event.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="noteOffEvent"/> is null.</exception>
        public static int GetNoteOctave(this NoteOffEvent noteOffEvent)
        {
            if (noteOffEvent == null)
                throw new ArgumentNullException(nameof(noteOffEvent));

            return GetNoteOctave(noteOffEvent.NoteNumber);
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
