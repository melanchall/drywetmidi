using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using System;
using System.ComponentModel;

namespace Melanchall.DryWetMidi.Composing
{
    public sealed partial class PatternBuilder
    {
        #region Methods

        /// <summary>
        /// Adds a note by the specified interval relative to the current root note using
        /// specified length and velocity.
        /// </summary>
        /// <param name="interval">The <see cref="Interval"/> which defines
        /// a number of half steps from the current root note (see <see cref="SetRootNote"/>).</param>
        /// <param name="length">The length of a note. <c>null</c> can be passed
        /// to use the <see cref="NoteLength"/> property value (see <see cref="SetNoteLength"/>).</param>
        /// <param name="velocity">The velocity of a note. <c>null</c> can be passed
        /// to use the <see cref="Velocity"/> property value (see <see cref="SetVelocity"/>).</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="interval"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The number of result note is out of valid range.</exception>
        public PatternBuilder Note(Interval interval, ITimeSpan length = null, SevenBitNumber? velocity = null)
        {
            ThrowIfArgument.IsNull(nameof(interval), interval);

            return Note(
                RootNote.Transpose(interval),
                length ?? NoteLength,
                velocity ?? Velocity);
        }

        /// <summary>
        /// Adds a note by the specified note name using specified velocity and length, and default octave.
        /// </summary>
        /// <param name="noteName">The name of a note to resolve using the current octave
        /// (see <see cref="SetOctave"/>).</param>
        /// <param name="length">The length of a note. <c>null</c> can be passed
        /// to use the <see cref="NoteLength"/> property value (see <see cref="SetNoteLength"/>).</param>
        /// <param name="velocity">The velocity of a note. <c>null</c> can be passed
        /// to use the <see cref="Velocity"/> property value (see <see cref="SetVelocity"/>).</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default octave use <see cref="SetOctave"/> method. By default the octave number is <see cref="DefaultOctave"/>.
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="noteName"/> specified an invalid value.</exception>
        public PatternBuilder Note(NoteName noteName, ITimeSpan length = null, SevenBitNumber? velocity = null)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(noteName), noteName);

            return Note(
                Octave.GetNote(noteName),
                length ?? NoteLength,
                velocity ?? Velocity);
        }

        /// <summary>
        /// Adds a note using specified velocity and length.
        /// </summary>
        /// <param name="note">A note.</param>
        /// <param name="length">The length of a note. <c>null</c> can be passed
        /// to use the <see cref="NoteLength"/> property value (see <see cref="SetNoteLength"/>).</param>
        /// <param name="velocity">The velocity of a note. <c>null</c> can be passed
        /// to use the <see cref="Velocity"/> property value (see <see cref="SetVelocity"/>).</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="note"/> is <c>null</c>.</exception>
        public PatternBuilder Note(MusicTheory.Note note, ITimeSpan length = null, SevenBitNumber? velocity = null)
        {
            ThrowIfArgument.IsNull(nameof(note), note);

            return AddAction(new AddNoteAction(new NoteDescriptor(
                note,
                velocity ?? Velocity,
                length ?? NoteLength)));
        }

        /// <summary>
        /// Adds a note using specified velocity and length.
        /// </summary>
        /// <param name="note">A note as a string (see <see href="xref:a_mt_note#parsing">the corresponding article</see>
        /// to learn more).</param>
        /// <param name="length">The length of a note. <c>null</c> can be passed
        /// to use the <see cref="NoteLength"/> property value (see <see cref="SetNoteLength"/>).</param>
        /// <param name="velocity">The velocity of a note. <c>null</c> can be passed
        /// to use the <see cref="Velocity"/> property value (see <see cref="SetVelocity"/>).</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="note"/> is <c>null</c> or contains white-spaces only.</exception>
        /// <exception cref="FormatException"><paramref name="note"/> has invalid format.</exception>
        public PatternBuilder Note(string note, ITimeSpan length = null, SevenBitNumber? velocity = null)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(note), note, "Note");

            return Note(
                MusicTheory.Note.Parse(note),
                length ?? NoteLength,
                velocity ?? Velocity);
        }

        #endregion
    }
}
