using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Interaction;

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
    ///     .Note(Interval.Seven) // +7  (D#4)
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
    public sealed partial class PatternBuilder
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
        /// <see cref="Note(MusicTheory.Note, ITimeSpan, SevenBitNumber?)"/> or
        /// <see cref="Chord(IEnumerable{Interval}, MusicTheory.Note, ITimeSpan, SevenBitNumber?)"/>.
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
        /// <see cref="Note(MusicTheory.Note, ITimeSpan, SevenBitNumber?)"/> or
        /// <see cref="Chord(IEnumerable{Interval}, MusicTheory.Note, ITimeSpan, SevenBitNumber?)"/>.
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
        /// <see cref="Note(NoteName, ITimeSpan, SevenBitNumber?)"/> or
        /// <see cref="Chord(IEnumerable{Interval}, NoteName, ITimeSpan, SevenBitNumber?)"/>.
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
        /// <see cref="Note(Interval, ITimeSpan, SevenBitNumber?)"/>.
        /// </para>
        /// </remarks>
        public MusicTheory.Note RootNote { get; private set; } = DefaultRootNote;

        #endregion

        #region Methods

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

        #endregion
    }
}
