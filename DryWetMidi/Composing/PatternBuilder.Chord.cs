using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Melanchall.DryWetMidi.Composing
{
    public sealed partial class PatternBuilder
    {
        #region Methods

        /// <summary>
        /// Adds a chord using the specified octave, length and velocity.
        /// </summary>
        /// <param name="chord">Chord to add.</param>
        /// <param name="octave">Octave to resolve chord's notes. <c>null</c>
        /// can be passed to use the <see cref="Octave"/> property value (see <see cref="SetOctave"/>).</param>
        /// <param name="length">Chord's notes length. <c>null</c> can be passed
        /// to use the <see cref="NoteLength"/> property value (see <see cref="SetNoteLength"/>).</param>
        /// <param name="velocity">Chord's notes velocity. <c>null</c> can be passed
        /// to use the <see cref="Velocity"/> property value (see <see cref="SetVelocity"/>).</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="chord"/> is <c>null</c>.</exception>
        public PatternBuilder Chord(MusicTheory.Chord chord, Octave octave = null, ITimeSpan length = null, SevenBitNumber? velocity = null)
        {
            ThrowIfArgument.IsNull(nameof(chord), chord);

            return Chord(
                chord.ResolveNotes(octave ?? Octave),
                length ?? NoteLength,
                velocity ?? Velocity);
        }

        /// <summary>
        /// Adds a chord by the specified intervals, root note's name, length and velocity.
        /// </summary>
        /// <param name="intervals">Intervals that represent the chord.</param>
        /// <param name="rootNoteName">The root note's name of the chord. The note will be resolved using the current octave
        /// (see <see cref="SetOctave"/>).</param>
        /// <param name="length">Chord's notes length. <c>null</c> can be passed
        /// to use the <see cref="NoteLength"/> property value (see <see cref="SetNoteLength"/>).</param>
        /// <param name="velocity">Chord's notes velocity. <c>null</c> can be passed
        /// to use the <see cref="Velocity"/> property value (see <see cref="SetVelocity"/>).</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="intervals"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="rootNoteName"/> specified an invalid value.</exception>
        public PatternBuilder Chord(
            IEnumerable<Interval> intervals,
            NoteName rootNoteName,
            ITimeSpan length = null,
            SevenBitNumber? velocity = null)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(rootNoteName), rootNoteName);

            return Chord(
                intervals,
                Octave.GetNote(rootNoteName),
                length ?? NoteLength,
                velocity ?? Velocity);
        }

        /// <summary>
        /// Adds a chord by the specified intervals relative to the root note using specified
        /// length and velocity.
        /// </summary>
        /// <param name="intervals">The <see cref="Interval"/> objects which define
        /// a numbers of half steps from the <paramref name="rootNote"/>.</param>
        /// <param name="rootNote">The chord's root note. <c>null</c> can be passed
        /// to use the <see cref="RootNote"/> property value (see <see cref="SetRootNote"/>).</param>
        /// <param name="length">Chord's notes length. <c>null</c> can be passed
        /// to use the <see cref="NoteLength"/> property value (see <see cref="SetNoteLength"/>).</param>
        /// <param name="velocity">Chord's notes velocity. <c>null</c> can be passed
        /// to use the <see cref="Velocity"/> property value (see <see cref="SetVelocity"/>).</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// The result chord will contain the specified root note and notes produced by transposing
        /// the <paramref name="rootNote"/> by the <paramref name="intervals"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="intervals"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The number of result chord's note is out of valid range.</exception>
        public PatternBuilder Chord(
            IEnumerable<Interval> intervals,
            MusicTheory.Note rootNote = null,
            ITimeSpan length = null,
            SevenBitNumber? velocity = null)
        {
            ThrowIfArgument.IsNull(nameof(intervals), intervals);

            rootNote = rootNote ?? RootNote;
            return Chord(
                new[] { rootNote }.Concat(intervals.Where(i => i != null).Select(rootNote.Transpose)),
                length ?? NoteLength,
                velocity ?? Velocity);
        }

        /// <summary>
        /// Adds a chord by the specified notes names using specified velocity and length, and default octave.
        /// </summary>
        /// <param name="noteNames">Names of notes that represent a chord. The notes will be resolved using the current octave
        /// (see <see cref="SetOctave"/>).</param>
        /// <param name="length">Chord's notes length. <c>null</c> can be passed
        /// to use the <see cref="NoteLength"/> property value (see <see cref="SetNoteLength"/>).</param>
        /// <param name="velocity">Chord's notes velocity. <c>null</c> can be passed
        /// to use the <see cref="Velocity"/> property value (see <see cref="SetVelocity"/>).</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default octave use <see cref="SetOctave"/> method. By default the octave number is 4.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="noteNames"/> is <c>null</c>.</exception>
        public PatternBuilder Chord(
            IEnumerable<NoteName> noteNames,
            ITimeSpan length = null,
            SevenBitNumber? velocity = null)
        {
            ThrowIfArgument.IsNull(nameof(noteNames), noteNames);

            return Chord(
                noteNames.Select(n => Octave.GetNote(n)),
                length ?? NoteLength,
                velocity ?? Velocity);
        }

        /// <summary>
        /// Adds a chord by the specified notes using specified velocity and length.
        /// </summary>
        /// <param name="notes">Notes that represent a chord.</param>
        /// <param name="length">Chord's notes length. <c>null</c> can be passed
        /// to use the <see cref="NoteLength"/> property value (see <see cref="SetNoteLength"/>).</param>
        /// <param name="velocity">Chord's notes velocity. <c>null</c> can be passed
        /// to use the <see cref="Velocity"/> property value (see <see cref="SetVelocity"/>).</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="notes"/> is <c>null</c>.</exception>
        public PatternBuilder Chord(
            IEnumerable<MusicTheory.Note> notes,
            ITimeSpan length = null,
            SevenBitNumber? velocity = null)
        {
            ThrowIfArgument.IsNull(nameof(notes), notes);

            return AddAction(new AddChordAction(new ChordDescriptor(
                notes,
                velocity ?? Velocity,
                length ?? NoteLength)));
        }

        /// <summary>
        /// Adds a chord using the specified octave, length and velocity.
        /// </summary>
        /// <param name="chord">A chord as a string (see <see href="xref:a_mt_chord#parsing">the corresponding article</see>
        /// to learn more).</param>
        /// <param name="octave">Octave to resolve chord's notes. <c>null</c>
        /// can be passed to use the <see cref="Octave"/> property value (see <see cref="SetOctave"/>).</param>
        /// <param name="length">Chord's notes length. <c>null</c> can be passed
        /// to use the <see cref="NoteLength"/> property value (see <see cref="SetNoteLength"/>).</param>
        /// <param name="velocity">Chord's notes velocity. <c>null</c> can be passed
        /// to use the <see cref="Velocity"/> property value (see <see cref="SetVelocity"/>).</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="chord"/> is <c>null</c> or contains white-spaces only.</exception>
        /// <exception cref="FormatException"><paramref name="chord"/> has invalid format.</exception>
        public PatternBuilder Chord(
            string chord,
            Octave octave = null,
            ITimeSpan length = null,
            SevenBitNumber? velocity = null)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(chord), chord, "Chord");

            return Chord(
                MusicTheory.Chord.Parse(chord),
                octave ?? Octave,
                length ?? NoteLength,
                velocity ?? Velocity);
        }

        #endregion
    }
}
