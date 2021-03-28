using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Standards;

namespace Melanchall.DryWetMidi.Composing
{
    /// <summary>
    /// Provides a fluent API to build an instance of the <see cref="Composing.Pattern"/>.
    /// </summary>
    /// <example>
    /// <para>Following example shows how to create first four bars of Beethoven's 'Moonlight Sonata':</para>
    /// <code language="csharp">
    /// // Define a chord for bass part which is just an octave
    /// var bassChord = new[] { Interval.Twelve };
    /// 
    /// // Build the composition
    /// var pattern = new PatternBuilder()
    /// 
    ///     // The length of all main theme's notes within four first bars is
    ///     // triplet eight so set it which will free us from necessity to specify
    ///     // the length of each note explicitly
    ///     .SetNoteLength(MusicalTimeSpan.Eighth.Triplet())
    /// 
    ///     // Anchor current time (start of the pattern) to jump to it
    ///     // when we'll start to program bass part
    ///     .Anchor()
    /// 
    ///     // We will add notes relative to G#3.
    ///     // Instead of Octave.Get(3).GSharp it is possible to use Note.Get(NoteName.GSharp, 3)
    ///     .SetRootNote(Octave.Get(3).GSharp)
    /// 
    ///     // Add first three notes and repeat them seven times which will
    ///     // give us two bars of the main theme
    ///                           // G#3
    ///     .Note(Interval.Zero)  // +0  (G#3)
    ///     .Note(Interval.Five)  // +5  (C#4)
    ///     .Note(Interval.Eight) // +8  (E4)
    ///     .Repeat(3, 7)         // repeat three previous notes seven times
    /// 
    ///     // Add notes of the next two bars
    ///                           // G#3
    ///     .Note(Interval.One)   // +1  (A3)
    ///     .Note(Interval.Five)  // +5  (C#4)
    ///     .Note(Interval.Eight) // +8  (E4)
    ///     .Repeat(3, 1)         // repeat three previous notes
    ///     .Note(Interval.One)   // +1  (A3)
    ///     .Note(Interval.Six)   // +6  (D4)
    ///     .Note(Interval.Ten)   // +10 (F#4)
    ///     .Repeat(3, 1)         // repeat three previous notes
    ///                           // reaching the end of third bar
    ///     .Note(Interval.Zero)  // +0  (G#3)
    ///     .Note(Interval.Four)  // +4  (C4)
    ///     .Note(Interval.Ten)   // +10 (F#4)
    ///     .Note(Interval.Zero)  // +0  (G#3)
    ///     .Note(Interval.Five)  // +5  (C#4)
    ///     .Note(Interval.Eight) // +8  (E4)
    ///     .Note(Interval.Zero)  // +0  (G#3)
    ///     .Note(Interval.Five)  // +5  (C#4)
    ///     .Note(Intervaln.Seven) // +7  (D#4)
    ///     .Note(-Interval.Two)  // -2  (F#3)
    ///     .Note(Interval.Four)  // +4  (C4)
    ///     .Note(Interval.Seven) // +7  (D#4)
    /// 
    ///     // Now we will program bass part. To start adding notes from the
    ///     // beginning of the pattern we need to move to the anchor we set
    ///     // above
    ///     .MoveToFirstAnchor()
    /// 
    ///     // First two chords have whole length
    ///     .SetNoteLength(MusicalTimeSpan.Whole)
    /// 
    ///                                             // insert a chord relative to
    ///     .Chord(bassChord, Octave.Get(2).CSharp) // C#2 (C#2, C#3)
    ///     .Chord(bassChord, Octave.Get(1).B)      // B1  (B1, B2)
    /// 
    ///     // Remaining four chords has half length
    ///     .SetNoteLength(MusicalTimeSpan.Half)
    /// 
    ///     .Chord(bassChord, Octave.Get(1).A)      // A1  (A1, A2)
    ///     .Chord(bassChord, Octave.Get(1).FSharp) // F#1 (F#1, F#2)
    ///     .Chord(bassChord, Octave.Get(1).GSharp) // G#1 (G#1, G#2)
    ///     .Repeat()                               // repeat the previous chord
    /// 
    ///     // Build a pattern that can be then saved to a MIDI file
    ///     .Build();
    /// </code>
    /// </example>
    public sealed class PatternBuilder
    {
        #region Constants

