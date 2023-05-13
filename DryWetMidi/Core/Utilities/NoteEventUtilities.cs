using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using System;
using System.ComponentModel;

namespace Melanchall.DryWetMidi.Core
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
        /// <exception cref="ArgumentNullException"><paramref name="noteEvent"/> is <c>null</c>.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="noteEvent"/> is <c>null</c>.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="noteEvent"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="noteName"/> specified an
        /// invalid value.</exception>
        /// <exception cref="ArgumentException">Note number is out of range for the specified note
        /// name and octave.</exception>
        public static void SetNoteNumber(this NoteEvent noteEvent, NoteName noteName, int octave)
        {
            ThrowIfArgument.IsNull(nameof(noteEvent), noteEvent);

            noteEvent.NoteNumber = NoteUtilities.GetNoteNumber(noteName, octave);
        }

        /// <summary>
        /// Checks if the specified <see cref="NoteOnEvent"/> corresponds to the specified
        /// <see cref="NoteOffEvent"/>.
        /// </summary>
        /// <remarks>
        /// Note On event corresponds to Note Off one if it has the same note's number and channel,
        /// i.e. those events make up a note.
        /// </remarks>
        /// <param name="noteOnEvent"><see cref="NoteOnEvent"/> to check <see cref="NoteOffEvent"/> for.</param>
        /// <param name="noteOffEvent"><see cref="NoteOffEvent"/> to check <see cref="NoteOnEvent"/> for.</param>
        /// <returns><c>true</c> if <paramref name="noteOnEvent"/> corresponds to <paramref name="noteOffEvent"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="noteOnEvent"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="noteOffEvent"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static bool IsNoteOnCorrespondToNoteOff(NoteOnEvent noteOnEvent, NoteOffEvent noteOffEvent)
        {
            ThrowIfArgument.IsNull(nameof(noteOnEvent), noteOnEvent);
            ThrowIfArgument.IsNull(nameof(noteOffEvent), noteOffEvent);

            return noteOnEvent.Channel == noteOffEvent.Channel &&
                   noteOnEvent.NoteNumber == noteOffEvent.NoteNumber;
        }

        #endregion
    }
}
