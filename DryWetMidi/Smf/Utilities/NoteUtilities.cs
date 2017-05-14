using System;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Set of extension methods for <see cref="NoteOnEvent"/> and <see cref="NoteOffEvent"/> events.
    /// </summary>
    public static class NoteUtilities
    {
        #region Constants

        private const int OctaveSize = 12;

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
        /// <exception cref="ArgumentNullException"><paramref name="noteOffEvent"/> is null.</exception>
        public static int GetNoteOctave(this NoteOffEvent noteOffEvent)
        {
            if (noteOffEvent == null)
                throw new ArgumentNullException(nameof(noteOffEvent));

            return GetNoteOctave(noteOffEvent.NoteNumber);
        }

        /// <summary>
        /// Gets name of the note presented by note number.
        /// </summary>
        /// <param name="noteNumber">Note number to get note name of.</param>
        /// <returns>Name of the note presented by <paramref name="noteNumber"/>.</returns>
        private static NoteName GetNoteName(SevenBitNumber noteNumber)
        {
            return (NoteName)(noteNumber % OctaveSize);
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

        #endregion
    }
}