        /// <summary>
        /// Default velocity that will be applied to all further notes and chords if it's not
        /// specified explicitly. Velocity can be altered with <see cref="SetVelocity(SevenBitNumber)"/>.
        /// </summary>
        public static readonly SevenBitNumber DefaultVelocity = Interaction.Note.DefaultVelocity;

        /// <summary>
        /// Default length that will be applied to all further notes and chords if it's not
        /// specified explicitly. The length can be altered with <see cref="SetNoteLength(ITimeSpan)"/>.
        /// </summary>
        public static readonly ITimeSpan DefaultNoteLength = MusicalTimeSpan.Quarter;

        /// <summary>
        /// Default step size that will be applied to all further move operations if it's not
        /// specified explicitly. Step size can be altered with <see cref="SetStep(ITimeSpan)"/>.
        /// </summary>
        public static readonly ITimeSpan DefaultStep = MusicalTimeSpan.Quarter;

        /// <summary>
        /// Default octave further notes and chords will be relative to if it's not
        /// specified explicitly. Octave can be altered with <see cref="SetOctave"/>.
        /// </summary>
        public static readonly Octave DefaultOctave = Octave.Middle;

        /// <summary>
        /// Default root note further notes will be based on if it's not specified explicitly.
        /// Root note can be altered with <see cref="SetRootNote(MusicTheory.Note)"/>.
        /// </summary>
        public static readonly MusicTheory.Note DefaultRootNote = Octave.Middle.C;

        #endregion

        #region Fields

        private readonly List<PatternAction> _actions = new List<PatternAction>();

