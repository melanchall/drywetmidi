using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Composing
{
    public sealed partial class PatternBuilder
    {
        #region Methods

        /// <summary>
        /// Adds a chord.
        /// </summary>
        /// <param name="chord">Chord to add.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// Chord's notes will be resolved according to <see cref="Octave"/>.
        /// To change octave use <see cref="SetOctave"/> method.
        /// </para>
        /// <para>
        /// Chord's notes length will be taken from <see cref="NoteLength"/>.
        /// To change notes length use <see cref="SetNoteLength(ITimeSpan)"/> method.
        /// </para>
        /// <para>
        /// Chord's notes velocity will be taken from <see cref="Velocity"/>.
        /// To change velocity use <see cref="SetVelocity(SevenBitNumber)"/> method.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="chord"/> is <c>null</c>.</exception>
        public PatternBuilder Chord(MusicTheory.Chord chord)
        {
            ThrowIfArgument.IsNull(nameof(chord), chord);

            return Chord(chord.ResolveNotes(Octave), NoteLength, Velocity);
        }

        /// <summary>
        /// Adds a chord using the specified octave.
        /// </summary>
        /// <param name="chord">Chord to add.</param>
        /// <param name="octave">Octave to resolve chord's notes.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// Chord's notes length will be taken from <see cref="NoteLength"/>.
        /// To change notes length use <see cref="SetNoteLength(ITimeSpan)"/> method.
        /// </para>
        /// <para>
        /// Chord's notes velocity will be taken from <see cref="Velocity"/>.
        /// To change velocity use <see cref="SetVelocity(SevenBitNumber)"/> method.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="chord"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="octave"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public PatternBuilder Chord(MusicTheory.Chord chord, Octave octave)
        {
            ThrowIfArgument.IsNull(nameof(chord), chord);
            ThrowIfArgument.IsNull(nameof(octave), octave);

            return Chord(chord.ResolveNotes(octave), NoteLength, Velocity);
        }

        /// <summary>
        /// Adds a chord with the specified length.
        /// </summary>
        /// <param name="chord">Chord to add.</param>
        /// <param name="length">Chord's notes length.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// Chord's notes will be resolved according to <see cref="Octave"/>.
        /// To change octave use <see cref="SetOctave"/> method.
        /// </para>
        /// <para>
        /// Chord's notes velocity will be taken from <see cref="Velocity"/>.
        /// To change velocity use <see cref="SetVelocity(SevenBitNumber)"/> method.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="chord"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public PatternBuilder Chord(MusicTheory.Chord chord, ITimeSpan length)
        {
            ThrowIfArgument.IsNull(nameof(chord), chord);
            ThrowIfArgument.IsNull(nameof(length), length);

            return Chord(chord.ResolveNotes(Octave), length, Velocity);
        }

        /// <summary>
        /// Adds a chord using the specified octave and notes length.
        /// </summary>
        /// <param name="chord">Chord to add.</param>
        /// <param name="octave">Octave to resolve chord's notes.</param>
        /// <param name="length">Chord's notes length.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// Chord's notes velocity will be taken from <see cref="Velocity"/>.
        /// To change velocity use <see cref="SetVelocity(SevenBitNumber)"/> method.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="chord"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="octave"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public PatternBuilder Chord(MusicTheory.Chord chord, Octave octave, ITimeSpan length)
        {
            ThrowIfArgument.IsNull(nameof(chord), chord);
            ThrowIfArgument.IsNull(nameof(octave), octave);
            ThrowIfArgument.IsNull(nameof(length), length);

            return Chord(chord.ResolveNotes(octave), length, Velocity);
        }

        /// <summary>
        /// Adds a chord with the specified velocity.
        /// </summary>
        /// <param name="chord">Chord to add.</param>
        /// <param name="velocity">Chord's notes velocity.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// Chord's notes will be resolved according to <see cref="Octave"/>.
        /// To change octave use <see cref="SetOctave"/> method.
        /// </para>
        /// <para>
        /// Chord's notes length will be taken from <see cref="NoteLength"/>.
        /// To change notes length use <see cref="SetNoteLength(ITimeSpan)"/> method.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="chord"/> is <c>null</c>.</exception>
        public PatternBuilder Chord(MusicTheory.Chord chord, SevenBitNumber velocity)
        {
            ThrowIfArgument.IsNull(nameof(chord), chord);

            return Chord(chord.ResolveNotes(Octave), NoteLength, velocity);
        }

        /// <summary>
        /// Adds a chord using the specified octave and velocity.
        /// </summary>
        /// <param name="chord">Chord to add.</param>
        /// <param name="octave">Octave to resolve chord's notes.</param>
        /// <param name="velocity">Chord's notes velocity.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// Chord's notes length will be taken from <see cref="NoteLength"/>.
        /// To change notes length use <see cref="SetNoteLength(ITimeSpan)"/> method.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="chord"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="octave"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public PatternBuilder Chord(MusicTheory.Chord chord, Octave octave, SevenBitNumber velocity)
        {
            ThrowIfArgument.IsNull(nameof(chord), chord);
            ThrowIfArgument.IsNull(nameof(octave), octave);

            return Chord(chord.ResolveNotes(octave), NoteLength, velocity);
        }

        /// <summary>
        /// Adds a chord with the specified notes length and velocity.
        /// </summary>
        /// <param name="chord">Chord to add.</param>
        /// <param name="length">Chord's notes length.</param>
        /// <param name="velocity">Chord's notes velocity.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// Chord's notes will be resolved according to <see cref="Octave"/>.
        /// To change octave use <see cref="SetOctave"/> method.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="chord"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public PatternBuilder Chord(MusicTheory.Chord chord, ITimeSpan length, SevenBitNumber velocity)
        {
            ThrowIfArgument.IsNull(nameof(chord), chord);
            ThrowIfArgument.IsNull(nameof(length), length);

            return Chord(chord.ResolveNotes(Octave), length, velocity);
        }

        /// <summary>
        /// Adds a chord using the specified octave, length and velocity.
        /// </summary>
        /// <param name="chord">Chord to add.</param>
        /// <param name="octave">Octave to resolve chord's notes.</param>
        /// <param name="length">Chord's notes length.</param>
        /// <param name="velocity">Chord's notes velocity.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="chord"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="octave"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public PatternBuilder Chord(MusicTheory.Chord chord, Octave octave, ITimeSpan length, SevenBitNumber velocity)
        {
            ThrowIfArgument.IsNull(nameof(chord), chord);
            ThrowIfArgument.IsNull(nameof(octave), octave);
            ThrowIfArgument.IsNull(nameof(length), length);

            return Chord(chord.ResolveNotes(octave), length, velocity);
        }

        /// <summary>
        /// Adds a chord by the specified intervals and root note's name.
        /// </summary>
        /// <param name="intervals">Intervals that represent the chord.</param>
        /// <param name="rootNoteName">The root note's name of the chord.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default octave use <see cref="SetOctave"/> method. By default the octave number is 4.
        /// To set default note length use <see cref="SetNoteLength(ITimeSpan)"/> method. By default the length
        /// is 1/4. To set default velocity use <see cref="SetVelocity(SevenBitNumber)"/> method. By default the
        /// velocity is 100.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="intervals"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="rootNoteName"/> specified an invalid value.</exception>
        public PatternBuilder Chord(IEnumerable<Interval> intervals, NoteName rootNoteName)
        {
            return Chord(intervals, rootNoteName, NoteLength, Velocity);
        }

        /// <summary>
        /// Adds a chord by the specified intervals, root note's name and length.
        /// </summary>
        /// <param name="intervals">Intervals that represent the chord.</param>
        /// <param name="rootNoteName">The root note's name of the chord.</param>
        /// <param name="length">The length of a chord.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default octave use <see cref="SetOctave"/> method. By default the octave number is 4.
        /// To set default velocity use <see cref="SetVelocity(SevenBitNumber)"/> method. By default the
        /// velocity is 100.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="intervals"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="rootNoteName"/> specified an invalid value.</exception>
        public PatternBuilder Chord(IEnumerable<Interval> intervals,
                                    NoteName rootNoteName,
                                    ITimeSpan length)
        {
            return Chord(intervals, rootNoteName, length, Velocity);
        }

        /// <summary>
        /// Adds a chord by the specified intervals, root note's name and velocity.
        /// </summary>
        /// <param name="intervals">Intervals that represent the chord.</param>
        /// <param name="rootNoteName">The root note's name of the chord.</param>
        /// <param name="velocity">The velocity of a chord.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default octave use <see cref="SetOctave"/> method. By default the octave number is 4.
        /// To set default note length use <see cref="SetNoteLength(ITimeSpan)"/> method. By default the length
        /// is 1/4.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="intervals"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="rootNoteName"/> specified an invalid value.</exception>
        public PatternBuilder Chord(IEnumerable<Interval> intervals,
                                    NoteName rootNoteName,
                                    SevenBitNumber velocity)
        {
            return Chord(intervals, rootNoteName, NoteLength, velocity);
        }

        /// <summary>
        /// Adds a chord by the specified intervals, root note's name, length and velocity.
        /// </summary>
        /// <param name="intervals">Intervals that represent the chord.</param>
        /// <param name="rootNoteName">The root note's name of the chord.</param>
        /// <param name="length">The length of a chord.</param>
        /// <param name="velocity">The velocity of a chord.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="intervals"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="rootNoteName"/> specified an invalid value.</exception>
        public PatternBuilder Chord(IEnumerable<Interval> intervals,
                                    NoteName rootNoteName,
                                    ITimeSpan length,
                                    SevenBitNumber velocity)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(rootNoteName), rootNoteName);

            return Chord(intervals, Octave.GetNote(rootNoteName), length, velocity);
        }

        /// <summary>
        /// Adds a chord by the specified intervals relative to the root note using default
        /// length and velocity.
        /// </summary>
        /// <param name="intervals">The <see cref="Interval"/> objects which define
        /// a numbers of half steps from the <paramref name="rootNote"/>.</param>
        /// <param name="rootNote">The chord's root note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// The result chord will contain the specified root note and notes produced by transposing
        /// the <paramref name="rootNote"/> by the <paramref name="intervals"/>.
        /// To set default note length use <see cref="SetNoteLength(ITimeSpan)"/> method. By default the length
        /// is 1/4. To set default velocity use <see cref="SetVelocity(SevenBitNumber)"/> method. By default the
        /// velocity is 100.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="intervals"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="rootNote"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">The number of result chord's note is out of valid range.</exception>
        public PatternBuilder Chord(IEnumerable<Interval> intervals, MusicTheory.Note rootNote)
        {
            return Chord(intervals, rootNote, NoteLength, Velocity);
        }

        /// <summary>
        /// Adds a chord by the specified intervals relative to the root note using specified
        /// length and default velocity.
        /// </summary>
        /// <param name="interval">The <see cref="Interval"/> objects which define
        /// a numbers of half steps from the <paramref name="rootNote"/>.</param>
        /// <param name="rootNote">The chord's root note.</param>
        /// <param name="length">The length of a chord.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// The result chord will contain the specified root note and notes produced by transposing
        /// the <paramref name="rootNote"/> by the <paramref name="interval"/>.
        /// To set default velocity use <see cref="SetVelocity(SevenBitNumber)"/> method. By default the
        /// velocity is 100.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="interval"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="rootNote"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">The number of result chord's note is out of valid range.</exception>
        public PatternBuilder Chord(IEnumerable<Interval> interval,
                                    MusicTheory.Note rootNote,
                                    ITimeSpan length)
        {
            return Chord(interval, rootNote, length, Velocity);
        }

        /// <summary>
        /// Adds a chord by the specified intervals relative to the root note using default
        /// length and specified velocity.
        /// </summary>
        /// <param name="intervals">The <see cref="Interval"/> objects which define
        /// a numbers of half steps from the <paramref name="rootNote"/>.</param>
        /// <param name="rootNote">The chord's root note.</param>
        /// <param name="velocity">The velocity of a chord.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// The result chord will contain the specified root note and notes produced by transposing
        /// the <paramref name="rootNote"/> by the <paramref name="intervals"/>.
        /// To set default note length use <see cref="SetNoteLength(ITimeSpan)"/> method. By default the length
        /// is 1/4.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="intervals"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="rootNote"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">The number of result chord's note is out of valid range.</exception>
        public PatternBuilder Chord(IEnumerable<Interval> intervals,
                                    MusicTheory.Note rootNote,
                                    SevenBitNumber velocity)
        {
            return Chord(intervals, rootNote, NoteLength, velocity);
        }

        /// <summary>
        /// Adds a chord by the specified intervals relative to the root note using specified
        /// length and velocity.
        /// </summary>
        /// <param name="intervals">The <see cref="Interval"/> objects which define
        /// a numbers of half steps from the <paramref name="rootNote"/>.</param>
        /// <param name="rootNote">The chord's root note.</param>
        /// <param name="length">The length of a chord.</param>
        /// <param name="velocity">The velocity of a chord.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// The result chord will contain the specified root note and notes produced by transposing
        /// the <paramref name="rootNote"/> by the <paramref name="intervals"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="intervals"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="rootNote"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">The number of result chord's note is out of valid range.</exception>
        public PatternBuilder Chord(IEnumerable<Interval> intervals,
                                    MusicTheory.Note rootNote,
                                    ITimeSpan length,
                                    SevenBitNumber velocity)
        {
            ThrowIfArgument.IsNull(nameof(intervals), intervals);
            ThrowIfArgument.IsNull(nameof(rootNote), rootNote);

            return Chord(new[] { rootNote }.Concat(intervals.Where(i => i != null).Select(rootNote.Transpose)),
                         length,
                         velocity);
        }

        /// <summary>
        /// Adds a chord by the specified notes names using default velocity, length and octave.
        /// </summary>
        /// <param name="noteNames">Names of notes that represent a chord.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default octave use <see cref="SetOctave"/> method. By default the octave number is 4.
        /// To set default note length use <see cref="SetNoteLength(ITimeSpan)"/> method. By default the length
        /// is 1/4. To set default velocity use <see cref="SetVelocity(SevenBitNumber)"/> method. By default the
        /// velocity is 100.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="noteNames"/> is <c>null</c>.</exception>
        public PatternBuilder Chord(IEnumerable<NoteName> noteNames)
        {
            return Chord(noteNames, NoteLength, Velocity);
        }

        /// <summary>
        /// Adds a chord by the specified notes names using specified length and default velocity, and default octave.
        /// </summary>
        /// <param name="noteNames">Names of notes that represent a chord.</param>
        /// <param name="length">The length of a chord.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default octave use <see cref="SetOctave"/> method. By default the octave number is 4.
        /// To set default velocity use <see cref="SetVelocity(SevenBitNumber)"/> method. By default the
        /// velocity is 100.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="noteNames"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public PatternBuilder Chord(IEnumerable<NoteName> noteNames, ITimeSpan length)
        {
            return Chord(noteNames, length, Velocity);
        }

        /// <summary>
        /// Adds a chord by the specified notes names using specified velocity and default length, and default octave.
        /// </summary>
        /// <param name="noteNames">Names of notes that represent a chord.</param>
        /// <param name="velocity">The velocity of a chord.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default octave use <see cref="SetOctave"/> method. By default the octave number is 4.
        /// To set default note length use <see cref="SetNoteLength(ITimeSpan)"/> method. By default the length
        /// is 1/4.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="noteNames"/> is <c>null</c>.</exception>
        public PatternBuilder Chord(IEnumerable<NoteName> noteNames, SevenBitNumber velocity)
        {
            return Chord(noteNames, NoteLength, velocity);
        }

        /// <summary>
        /// Adds a chord by the specified notes names using specified velocity and length, and default octave.
        /// </summary>
        /// <param name="noteNames">Names of notes that represent a chord.</param>
        /// <param name="length">The length of a chord.</param>
        /// <param name="velocity">The velocity of a chord.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default octave use <see cref="SetOctave"/> method. By default the octave number is 4.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="noteNames"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public PatternBuilder Chord(IEnumerable<NoteName> noteNames, ITimeSpan length, SevenBitNumber velocity)
        {
            ThrowIfArgument.IsNull(nameof(noteNames), noteNames);
            ThrowIfArgument.IsNull(nameof(length), length);

            return Chord(noteNames.Select(n => Octave.GetNote(n)), length, velocity);
        }

        /// <summary>
        /// Adds a chord by the specified notes using default velocity and length.
        /// </summary>
        /// <param name="notes">Notes that represent a chord.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default note length use <see cref="SetNoteLength(ITimeSpan)"/> method. By default the length
        /// is 1/4. To set default velocity use <see cref="SetVelocity(SevenBitNumber)"/> method. By default the
        /// velocity is 100.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="notes"/> is <c>null</c>.</exception>
        public PatternBuilder Chord(IEnumerable<MusicTheory.Note> notes)
        {
            return Chord(notes, NoteLength, Velocity);
        }

        /// <summary>
        /// Adds a chord by the specified notes using specified length and default velocity.
        /// </summary>
        /// <param name="notes">Notes that represent a chord.</param>
        /// <param name="length">The length of a chord.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default velocity use <see cref="SetVelocity(SevenBitNumber)"/> method. By default the
        /// velocity is 100.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="notes"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public PatternBuilder Chord(IEnumerable<MusicTheory.Note> notes, ITimeSpan length)
        {
            return Chord(notes, length, Velocity);
        }

        /// <summary>
        /// Adds a chord by the specified notes using specified velocity and default length.
        /// </summary>
        /// <param name="notes">Notes that represent a chord.</param>
        /// <param name="velocity">The velocity of a chord.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default note length use <see cref="SetNoteLength(ITimeSpan)"/> method. By default the length
        /// is 1/4.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="notes"/> is <c>null</c>.</exception>
        public PatternBuilder Chord(IEnumerable<MusicTheory.Note> notes, SevenBitNumber velocity)
        {
            return Chord(notes, NoteLength, velocity);
        }

        /// <summary>
        /// Adds a chord by the specified notes using specified velocity and length.
        /// </summary>
        /// <param name="notes">Notes that represent a chord.</param>
        /// <param name="length">The length of a chord.</param>
        /// <param name="velocity">The velocity of a chord.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="notes"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public PatternBuilder Chord(IEnumerable<MusicTheory.Note> notes, ITimeSpan length, SevenBitNumber velocity)
        {
            ThrowIfArgument.IsNull(nameof(notes), notes);
            ThrowIfArgument.IsNull(nameof(length), length);

            return AddAction(new AddChordAction(new ChordDescriptor(notes, velocity, length)));
        }

        /// <summary>
        /// Adds a chord.
        /// </summary>
        /// <param name="chord">A chord as a string (like Dmin, for example).</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// Chord's notes will be resolved according to <see cref="Octave"/>.
        /// To change octave use <see cref="SetOctave"/> method.
        /// </para>
        /// <para>
        /// Chord's notes length will be taken from <see cref="NoteLength"/>.
        /// To change notes length use <see cref="SetNoteLength(ITimeSpan)"/> method.
        /// </para>
        /// <para>
        /// Chord's notes velocity will be taken from <see cref="Velocity"/>.
        /// To change velocity use <see cref="SetVelocity(SevenBitNumber)"/> method.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentException"><paramref name="chord"/> is <c>null</c> or contains white-spaces only.</exception>
        /// <exception cref="FormatException"><paramref name="chord"/> has invalid format.</exception>
        public PatternBuilder Chord(string chord)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(chord), chord, "Chord");

            return Chord(chord, Octave, NoteLength, Velocity);
        }

        /// <summary>
        /// Adds a chord using the specified octave.
        /// </summary>
        /// <param name="chord">A chord as a string (like Dmin, for example).</param>
        /// <param name="octave">Octave to resolve chord's notes.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// Chord's notes length will be taken from <see cref="NoteLength"/>.
        /// To change notes length use <see cref="SetNoteLength(ITimeSpan)"/> method.
        /// </para>
        /// <para>
        /// Chord's notes velocity will be taken from <see cref="Velocity"/>.
        /// To change velocity use <see cref="SetVelocity(SevenBitNumber)"/> method.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentException"><paramref name="chord"/> is <c>null</c> or contains white-spaces only.</exception>
        /// <exception cref="FormatException"><paramref name="chord"/> has invalid format.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="octave"/> is <c>null</c>.</exception>
        public PatternBuilder Chord(string chord, Octave octave)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(chord), chord, "Chord");
            ThrowIfArgument.IsNull(nameof(octave), octave);

            return Chord(chord, octave, NoteLength, Velocity);
        }

        /// <summary>
        /// Adds a chord with the specified length.
        /// </summary>
        /// <param name="chord">A chord as a string (like Dmin, for example).</param>
        /// <param name="length">Chord's notes length.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// Chord's notes will be resolved according to <see cref="Octave"/>.
        /// To change octave use <see cref="SetOctave"/> method.
        /// </para>
        /// <para>
        /// Chord's notes velocity will be taken from <see cref="Velocity"/>.
        /// To change velocity use <see cref="SetVelocity(SevenBitNumber)"/> method.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentException"><paramref name="chord"/> is <c>null</c> or contains white-spaces only.</exception>
        /// <exception cref="FormatException"><paramref name="chord"/> has invalid format.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="length"/> is <c>null</c>.</exception>
        public PatternBuilder Chord(string chord, ITimeSpan length)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(chord), chord, "Chord");
            ThrowIfArgument.IsNull(nameof(length), length);

            return Chord(chord, Octave, length, Velocity);
        }

        /// <summary>
        /// Adds a chord using the specified octave and notes length.
        /// </summary>
        /// <param name="chord">A chord as a string (like Dmin, for example).</param>
        /// <param name="octave">Octave to resolve chord's notes.</param>
        /// <param name="length">Chord's notes length.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// Chord's notes velocity will be taken from <see cref="Velocity"/>.
        /// To change velocity use <see cref="SetVelocity(SevenBitNumber)"/> method.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentException"><paramref name="chord"/> is <c>null</c> or contains white-spaces only.</exception>
        /// <exception cref="FormatException"><paramref name="chord"/> has invalid format.</exception>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="octave"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public PatternBuilder Chord(string chord, Octave octave, ITimeSpan length)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(chord), chord, "Chord");
            ThrowIfArgument.IsNull(nameof(octave), octave);
            ThrowIfArgument.IsNull(nameof(length), length);

            return Chord(chord, octave, length, Velocity);
        }

        /// <summary>
        /// Adds a chord with the specified velocity.
        /// </summary>
        /// <param name="chord">A chord as a string (like Dmin, for example).</param>
        /// <param name="velocity">Chord's notes velocity.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// Chord's notes will be resolved according to <see cref="Octave"/>.
        /// To change octave use <see cref="SetOctave"/> method.
        /// </para>
        /// <para>
        /// Chord's notes length will be taken from <see cref="NoteLength"/>.
        /// To change notes length use <see cref="SetNoteLength(ITimeSpan)"/> method.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentException"><paramref name="chord"/> is <c>null</c> or contains white-spaces only.</exception>
        /// <exception cref="FormatException"><paramref name="chord"/> has invalid format.</exception>
        public PatternBuilder Chord(string chord, SevenBitNumber velocity)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(chord), chord, "Chord");

            return Chord(chord, Octave, NoteLength, velocity);
        }

        /// <summary>
        /// Adds a chord using the specified octave and velocity.
        /// </summary>
        /// <param name="chord">A chord as a string (like Dmin, for example).</param>
        /// <param name="octave">Octave to resolve chord's notes.</param>
        /// <param name="velocity">Chord's notes velocity.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// Chord's notes length will be taken from <see cref="NoteLength"/>.
        /// To change notes length use <see cref="SetNoteLength(ITimeSpan)"/> method.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentException"><paramref name="chord"/> is <c>null</c> or contains white-spaces only.</exception>
        /// <exception cref="FormatException"><paramref name="chord"/> has invalid format.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="octave"/> is <c>null</c>.</exception>
        public PatternBuilder Chord(string chord, Octave octave, SevenBitNumber velocity)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(chord), chord, "Chord");
            ThrowIfArgument.IsNull(nameof(octave), octave);

            return Chord(chord, octave, NoteLength, velocity);
        }

        /// <summary>
        /// Adds a chord with the specified notes length and velocity.
        /// </summary>
        /// <param name="chord">A chord as a string (like Dmin, for example).</param>
        /// <param name="length">Chord's notes length.</param>
        /// <param name="velocity">Chord's notes velocity.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// Chord's notes will be resolved according to <see cref="Octave"/>.
        /// To change octave use <see cref="SetOctave"/> method.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentException"><paramref name="chord"/> is <c>null</c> or contains white-spaces only.</exception>
        /// <exception cref="FormatException"><paramref name="chord"/> has invalid format.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="length"/> is <c>null</c>.</exception>
        public PatternBuilder Chord(string chord, ITimeSpan length, SevenBitNumber velocity)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(chord), chord, "Chord");
            ThrowIfArgument.IsNull(nameof(length), length);

            return Chord(chord, Octave, length, velocity);
        }

        /// <summary>
        /// Adds a chord using the specified octave, length and velocity.
        /// </summary>
        /// <param name="chord">A chord as a string (like Dmin, for example).</param>
        /// <param name="octave">Octave to resolve chord's notes.</param>
        /// <param name="length">Chord's notes length.</param>
        /// <param name="velocity">Chord's notes velocity.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="chord"/> is <c>null</c> or contains white-spaces only.</exception>
        /// <exception cref="FormatException"><paramref name="chord"/> has invalid format.</exception>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="octave"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public PatternBuilder Chord(string chord, Octave octave, ITimeSpan length, SevenBitNumber velocity)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(chord), chord, "Chord");
            ThrowIfArgument.IsNull(nameof(octave), octave);
            ThrowIfArgument.IsNull(nameof(length), length);

            return Chord(MusicTheory.Chord.Parse(chord), octave, length, velocity);
        }

        #endregion
    }
}
