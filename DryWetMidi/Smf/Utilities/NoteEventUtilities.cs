using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using System;
using System.ComponentModel;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Set of extension methods for <see cref="NoteOnEvent"/> and <see cref="NoteOffEvent"/> events.
    /// </summary>
    public static class NoteEventUtilities
    {
        #region Methods

        /// <summary>
        /// Gets name of the note presented by the specified <see cref="NoteEvent"/>.
        /// </summary>
        /// <param name="noteEvent">Note event to get note name of.</param>
        /// <returns>Note name of the <paramref name="noteEvent"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="noteEvent"/> is null.</exception>
        public static NoteName GetNoteName(this NoteEvent noteEvent)
        {
            ThrowIfArgument.IsNull(nameof(noteEvent), noteEvent);

            return NoteUtilities.GetNoteName(noteEvent.NoteNumber);
        }

        /// <summary>
        /// Gets octave of the note presented by the specified <see cref="NoteOnEvent"/>.
        /// </summary>
        /// <param name="noteEvent">Note event to get note octave of.</param>
        /// <returns>Note octave of the <paramref name="noteEvent"/>.</returns>
        /// <remarks>
        /// Octave number will be returned in scientific pitch notation which means
        /// that 4 will be returned for 60 note number.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="noteEvent"/> is null.</exception>
        public static int GetNoteOctave(this NoteEvent noteEvent)
        {
            ThrowIfArgument.IsNull(nameof(noteEvent), noteEvent);

            return NoteUtilities.GetNoteOctave(noteEvent.NoteNumber);
        }

        /// <summary>
        /// Sets the note number of the <see cref="NoteEvent"/> with the specified note name and octave.
        /// </summary>
        /// <param name="noteEvent">Note event to set the note number of.</param>
        /// <param name="noteName">Name of the note.</param>
        /// <param name="octave">Number of the octave.</param>
        /// <remarks>
        /// Octave number is specified in scientific pitch notation which means that 4 must be
        /// passed to get the number of the middle C.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="noteEvent"/> is null.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="noteName"/> specified an
        /// invalid value.</exception>
        /// <exception cref="ArgumentException">Note number is out of range for the specified note
        /// name and octave.</exception>
        public static void SetNoteNumber(this NoteEvent noteEvent, NoteName noteName, int octave)
        {
            ThrowIfArgument.IsNull(nameof(noteEvent), noteEvent);

            noteEvent.NoteNumber = NoteUtilities.GetNoteNumber(noteName, octave);
        }

        #endregion
    }
}