        private readonly Dictionary<object, int> _anchorCounters = new Dictionary<object, int>();
        private int _globalAnchorsCounter = 0;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PatternBuilder"/>.
        /// </summary>
        public PatternBuilder()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PatternBuilder"/> with the specified pattern.
        /// </summary>
        /// <param name="pattern">Pattern to initialize builder with.</param>
        /// <remarks>
        /// <para>
        /// This constructor is equivalent to calling default one followed by
        /// <see cref="ReplayPattern(Composing.Pattern)"/> call. Using this constructor pattern builder's
        /// current position will be placed right after <paramref name="pattern"/> so all further actions
        /// will be relative to the end of <paramref name="pattern"/> rather than zero.
        /// </para>
        /// <para>
        /// To start with fresh pattern and place data starting from zero use <see cref="PatternBuilder()"/>
        /// constructor.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is <c>null</c>.</exception>
        public PatternBuilder(Pattern pattern)
        {
            ThrowIfArgument.IsNull(nameof(pattern), pattern);

            ReplayPattern(pattern);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the velocity that will be applied to all further notes and chords if it's not
        /// specified explicitly. Velocity can be altered with <see cref="SetVelocity(SevenBitNumber)"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// There are methods to add notes and chords that don't take velocity as an argument. In these
        /// cases the value of the <see cref="Velocity"/> property will be used. For example,
        /// <see cref="Note(MusicTheory.Note)"/> or <see cref="Chord(IEnumerable{Interval}, MusicTheory.Note)"/>.
        /// </para>
        /// </remarks>
        public SevenBitNumber Velocity { get; private set; } = DefaultVelocity;

        /// <summary>
        /// Gets the length that will be applied to all further notes and chords if it's not
        /// specified explicitly. The length can be altered with <see cref="SetNoteLength(ITimeSpan)"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// There are methods to add notes and chords that don't take length as an argument. In these
        /// cases the value of the <see cref="NoteLength"/> property will be used. For example,
        /// <see cref="Note(MusicTheory.Note)"/> or <see cref="Chord(IEnumerable{Interval}, MusicTheory.Note)"/>.
        /// </para>
        /// </remarks>
        public ITimeSpan NoteLength { get; private set; } = DefaultNoteLength;

        /// <summary>
        /// Gets the step size that will be applied to all further move operations if it's not
        /// specified explicitly. Step size can be altered with <see cref="SetStep(ITimeSpan)"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// There are methods to move current builder's position that don't take step as an argument. In these
        /// cases the value of the <see cref="Step"/> property will be used. For example,
        /// <see cref="StepForward()"/> or <see cref="StepBack()"/>.
        /// </para>
        /// </remarks>
        public ITimeSpan Step { get; private set; } = DefaultStep;

        /// <summary>
        /// Gets the octave further notes and chords will be relative to if it's not
        /// specified explicitly. Octave can be altered with <see cref="SetOctave"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// There are methods to add notes and chords where octave is not specified explicitly. In these
        /// cases the value of the <see cref="Octave"/> property will be used. For example,
        /// <see cref="Note(NoteName)"/> or <see cref="Chord(IEnumerable{Interval}, NoteName)"/>.
        /// </para>
        /// </remarks>
        public Octave Octave { get; private set; } = DefaultOctave;

        /// <summary>
        /// Gets the root note further notes will be based on if it's not specified explicitly.
        /// Root note can be altered with <see cref="SetRootNote(MusicTheory.Note)"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// There are methods to add notes by interval where root note is not specified explicitly.
        /// In these cases the value of the <see cref="RootNote"/> property will be used. For example,
        /// <see cref="Note(Interval)"/>.
        /// </para>
        /// </remarks>
        public MusicTheory.Note RootNote { get; private set; } = DefaultRootNote;

        #endregion

        #region Methods

        #region Note

        /// <summary>
        /// Adds a note by the specified interval relative to the current root note using
        /// default length and velocity.
        /// </summary>
        /// <param name="interval">The <see cref="Interval"/> which defines
        /// a number of half steps from the current root note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set root note use <see cref="SetRootNote(MusicTheory.Note)"/> method. By default the root note is C4.
        /// To set default note length use <see cref="SetNoteLength(ITimeSpan)"/> method. By default the length
        /// is 1/4. To set default velocity use <see cref="SetVelocity(SevenBitNumber)"/> method. By default the
        /// velocity is 100.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="interval"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The number of result note is out of valid range.</exception>
        public PatternBuilder Note(Interval interval)
        {
            return Note(interval, NoteLength, Velocity);
        }

        /// <summary>
        /// Adds a note by the specified interval relative to the current root note using
        /// specified length and default velocity.
        /// </summary>
        /// <param name="interval">The <see cref="Interval"/> which defines
        /// a number of half steps from the current root note.</param>
        /// <param name="length">The length of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set root note use <see cref="SetRootNote(MusicTheory.Note)"/> method. By default the root note is C4.
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
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">The number of result note is out of valid range.</exception>
        public PatternBuilder Note(Interval interval, ITimeSpan length)
        {
            return Note(interval, length, Velocity);
        }

        /// <summary>
        /// Adds a note by the specified interval relative to the current root note using
        /// default length and specified velocity.
        /// </summary>
        /// <param name="interval">The <see cref="Interval"/> which defines
        /// a number of half steps from the current root note.</param>
        /// <param name="velocity">The velocity of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set root note use <see cref="SetRootNote(MusicTheory.Note)"/> method. By default the root note is C4.
        /// To set default note length use <see cref="SetNoteLength(ITimeSpan)"/> method. By default the length
        /// is 1/4.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="interval"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The number of result note is out of valid range.</exception>
        public PatternBuilder Note(Interval interval, SevenBitNumber velocity)
        {
            return Note(interval, NoteLength, velocity);
        }

        /// <summary>
        /// Adds a note by the specified interval relative to the current root note using
        /// specified length and velocity.
        /// </summary>
        /// <param name="interval">The <see cref="Interval"/> which defines
        /// a number of half steps from the current root note.</param>
        /// <param name="length">The length of a note.</param>
        /// <param name="velocity">The velocity of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set root note use <see cref="SetRootNote(MusicTheory.Note)"/> method. By default the root note is C4.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="interval"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">The number of result note is out of valid range.</exception>
        public PatternBuilder Note(Interval interval, ITimeSpan length, SevenBitNumber velocity)
        {
            ThrowIfArgument.IsNull(nameof(interval), interval);

            return Note(RootNote.Transpose(interval), length, velocity);
        }

        /// <summary>
        /// Adds a note by the specified note name using default velocity, length and octave.
        /// </summary>
        /// <param name="noteName">The name of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default octave use <see cref="SetOctave"/> method. By default the octave number is 4.
        /// To set default note length use <see cref="SetNoteLength(ITimeSpan)"/> method. By default the length
        /// is 1/4. To set default velocity use <see cref="SetVelocity(SevenBitNumber)"/> method. By default the
        /// velocity is 100.
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="noteName"/> specified an invalid value.</exception>
        public PatternBuilder Note(NoteName noteName)
        {
            return Note(noteName, NoteLength, Velocity);
        }

        /// <summary>
        /// Adds a note by the specified note name using specified length and default velocity and octave.
        /// </summary>
        /// <param name="noteName">The name of a note.</param>
        /// <param name="length">The length of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default octave use <see cref="SetOctave"/> method. By default the octave number is 4.
        /// To set default velocity use <see cref="SetVelocity(SevenBitNumber)"/> method. By default the
        /// velocity is 100.
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="noteName"/> specified an invalid value.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="length"/> is <c>null</c>.</exception>
        public PatternBuilder Note(NoteName noteName, ITimeSpan length)
        {
            return Note(noteName, length, Velocity);
        }

        /// <summary>
        /// Adds a note by the specified note name using specified velocity and default length and octave.
        /// </summary>
        /// <param name="noteName">The name of a note.</param>
        /// <param name="velocity">The velocity of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default octave use <see cref="SetOctave"/> method. By default the octave number is 4.
        /// To set default note length use <see cref="SetNoteLength(ITimeSpan)"/> method. By default the length
        /// is 1/4.
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="noteName"/> specified an invalid value.</exception>
        public PatternBuilder Note(NoteName noteName, SevenBitNumber velocity)
        {
            return Note(noteName, NoteLength, velocity);
        }

        /// <summary>
        /// Adds a note by the specified note name using specified velocity and length, and default octave.
        /// </summary>
        /// <param name="noteName">The name of a note.</param>
        /// <param name="length">The length of a note.</param>
        /// <param name="velocity">The velocity of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default octave use <see cref="SetOctave"/> method. By default the octave number is 4.
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="noteName"/> specified an invalid value.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="length"/> is <c>null</c>.</exception>
        public PatternBuilder Note(NoteName noteName, ITimeSpan length, SevenBitNumber velocity)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(noteName), noteName);

