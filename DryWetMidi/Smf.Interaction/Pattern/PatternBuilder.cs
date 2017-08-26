using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Provides a fluent interface to build an instance of the <see cref="Interaction.Pattern"/>.
    /// </summary>
    public sealed class PatternBuilder
    {
        #region Fields

        private readonly List<IPatternAction> _actions = new List<IPatternAction>();

        private readonly Dictionary<object, int> _anchorCounters = new Dictionary<object, int>();
        private int _globalAnchorsCounter = 0;

        private SevenBitNumber _velocity = Interaction.Note.DefaultVelocity;
        private ILength _noteLength = (MusicalLength)MusicalFraction.Quarter;
        private ILength _step = (MusicalLength)MusicalFraction.Quarter;
        private OctaveDefinition _octave = OctaveDefinition.Middle;
        private NoteDefinition _rootNote = OctaveDefinition.Middle.C;

        #endregion

        #region Methods

        #region Note

        /// <summary>
        /// Adds a note by the specified interval relative to the current root note using
        /// default length and velocity.
        /// </summary>
        /// <param name="intervalDefinition">The <see cref="IntervalDefinition"/> which defines
        /// a number of half steps from the current root note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set root note use <see cref="SetRootNote(NoteDefinition)"/> method. By default the root note is C4.
        /// To set default note length use <see cref="SetNoteLength(ILength)"/> method. By default the length
        /// is 1/4. To set default velocity use <see cref="SetVelocity(SevenBitNumber)"/> method. By default the
        /// velocity is 100.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="intervalDefinition"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The number of result note is out of valid range.</exception>
        public PatternBuilder Note(IntervalDefinition intervalDefinition)
        {
            return Note(intervalDefinition, _noteLength, _velocity);
        }

        /// <summary>
        /// Adds a note by the specified interval relative to the current root note using
        /// specified length and default velocity.
        /// </summary>
        /// <param name="intervalDefinition">The <see cref="IntervalDefinition"/> which defines
        /// a number of half steps from the current root note.</param>
        /// <param name="length">The length of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set root note use <see cref="SetRootNote(NoteDefinition)"/> method. By default the root note is C4.
        /// To set default velocity use <see cref="SetVelocity(SevenBitNumber)"/> method. By default the
        /// velocity is 100.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="intervalDefinition"/> is null. -or-
        /// <paramref name="length"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The number of result note is out of valid range.</exception>
        public PatternBuilder Note(IntervalDefinition intervalDefinition, ILength length)
        {
            return Note(intervalDefinition, length, _velocity);
        }

        /// <summary>
        /// Adds a note by the specified interval relative to the current root note using
        /// default length and specified velocity.
        /// </summary>
        /// <param name="intervalDefinition">The <see cref="IntervalDefinition"/> which defines
        /// a number of half steps from the current root note.</param>
        /// <param name="velocity">The velocity of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set root note use <see cref="SetRootNote(NoteDefinition)"/> method. By default the root note is C4.
        /// To set default note length use <see cref="SetNoteLength(ILength)"/> method. By default the length
        /// is 1/4.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="intervalDefinition"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The number of result note is out of valid range.</exception>
        public PatternBuilder Note(IntervalDefinition intervalDefinition, SevenBitNumber velocity)
        {
            return Note(intervalDefinition, _noteLength, velocity);
        }

        /// <summary>
        /// Adds a note by the specified interval relative to the current root note using
        /// specified length and velocity.
        /// </summary>
        /// <param name="intervalDefinition">The <see cref="IntervalDefinition"/> which defines
        /// a number of half steps from the current root note.</param>
        /// <param name="length">The length of a note.</param>
        /// <param name="velocity">The velocity of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set root note use <see cref="SetRootNote(NoteDefinition)"/> method. By default the root note is C4.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="intervalDefinition"/> is null. -or-
        /// <paramref name="length"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The number of result note is out of valid range.</exception>
        public PatternBuilder Note(IntervalDefinition intervalDefinition, ILength length, SevenBitNumber velocity)
        {
            ThrowIfArgument.IsNull(nameof(intervalDefinition), intervalDefinition);

            return Note(_rootNote.Transpose(intervalDefinition), length, velocity);
        }

        /// <summary>
        /// Adds a note by the specified note name using default velocity, length and octave.
        /// </summary>
        /// <param name="noteName">The name of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default octave use <see cref="SetOctave(int)"/> method. By default the octave number is 4.
        /// To set default note length use <see cref="SetNoteLength(ILength)"/> method. By default the length
        /// is 1/4. To set default velocity use <see cref="SetVelocity(SevenBitNumber)"/> method. By default the
        /// velocity is 100.
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="noteName"/> specified an invalid value.</exception>
        public PatternBuilder Note(NoteName noteName)
        {
            return Note(noteName, _noteLength, _velocity);
        }

        /// <summary>
        /// Adds a note by the specified note name using specified length and default velocity and octave.
        /// </summary>
        /// <param name="noteName">The name of a note.</param>
        /// <param name="length">The length of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default octave use <see cref="SetOctave(int)"/> method. By default the octave number is 4.
        /// To set default velocity use <see cref="SetVelocity(SevenBitNumber)"/> method. By default the
        /// velocity is 100.
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="noteName"/> specified an invalid value.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="length"/> is null.</exception>
        public PatternBuilder Note(NoteName noteName, ILength length)
        {
            return Note(noteName, length, _velocity);
        }

        /// <summary>
        /// Adds a note by the specified note name using specified velocity and default length and octave.
        /// </summary>
        /// <param name="noteName">The name of a note.</param>
        /// <param name="velocity">The velocity of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default octave use <see cref="SetOctave(int)"/> method. By default the octave number is 4.
        /// To set default note length use <see cref="SetNoteLength(ILength)"/> method. By default the length
        /// is 1/4.
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="noteName"/> specified an invalid value.</exception>
        public PatternBuilder Note(NoteName noteName, SevenBitNumber velocity)
        {
            return Note(noteName, _noteLength, velocity);
        }

        /// <summary>
        /// Adds a note by the specified note name using specified velocity and length, and default octave.
        /// </summary>
        /// <param name="noteName">The name of a note.</param>
        /// <param name="length">The length of a note.</param>
        /// <param name="velocity">The velocity of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default octave use <see cref="SetOctave(int)"/> method. By default the octave number is 4.
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="noteName"/> specified an invalid value.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="length"/> is null.</exception>
        public PatternBuilder Note(NoteName noteName, ILength length, SevenBitNumber velocity)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(noteName), noteName);

            return Note(_octave.GetNoteDefinition(noteName), length, velocity);
        }

        /// <summary>
        /// Adds a note by the specified definition using default length and velocity.
        /// </summary>
        /// <param name="noteDefinition">The definition of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default note length use <see cref="SetNoteLength(ILength)"/> method. By default the length
        /// is 1/4. To set default velocity use <see cref="SetVelocity(SevenBitNumber)"/> method. By default the
        /// velocity is 100.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="noteDefinition"/> is null.</exception>
        public PatternBuilder Note(NoteDefinition noteDefinition)
        {
            return Note(noteDefinition, _noteLength, _velocity);
        }

        /// <summary>
        /// Adds a note by the specified definition using specified length and default velocity.
        /// </summary>
        /// <param name="noteDefinition">The definition of a note.</param>
        /// <param name="length">The length of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default velocity use <see cref="SetVelocity(SevenBitNumber)"/> method. By default the
        /// velocity is 100.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="noteDefinition"/> is null. -or-
        /// <paramref name="length"/> is null.</exception>
        public PatternBuilder Note(NoteDefinition noteDefinition, ILength length)
        {
            return Note(noteDefinition, length, _velocity);
        }

        /// <summary>
        /// Adds a note by the specified definition using specified velocity and default length.
        /// </summary>
        /// <param name="noteDefinition">The definition of a note.</param>
        /// <param name="velocity">The velocity of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default note length use <see cref="SetNoteLength(ILength)"/> method. By default the length
        /// is 1/4.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="noteDefinition"/> is null.</exception>
        public PatternBuilder Note(NoteDefinition noteDefinition, SevenBitNumber velocity)
        {
            return Note(noteDefinition, _noteLength, velocity);
        }

        /// <summary>
        /// Adds a note by the specified definition using specified velocity and length.
        /// </summary>
        /// <param name="noteDefinition">The definition of a note.</param>
        /// <param name="length">The length of a note.</param>
        /// <param name="velocity">The velocity of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="noteDefinition"/> is null. -or-
        /// <paramref name="length"/> is null.</exception>
        public PatternBuilder Note(NoteDefinition noteDefinition, ILength length, SevenBitNumber velocity)
        {
            ThrowIfArgument.IsNull(nameof(noteDefinition), noteDefinition);
            ThrowIfArgument.IsNull(nameof(length), length);

            return AddAction(new AddNoteAction(noteDefinition, velocity, length));
        }

        #endregion

        #region Chord

        public PatternBuilder Chord(IEnumerable<IntervalDefinition> intervalDefinitions, NoteName rootNoteName)
        {
            return Chord(intervalDefinitions, rootNoteName, _noteLength, _velocity);
        }

        public PatternBuilder Chord(IEnumerable<IntervalDefinition> intervalDefinitions,
                                    NoteName rootNoteName,
                                    ILength length)
        {
            return Chord(intervalDefinitions, rootNoteName, length, _velocity);
        }

        public PatternBuilder Chord(IEnumerable<IntervalDefinition> intervalDefinitions,
                                    NoteName rootNoteName,
                                    SevenBitNumber velocity)
        {
            return Chord(intervalDefinitions, rootNoteName, _noteLength, velocity);
        }

        public PatternBuilder Chord(IEnumerable<IntervalDefinition> intervalDefinitions,
                                    NoteName rootNoteName,
                                    ILength length,
                                    SevenBitNumber velocity)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(rootNoteName), rootNoteName);

            return Chord(intervalDefinitions, _octave.GetNoteDefinition(rootNoteName), length, velocity);
        }

        /// <summary>
        /// Adds a chord by the specified intervals relative to the root note using default
        /// length and velocity.
        /// </summary>
        /// <param name="intervalDefinitions">The <see cref="IntervalDefinition"/> objects which define
        /// a numbers of half steps from the <paramref name="rootNoteDefinition"/>.</param>
        /// <param name="rootNoteDefinition">The definition of the chord's root note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// The result chord will contain the specified root note and notes produced by transposing
        /// the <paramref name="rootNoteDefinition"/> by the <paramref name="intervalDefinitions"/>.
        /// To set default note length use <see cref="SetNoteLength(ILength)"/> method. By default the length
        /// is 1/4. To set default velocity use <see cref="SetVelocity(SevenBitNumber)"/> method. By default the
        /// velocity is 100.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="intervalDefinitions"/> is null. -or-
        /// <paramref name="rootNoteDefinition"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The number of result chord's note is out of valid range.</exception>
        public PatternBuilder Chord(IEnumerable<IntervalDefinition> intervalDefinitions, NoteDefinition rootNoteDefinition)
        {
            return Chord(intervalDefinitions, rootNoteDefinition, _noteLength, _velocity);
        }

        /// <summary>
        /// Adds a chord by the specified intervals relative to the root note using specified
        /// length and default velocity.
        /// </summary>
        /// <param name="intervalDefinitions">The <see cref="IntervalDefinition"/> objects which define
        /// a numbers of half steps from the <paramref name="rootNoteDefinition"/>.</param>
        /// <param name="rootNoteDefinition">The definition of the chord's root note.</param>
        /// <param name="length">The length of a chord.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// The result chord will contain the specified root note and notes produced by transposing
        /// the <paramref name="rootNoteDefinition"/> by the <paramref name="intervalDefinitions"/>.
        /// To set default velocity use <see cref="SetVelocity(SevenBitNumber)"/> method. By default the
        /// velocity is 100.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="intervalDefinitions"/> is null. -or-
        /// <paramref name="rootNoteDefinition"/> is null. -or- <paramref name="length"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The number of result chord's note is out of valid range.</exception>
        public PatternBuilder Chord(IEnumerable<IntervalDefinition> intervalDefinitions,
                                    NoteDefinition rootNoteDefinition,
                                    ILength length)
        {
            return Chord(intervalDefinitions, rootNoteDefinition, length, _velocity);
        }

        /// <summary>
        /// Adds a chord by the specified intervals relative to the root note using default
        /// length and specified velocity.
        /// </summary>
        /// <param name="intervalDefinitions">The <see cref="IntervalDefinition"/> objects which define
        /// a numbers of half steps from the <paramref name="rootNoteDefinition"/>.</param>
        /// <param name="rootNoteDefinition">The definition of the chord's root note.</param>
        /// <param name="velocity">The velocity of a chord.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// The result chord will contain the specified root note and notes produced by transposing
        /// the <paramref name="rootNoteDefinition"/> by the <paramref name="intervalDefinitions"/>.
        /// To set default note length use <see cref="SetNoteLength(ILength)"/> method. By default the length
        /// is 1/4.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="intervalDefinitions"/> is null. -or-
        /// <paramref name="rootNoteDefinition"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The number of result chord's note is out of valid range.</exception>
        public PatternBuilder Chord(IEnumerable<IntervalDefinition> intervalDefinitions,
                                    NoteDefinition rootNoteDefinition,
                                    SevenBitNumber velocity)
        {
            return Chord(intervalDefinitions, rootNoteDefinition, _noteLength, velocity);
        }

        /// <summary>
        /// Adds a chord by the specified intervals relative to the root note using specified
        /// length and velocity.
        /// </summary>
        /// <param name="intervalDefinitions">The <see cref="IntervalDefinition"/> objects which define
        /// a numbers of half steps from the <paramref name="rootNoteDefinition"/>.</param>
        /// <param name="rootNoteDefinition">The definition of the chord's root note.</param>
        /// <param name="length">The length of a chord.</param>
        /// <param name="velocity">The velocity of a chord.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// The result chord will contain the specified root note and notes produced by transposing
        /// the <paramref name="rootNoteDefinition"/> by the <paramref name="intervalDefinitions"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="intervalDefinitions"/> is null. -or-
        /// <paramref name="rootNoteDefinition"/> is null. -or- <paramref name="length"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The number of result chord's note is out of valid range.</exception>
        public PatternBuilder Chord(IEnumerable<IntervalDefinition> intervalDefinitions,
                                    NoteDefinition rootNoteDefinition,
                                    ILength length,
                                    SevenBitNumber velocity)
        {
            ThrowIfArgument.IsNull(nameof(intervalDefinitions), intervalDefinitions);
            ThrowIfArgument.IsNull(nameof(rootNoteDefinition), rootNoteDefinition);

            return Chord(new[] { rootNoteDefinition }.Concat(intervalDefinitions.Where(i => i != null)
                                                                                .Select(i => rootNoteDefinition.Transpose(i))),
                         length,
                         velocity);
        }

        /// <summary>
        /// Adds a chord by the specified notes names using default velocity, length and octave.
        /// </summary>
        /// <param name="noteNames">Names of notes that represent a chord.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default octave use <see cref="SetOctave(int)"/> method. By default the octave number is 4.
        /// To set default note length use <see cref="SetNoteLength(ILength)"/> method. By default the length
        /// is 1/4. To set default velocity use <see cref="SetVelocity(SevenBitNumber)"/> method. By default the
        /// velocity is 100.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="noteNames"/> is null.</exception>
        public PatternBuilder Chord(IEnumerable<NoteName> noteNames)
        {
            return Chord(noteNames, _noteLength, _velocity);
        }

        /// <summary>
        /// Adds a chord by the specified notes names using specified length and default velocity, and default octave.
        /// </summary>
        /// <param name="noteNames">Names of notes that represent a chord.</param>
        /// <param name="length">The length of a chord.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default octave use <see cref="SetOctave(int)"/> method. By default the octave number is 4.
        /// To set default velocity use <see cref="SetVelocity(SevenBitNumber)"/> method. By default the
        /// velocity is 100.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="noteNames"/> is null. -or-
        /// <paramref name="length"/> is null.</exception>
        public PatternBuilder Chord(IEnumerable<NoteName> noteNames, ILength length)
        {
            return Chord(noteNames, length, _velocity);
        }

        /// <summary>
        /// Adds a chord by the specified notes names using specified velocity and default length, and default octave.
        /// </summary>
        /// <param name="noteNames">Names of notes that represent a chord.</param>
        /// <param name="velocity">The velocity of a chord.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default octave use <see cref="SetOctave(int)"/> method. By default the octave number is 4.
        /// To set default note length use <see cref="SetNoteLength(ILength)"/> method. By default the length
        /// is 1/4.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="noteNames"/> is null.</exception>
        public PatternBuilder Chord(IEnumerable<NoteName> noteNames, SevenBitNumber velocity)
        {
            return Chord(noteNames, _noteLength, velocity);
        }

        /// <summary>
        /// Adds a chord by the specified notes names using specified velocity and length, and default octave.
        /// </summary>
        /// <param name="noteNames">Names of notes that represent a chord.</param>
        /// <param name="length">The length of a chord.</param>
        /// <param name="velocity">The velocity of a chord.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default octave use <see cref="SetOctave(int)"/> method. By default the octave number is 4.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="noteNames"/> is null. -or-
        /// <paramref name="length"/> is null.</exception>
        public PatternBuilder Chord(IEnumerable<NoteName> noteNames, ILength length, SevenBitNumber velocity)
        {
            ThrowIfArgument.IsNull(nameof(noteNames), noteNames);
            ThrowIfArgument.IsNull(nameof(length), length);

            return Chord(noteNames.Select(n => _octave.GetNoteDefinition(n)), length, velocity);
        }

        /// <summary>
        /// Adds a chord by the specified notes definitions using default velocity and length.
        /// </summary>
        /// <param name="noteDefinitions">Definitions of notes that represent a chord.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default note length use <see cref="SetNoteLength(ILength)"/> method. By default the length
        /// is 1/4. To set default velocity use <see cref="SetVelocity(SevenBitNumber)"/> method. By default the
        /// velocity is 100.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="noteDefinitions"/> is null.</exception>
        public PatternBuilder Chord(IEnumerable<NoteDefinition> noteDefinitions)
        {
            return Chord(noteDefinitions, _noteLength, _velocity);
        }

        /// <summary>
        /// Adds a chord by the specified notes definitions using specified length and default velocity.
        /// </summary>
        /// <param name="noteDefinitions">Definitions of notes that represent a chord.</param>
        /// <param name="length">The length of a chord.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default velocity use <see cref="SetVelocity(SevenBitNumber)"/> method. By default the
        /// velocity is 100.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="noteDefinitions"/> is null. -or-
        /// <paramref name="length"/> is null.</exception>
        public PatternBuilder Chord(IEnumerable<NoteDefinition> noteDefinitions, ILength length)
        {
            return Chord(noteDefinitions, length, _velocity);
        }

        /// <summary>
        /// Adds a chord by the specified notes definitions using specified velocity and default length.
        /// </summary>
        /// <param name="noteDefinitions">Definitions of notes that represent a chord.</param>
        /// <param name="velocity">The velocity of a chord.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default note length use <see cref="SetNoteLength(ILength)"/> method. By default the length
        /// is 1/4.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="noteDefinitions"/> is null.</exception>
        public PatternBuilder Chord(IEnumerable<NoteDefinition> noteDefinitions, SevenBitNumber velocity)
        {
            return Chord(noteDefinitions, _noteLength, velocity);
        }

        /// <summary>
        /// Adds a chord by the specified notes definitions using specified velocity and length.
        /// </summary>
        /// <param name="noteDefinitions">Definitions of notes that represent a chord.</param>
        /// <param name="length">The length of a chord.</param>
        /// <param name="velocity">The velocity of a chord.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="noteDefinitions"/> is null. -or-
        /// <paramref name="length"/> is null.</exception>
        public PatternBuilder Chord(IEnumerable<NoteDefinition> noteDefinitions, ILength length, SevenBitNumber velocity)
        {
            ThrowIfArgument.IsNull(nameof(noteDefinitions), noteDefinitions);
            ThrowIfArgument.IsNull(nameof(length), length);

            return AddAction(new AddChordAction(noteDefinitions, velocity, length));
        }

        #endregion

        #region Pattern

        /// <summary>
        /// Adds a pattern.
        /// </summary>
        /// <param name="pattern">Pattern to add.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is null.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="anchor"/> is null.</exception>
        public PatternBuilder Anchor(object anchor)
        {
            ThrowIfArgument.IsNull(nameof(anchor), anchor);

            UpdateAnchorsCounters(anchor);

            return AddAction(new AddAnchorAction(anchor));
        }

        /// <summary>
        /// Places an anchor at the current time.
        /// </summary>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        public PatternBuilder Anchor()
        {
            UpdateAnchorsCounters(null);

            return AddAction(new AddAnchorAction());
        }

        /// <summary>
        /// Moves to the first specified anchor.
        /// </summary>
        /// <param name="anchor">Anchor to move to.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="anchor"/> is null.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="anchor"/> is null.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="anchor"/> is null.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="step"/> is null.</exception>
        public PatternBuilder StepForward(ILength step)
        {
            ThrowIfArgument.IsNull(nameof(step), step);

            return AddAction(new StepForwardAction(step));
        }

        /// <summary>
        /// Moves the current time by the default step forward.
        /// </summary>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default step use <see cref="SetStep(ILength)"/> method. By default the step is 1/4.
        /// </remarks>
        public PatternBuilder StepForward()
        {
            return AddAction(new StepForwardAction(_step));
        }

        /// <summary>
        /// Moves the current time by the specified step back.
        /// </summary>
        /// <param name="step">Step to move by.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="step"/> is null.</exception>
        public PatternBuilder StepBack(ILength step)
        {
            ThrowIfArgument.IsNull(nameof(step), step);

            return AddAction(new StepBackAction(step));
        }

        /// <summary>
        /// Moves the current time by the default step back.
        /// </summary>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default step use <see cref="SetStep(ILength)"/> method. By default the step is 1/4.
        /// </remarks>
        public PatternBuilder StepBack()
        {
            return AddAction(new StepBackAction(_step));
        }

        /// <summary>
        /// Moves the current time to the specified one.
        /// </summary>
        /// <param name="time">Time to move to.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="time"/> is null.</exception>
        public PatternBuilder MoveToTime(ITime time)
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
        /// Note that <see cref="SetNoteLength(ILength)"/>, <see cref="SetOctave(int)"/>,
        /// <see cref="SetStep(ILength)"/> and <see cref="SetVelocity(SevenBitNumber)"/> are not
        /// actions and will not be repeated since default values applies immidiately on next actions.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="actionsCount"/> is
        /// negative. -or- <paramref name="actionsCount"/> is greater than count of existing actions. -or-
        /// <paramref name="repetitionsCount"/> is negative.</exception>
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
        /// Note that <see cref="SetNoteLength(ILength)"/>, <see cref="SetOctave(int)"/>,
        /// <see cref="SetStep(ILength)"/> and <see cref="SetVelocity(SevenBitNumber)"/> are not
        /// actions and will not be repeated since default values applies immidiately on next actions.
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
        /// Note that <see cref="SetNoteLength(ILength)"/>, <see cref="SetOctave(int)"/>,
        /// <see cref="SetStep(ILength)"/> and <see cref="SetVelocity(SevenBitNumber)"/> are not
        /// actions and will not be repeated since default values applies immidiately on next actions.
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

        public PatternBuilder Lyrics(string text)
        {
            ThrowIfArgument.IsNull(nameof(text), text);

            return AddAction(new AddTextEventAction<LyricEvent>(text));
        }

        public PatternBuilder Marker(string marker)
        {
            ThrowIfArgument.IsNull(nameof(marker), marker);

            return AddAction(new AddTextEventAction<MarkerEvent>(marker));
        }

        #endregion

        #region Default

        /// <summary>
        /// Sets a root note that will be used by next actions of the builder using
        /// <see cref="IntervalDefinition"/> objects.
        /// </summary>
        /// <param name="rootNoteDefinition">The definition of the root note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// Setting a root note is not an action and thus will not be stored in a pattern.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="rootNoteDefinition"/> is null.</exception>
        public PatternBuilder SetRootNote(NoteDefinition rootNoteDefinition)
        {
            ThrowIfArgument.IsNull(nameof(rootNoteDefinition), rootNoteDefinition);

            _rootNote = rootNoteDefinition;
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
            _velocity = velocity;
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
        /// <exception cref="ArgumentNullException"><paramref name="length"/> is null.</exception>
        public PatternBuilder SetNoteLength(ILength length)
        {
            ThrowIfArgument.IsNull(nameof(length), length);

            _noteLength = length;
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
        /// <exception cref="ArgumentNullException"><paramref name="step"/> is null.</exception>
        public PatternBuilder SetStep(ILength step)
        {
            ThrowIfArgument.IsNull(nameof(step), step);

            _step = step;
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
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="octave"/> is out of valid range.</exception>
        public PatternBuilder SetOctave(int octave)
        {
            _octave = OctaveDefinition.Get(octave);
            return this;
        }

        #endregion

        /// <summary>
        /// Build an instance of the <see cref="Interaction.Pattern"/> holding all actions
        /// defined via builder.
        /// </summary>
        /// <returns>An instance of the <see cref="Interaction.Pattern"/> that holds all actions
        /// defined by the current <see cref="PatternBuilder"/>.</returns>
        public Pattern Build()
        {
            return new Pattern(_actions.ToList());
        }

        private PatternBuilder AddAction(IPatternAction patternEvent)
        {
            _actions.Add(patternEvent);
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
                var addAnchorAction = action as AddAnchorAction;
                if (addAnchorAction != null)
                    UpdateAnchorsCounters(addAnchorAction.Anchor);

                _actions.Add(action);
            }

            return this;
        }

        #endregion
    }
}