            return Note(Octave.GetNote(noteName), length, velocity);
        }

        /// <summary>
        /// Adds a note using default length and velocity.
        /// </summary>
        /// <param name="note">A note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default note length use <see cref="SetNoteLength(ITimeSpan)"/> method. By default the length
        /// is 1/4. To set default velocity use <see cref="SetVelocity(SevenBitNumber)"/> method. By default the
        /// velocity is 100.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="note"/> is <c>null</c>.</exception>
        public PatternBuilder Note(MusicTheory.Note note)
        {
            return Note(note, NoteLength, Velocity);
        }

        /// <summary>
        /// Adds a note using specified length and default velocity.
        /// </summary>
        /// <param name="note">A note.</param>
        /// <param name="length">The length of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default velocity use <see cref="SetVelocity(SevenBitNumber)"/> method. By default the
        /// velocity is 100.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="note"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public PatternBuilder Note(MusicTheory.Note note, ITimeSpan length)
        {
            return Note(note, length, Velocity);
        }

        /// <summary>
        /// Adds a note using specified velocity and default length.
        /// </summary>
        /// <param name="note">A note.</param>
        /// <param name="velocity">The velocity of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default note length use <see cref="SetNoteLength(ITimeSpan)"/> method. By default the length
        /// is 1/4.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="note"/> is <c>null</c>.</exception>
        public PatternBuilder Note(MusicTheory.Note note, SevenBitNumber velocity)
        {
            return Note(note, NoteLength, velocity);
        }

        /// <summary>
        /// Adds a note using specified velocity and length.
        /// </summary>
        /// <param name="note">The note.</param>
        /// <param name="length">The length of the note.</param>
        /// <param name="velocity">The velocity of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="note"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public PatternBuilder Note(MusicTheory.Note note, ITimeSpan length, SevenBitNumber velocity)
        {
            ThrowIfArgument.IsNull(nameof(note), note);
            ThrowIfArgument.IsNull(nameof(length), length);

            return AddAction(new AddNoteAction(new NoteDescriptor(note, velocity, length)));
        }

        #endregion

        #region Chord

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

        #endregion

        #region Pattern

        /// <summary>
        /// Adds a pattern.
        /// </summary>
        /// <param name="pattern">Pattern to add.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is <c>null</c>.</exception>
        public PatternBuilder Pattern(Pattern pattern)
        {
            ThrowIfArgument.IsNull(nameof(pattern), pattern);

            return AddAction(new AddPatternAction(pattern));
        }

        #endregion

        #region Anchor

        /// <summary>
        /// Places the specified anchor at the current time.
        /// </summary>
        /// <param name="anchor">Anchor to place.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="anchor"/> is <c>null</c>.</exception>
        public PatternBuilder Anchor(object anchor)
        {
            ThrowIfArgument.IsNull(nameof(anchor), anchor);

            return AddAction(new AddAnchorAction(anchor));
        }

        /// <summary>
        /// Places an anchor at the current time.
        /// </summary>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        public PatternBuilder Anchor()
        {
            return AddAction(new AddAnchorAction());
        }

        /// <summary>
        /// Moves to the first specified anchor.
        /// </summary>
        /// <param name="anchor">Anchor to move to.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="anchor"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">There are no anchors with the <paramref name="anchor"/> key.</exception>
        public PatternBuilder MoveToFirstAnchor(object anchor)
        {
            ThrowIfArgument.IsNull(nameof(anchor), anchor);

            var counter = GetAnchorCounter(anchor);
            if (counter < 1)
                throw new ArgumentException($"There are no anchors with the '{anchor}' key.", nameof(anchor));

            return AddAction(new MoveToAnchorAction(anchor, AnchorPosition.First));
        }

        /// <summary>
        /// Move to the first anchor.
        /// </summary>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="InvalidOperationException">There are no anchors.</exception>
        public PatternBuilder MoveToFirstAnchor()
        {
            var counter = GetAnchorCounter(null);
            if (counter < 1)
                throw new InvalidOperationException("There are no anchors.");

            return AddAction(new MoveToAnchorAction(AnchorPosition.First));
        }

        /// <summary>
        /// Moves to the last specified anchor.
        /// </summary>
        /// <param name="anchor">Anchor to move to.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="anchor"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">There are no anchors with the <paramref name="anchor"/> key.</exception>
        public PatternBuilder MoveToLastAnchor(object anchor)
        {
            ThrowIfArgument.IsNull(nameof(anchor), anchor);

            var counter = GetAnchorCounter(anchor);
            if (counter < 1)
                throw new ArgumentException($"There are no anchors with the '{anchor}' key.", nameof(anchor));

            return AddAction(new MoveToAnchorAction(anchor, AnchorPosition.Last));
        }

        /// <summary>
        /// Moves to the last anchor.
        /// </summary>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="InvalidOperationException">The are no anchors.</exception>
        public PatternBuilder MoveToLastAnchor()
        {
            var counter = GetAnchorCounter(null);
            if (counter < 1)
                throw new InvalidOperationException("There are no anchors.");

            return AddAction(new MoveToAnchorAction(AnchorPosition.Last));
        }

        /// <summary>
        /// Moves to the nth specified anchor.
        /// </summary>
        /// <param name="anchor">Anchor to move to.</param>
        /// <param name="index">Index of an anchor to move to.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="anchor"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is out of range.</exception>
        public PatternBuilder MoveToNthAnchor(object anchor, int index)
        {
            ThrowIfArgument.IsNull(nameof(anchor), anchor);

            var counter = GetAnchorCounter(anchor);
            ThrowIfArgument.IsOutOfRange(nameof(index),
                                         index,
                                         0,
                                         counter - 1,
                                         "Index is out of range.");

            return AddAction(new MoveToAnchorAction(anchor, AnchorPosition.Nth, index));
        }

        /// <summary>
        /// Moves to the nth anchor.
        /// </summary>
        /// <param name="index">Index of an anchor to move to.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is out of range.</exception>
        public PatternBuilder MoveToNthAnchor(int index)
        {
            var counter = GetAnchorCounter(null);
            ThrowIfArgument.IsOutOfRange(nameof(index),
                                         index,
                                         0,
                                         counter - 1,
                                         "Index is out of range.");

            return AddAction(new MoveToAnchorAction(AnchorPosition.Nth, index));
        }

        #endregion

        #region Move

        /// <summary>
        /// Moves the current time by the specified step forward.
        /// </summary>
        /// <param name="step">Step to move by.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="step"/> is <c>null</c>.</exception>
        public PatternBuilder StepForward(ITimeSpan step)
        {
            ThrowIfArgument.IsNull(nameof(step), step);

            return AddAction(new StepForwardAction(step));
        }

        /// <summary>
        /// Moves the current time by the default step forward.
        /// </summary>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default step use <see cref="SetStep(ITimeSpan)"/> method. By default the step is 1/4.
        /// </remarks>
        public PatternBuilder StepForward()
        {
            return AddAction(new StepForwardAction(Step));
        }

        /// <summary>
        /// Moves the current time by the specified step back.
        /// </summary>
        /// <param name="step">Step to move by.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="step"/> is <c>null</c>.</exception>
        public PatternBuilder StepBack(ITimeSpan step)
        {
            ThrowIfArgument.IsNull(nameof(step), step);

            return AddAction(new StepBackAction(step));
        }

        /// <summary>
        /// Moves the current time by the default step back.
        /// </summary>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default step use <see cref="SetStep(ITimeSpan)"/> method. By default the step is 1/4.
        /// </remarks>
        public PatternBuilder StepBack()
        {
            return AddAction(new StepBackAction(Step));
        }

        /// <summary>
        /// Moves the current time to the specified one.
        /// </summary>
        /// <param name="time">Time to move to.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="time"/> is <c>null</c>.</exception>
        public PatternBuilder MoveToTime(ITimeSpan time)
        {
            ThrowIfArgument.IsNull(nameof(time), time);

            return AddAction(new MoveToTimeAction(time));
        }

        /// <summary>
        /// Moves the current time to the previous one.
        /// </summary>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// On every action current time is stored in the time history. To return to the last saved time
        /// you can call the <see cref="MoveToPreviousTime"/>.
        /// </remarks>
        public PatternBuilder MoveToPreviousTime()
        {
            return AddAction(new MoveToTimeAction());
        }

        #endregion

        #region Repeat

        /// <summary>
        /// Repeats the specified number of previous actions.
        /// </summary>
        /// <param name="actionsCount">Number of previous actions to repeat.</param>
        /// <param name="repetitionsCount">Count of repetitions.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// Note that <see cref="SetNoteLength(ITimeSpan)"/>, <see cref="SetOctave"/>,
        /// <see cref="SetStep(ITimeSpan)"/> and <see cref="SetVelocity(SevenBitNumber)"/> are not
        /// actions and will not be repeated since default values applies immediately on next actions.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="actionsCount"/> is negative.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="actionsCount"/> is greater than count of existing actions.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="repetitionsCount"/> is negative.</description>
        /// </item>
        /// </list>
        /// </exception>
        public PatternBuilder Repeat(int actionsCount, int repetitionsCount)
        {
            ThrowIfArgument.IsNegative(nameof(actionsCount), actionsCount, "Actions count is negative.");
            ThrowIfArgument.IsGreaterThan(nameof(actionsCount),
                                          actionsCount,
                                          _actions.Count,
                                          "Actions count is greater than existing actions count.");
            ThrowIfArgument.IsNegative(nameof(repetitionsCount), repetitionsCount, "Repetitions count is negative.");

            return RepeatActions(actionsCount, repetitionsCount);
        }

        /// <summary>
        /// Repeats the previous action the specified number of times.
        /// </summary>
        /// <param name="repetitionsCount">Count of repetitions.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// Note that <see cref="SetNoteLength(ITimeSpan)"/>, <see cref="SetOctave"/>,
        /// <see cref="SetStep(ITimeSpan)"/> and <see cref="SetVelocity(SevenBitNumber)"/> are not
        /// actions and will not be repeated since default values applies immediately on next actions.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="repetitionsCount"/> is negative.</exception>
        /// <exception cref="InvalidOperationException">There are no actions to repeat.</exception>
        public PatternBuilder Repeat(int repetitionsCount)
        {
            ThrowIfArgument.IsNegative(nameof(repetitionsCount), repetitionsCount, "Repetitions count is negative.");

            if (!_actions.Any())
                throw new InvalidOperationException("There is no action to repeat.");

            return RepeatActions(1, repetitionsCount);
        }

        /// <summary>
        /// Repeats the previous action one time.
        /// </summary>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// Note that <see cref="SetNoteLength(ITimeSpan)"/>, <see cref="SetOctave"/>,
        /// <see cref="SetStep(ITimeSpan)"/> and <see cref="SetVelocity(SevenBitNumber)"/> are not
        /// actions and will not be repeated since default values applies immediately on next actions.
        /// </remarks>
        /// <exception cref="InvalidOperationException">There are no actions to repeat.</exception>
        public PatternBuilder Repeat()
        {
            if (!_actions.Any())
                throw new InvalidOperationException("There is no action to repeat.");

            return RepeatActions(1, 1);
        }

        #endregion

        #region Text

        /// <summary>
        /// Adds lyrics.
        /// </summary>
        /// <param name="text">Text of lyrics.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is <c>null</c>.</exception>
        public PatternBuilder Lyrics(string text)
        {
            ThrowIfArgument.IsNull(nameof(text), text);

            return AddAction(new AddTextEventAction<LyricEvent>(text));
        }

        /// <summary>
        /// Adds a marker.
        /// </summary>
        /// <param name="marker">The text of marker.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="marker"/> is <c>null</c>.</exception>
        public PatternBuilder Marker(string marker)
        {
            ThrowIfArgument.IsNull(nameof(marker), marker);

            return AddAction(new AddTextEventAction<MarkerEvent>(marker));
        }

        #endregion

        #region Program

        /// <summary>
        /// Inserts <see cref="ProgramChangeEvent"/> to specify an instrument that will be used by following notes.
        /// </summary>
        /// <param name="programNumber">The number of a MIDI program.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        public PatternBuilder ProgramChange(SevenBitNumber programNumber)
        {
            return AddAction(new SetProgramNumberAction(programNumber));
        }

        /// <summary>
        /// Inserts <see cref="ProgramChangeEvent"/> to specify an instrument that will be used by following notes.
        /// </summary>
        /// <param name="program">The General MIDI Level 1 program.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="program"/> specified an invalid value.</exception>
        public PatternBuilder ProgramChange(GeneralMidiProgram program)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(program), program);

            return AddAction(new SetGeneralMidiProgramAction(program));
        }

        /// <summary>
        /// Inserts <see cref="ProgramChangeEvent"/> to specify an instrument that will be used by following notes.
        /// </summary>
        /// <param name="program">The General MIDI Level 2 program.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="program"/> specified an invalid value.</exception>
        public PatternBuilder ProgramChange(GeneralMidi2Program program)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(program), program);

            return AddAction(new SetGeneralMidi2ProgramAction(program));
        }

        #endregion

        #region Default

        /// <summary>
        /// Sets a root note that will be used by next actions of the builder using
        /// <see cref="Interval"/> objects.
        /// </summary>
        /// <param name="rootNote">The root note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// Setting a root note is not an action and thus will not be stored in a pattern.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="rootNote"/> is <c>null</c>.</exception>
        public PatternBuilder SetRootNote(MusicTheory.Note rootNote)
        {
            ThrowIfArgument.IsNull(nameof(rootNote), rootNote);

            RootNote = rootNote;
            return this;
        }

        /// <summary>
        /// Sets default velocity that will be used by next actions of the builder.
        /// </summary>
        /// <param name="velocity">New default velocity of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// Setting default velocity is not an action and thus will not be stored in a pattern.
        /// </remarks>
        public PatternBuilder SetVelocity(SevenBitNumber velocity)
        {
            Velocity = velocity;
            return this;
        }

        /// <summary>
        /// Sets default note length that will be used by next actions of the builder.
        /// </summary>
        /// <param name="length">New default note length.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// Setting default note length is not an action and thus will not be stored in a pattern.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="length"/> is <c>null</c>.</exception>
        public PatternBuilder SetNoteLength(ITimeSpan length)
        {
            ThrowIfArgument.IsNull(nameof(length), length);

            NoteLength = length;
            return this;
        }

        /// <summary>
        /// Sets default step for step back and step forward actions of the builder.
        /// </summary>
        /// <param name="step">New default step.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// Setting default step is not an action and thus will not be stored in a pattern.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="step"/> is <c>null</c>.</exception>
        public PatternBuilder SetStep(ITimeSpan step)
        {
            ThrowIfArgument.IsNull(nameof(step), step);

            Step = step;
            return this;
        }

        /// <summary>
        /// Sets default note octave that will be used by next actions of the builder.
        /// </summary>
        /// <param name="octave">New default octave.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// Setting default octave is not an action and thus will not be stored in a pattern.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="octave"/> is <c>null</c>.</exception>
        public PatternBuilder SetOctave(Octave octave)
        {
            ThrowIfArgument.IsNull(nameof(octave), octave);

            Octave = octave;
            return this;
        }

        #endregion

        /// <summary>
        /// Build an instance of the <see cref="Composing.Pattern"/> holding all actions
        /// defined via builder.
        /// </summary>
        /// <returns>An instance of the <see cref="Composing.Pattern"/> that holds all actions
        /// defined by the current <see cref="PatternBuilder"/>.</returns>
        public Pattern Build()
        {
            return new Pattern(_actions.ToList());
        }

        /// <summary>
        /// Replays all actions contained in the specified pattern.
        /// </summary>
        /// <param name="pattern">Pattern to replay actions of.</param>
        /// <remarks>
        /// <para>
        /// <see cref="ReplayPattern(Composing.Pattern)"/> inserts all actions from <paramref name="pattern"/>
        /// that were added by using methods of <see cref="PatternBuilder"/> to produce the specified pattern.
        /// These actions will be added as separate ones unlike <see cref="Pattern(Composing.Pattern)"/> which
        /// adds pattern as one single action.
        /// </para>
        /// </remarks>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is <c>null</c>.</exception>
        public PatternBuilder ReplayPattern(Pattern pattern)
        {
            ThrowIfArgument.IsNull(nameof(pattern), pattern);

            foreach (var action in pattern.Actions)
            {
                AddAction(action);
            }

            return this;
        }

        private PatternBuilder AddAction(PatternAction patternAction)
        {
            var addAnchorAction = patternAction as AddAnchorAction;
            if (addAnchorAction != null)
                UpdateAnchorsCounters(addAnchorAction.Anchor);

            _actions.Add(patternAction);
            return this;
        }

        private int GetAnchorCounter(object anchor)
        {
            if (anchor == null)
                return _globalAnchorsCounter;

            int counter;
            if (!_anchorCounters.TryGetValue(anchor, out counter))
                throw new ArgumentException($"Anchor {anchor} doesn't exist.", nameof(anchor));

            return counter;
        }

        private void UpdateAnchorsCounters(object anchor)
        {
            _globalAnchorsCounter++;

            if (anchor == null)
                return;

            if (!_anchorCounters.ContainsKey(anchor))
                _anchorCounters.Add(anchor, 0);

            _anchorCounters[anchor]++;
        }

        private PatternBuilder RepeatActions(int actionsCount, int repetitionsCount)
        {
            var actionsToRepeat = _actions.Skip(_actions.Count - actionsCount).ToList();
            var newActions = Enumerable.Range(0, repetitionsCount).SelectMany(i => actionsToRepeat);

            foreach (var action in newActions)
            {
                AddAction(action);
            }

            return this;
        }

        #endregion
    }
}
