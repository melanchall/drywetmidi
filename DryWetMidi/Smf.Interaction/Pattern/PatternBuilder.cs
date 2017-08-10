using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class PatternBuilder
    {
        #region Fields

        private readonly List<IPatternAction> _actions = new List<IPatternAction>();

        private readonly Dictionary<object, int> _anchorCounters = new Dictionary<object, int>();
        private int _globalAnchorsCounter = 0;

        private SevenBitNumber _defaultVelocity = Interaction.Note.DefaultVelocity;
        private ILength _defaultNoteLength = (MusicalLength)MusicalFraction.Quarter;
        private ILength _defaultStep = (MusicalLength)MusicalFraction.Quarter;
        private OctaveDefinition _defaultOctave = OctaveDefinition.Get(4);

        #endregion

        #region Methods

        #region Note

        public PatternBuilder Note(NoteName noteName)
        {
            return Note(noteName, _defaultVelocity, _defaultNoteLength);
        }

        public PatternBuilder Note(NoteName noteName, ILength length)
        {
            return Note(noteName, _defaultVelocity, length);
        }

        public PatternBuilder Note(NoteName noteName, SevenBitNumber velocity)
        {
            return Note(noteName, velocity, _defaultNoteLength);
        }

        public PatternBuilder Note(NoteName noteName, SevenBitNumber velocity, ILength length)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(noteName), noteName);
            ThrowIfArgument.IsNull(nameof(length), length);

            return Note(_defaultOctave.GetNoteDefinition(noteName), velocity, length);
        }

        public PatternBuilder Note(NoteDefinition noteDefinition)
        {
            return Note(noteDefinition, _defaultVelocity, _defaultNoteLength);
        }

        public PatternBuilder Note(NoteDefinition noteDefinition, ILength length)
        {
            return Note(noteDefinition, _defaultVelocity, length);
        }

        public PatternBuilder Note(NoteDefinition noteDefinition, SevenBitNumber velocity)
        {
            return Note(noteDefinition, velocity, _defaultNoteLength);
        }

        public PatternBuilder Note(NoteDefinition noteDefinition, SevenBitNumber velocity, ILength length)
        {
            ThrowIfArgument.IsNull(nameof(noteDefinition), noteDefinition);
            ThrowIfArgument.IsNull(nameof(length), length);

            return AddAction(new AddNoteAction(noteDefinition, velocity, length));
        }

        #endregion

        #region Chord

        public PatternBuilder Chord(IEnumerable<NoteName> noteNames)
        {
            return Chord(noteNames, _defaultVelocity, _defaultNoteLength);
        }

        public PatternBuilder Chord(IEnumerable<NoteName> noteNames, ILength length)
        {
            return Chord(noteNames, _defaultVelocity, length);
        }

        public PatternBuilder Chord(IEnumerable<NoteName> noteNames, SevenBitNumber velocity)
        {
            return Chord(noteNames, velocity, _defaultNoteLength);
        }

        public PatternBuilder Chord(IEnumerable<NoteName> noteNames, SevenBitNumber velocity, ILength length)
        {
            ThrowIfArgument.IsNull(nameof(noteNames), noteNames);
            ThrowIfArgument.IsNull(nameof(length), length);

            return Chord(noteNames.Select(n => _defaultOctave.GetNoteDefinition(n)), velocity, length);
        }

        public PatternBuilder Chord(IEnumerable<NoteDefinition> noteDefinitions)
        {
            return Chord(noteDefinitions, _defaultVelocity, _defaultNoteLength);
        }

        public PatternBuilder Chord(IEnumerable<NoteDefinition> noteDefinitions, ILength length)
        {
            return Chord(noteDefinitions, _defaultVelocity, length);
        }

        public PatternBuilder Chord(IEnumerable<NoteDefinition> noteDefinitions, SevenBitNumber velocity)
        {
            return Chord(noteDefinitions, velocity, _defaultNoteLength);
        }

        public PatternBuilder Chord(IEnumerable<NoteDefinition> noteDefinitions, SevenBitNumber velocity, ILength length)
        {
            ThrowIfArgument.IsNull(nameof(noteDefinitions), noteDefinitions);
            ThrowIfArgument.IsNull(nameof(length), length);

            return AddAction(new AddChordAction(noteDefinitions, velocity, length));
        }

        #endregion

        #region Pattern

        public PatternBuilder Pattern(Pattern pattern)
        {
            ThrowIfArgument.IsNull(nameof(pattern), pattern);

            return AddAction(new AddPatternAction(pattern));
        }

        #endregion

        #region Anchor

        public PatternBuilder Anchor(object anchor)
        {
            ThrowIfArgument.IsNull(nameof(anchor), anchor);

            UpdateAnchorsCounters(anchor);

            return AddAction(new AddAnchorAction(anchor));
        }

        public PatternBuilder Anchor()
        {
            UpdateAnchorsCounters(null);

            return AddAction(new AddAnchorAction());
        }

        public PatternBuilder MoveToFirstAnchor(object anchor)
        {
            ThrowIfArgument.IsNull(nameof(anchor), anchor);

            var counter = GetAnchorCounter(anchor);
            if (counter < 1)
                throw new ArgumentException($"Count of the '{anchor}' is less than 1.", nameof(anchor));

            return AddAction(new MoveToAnchorAction(anchor, AnchorPosition.First));
        }

        public PatternBuilder MoveToFirstAnchor()
        {
            var counter = GetAnchorCounter(null);
            if (counter < 1)
                throw new InvalidOperationException($"Count of anchors is less than 1.");

            return AddAction(new MoveToAnchorAction(AnchorPosition.First));
        }

        public PatternBuilder MoveToLastAnchor(object anchor)
        {
            ThrowIfArgument.IsNull(nameof(anchor), anchor);

            var counter = GetAnchorCounter(anchor);
            if (counter < 1)
                throw new ArgumentException($"Count of the '{anchor}' is less than 1.", nameof(anchor));

            return AddAction(new MoveToAnchorAction(anchor, AnchorPosition.Last));
        }

        public PatternBuilder MoveToLastAnchor()
        {
            var counter = GetAnchorCounter(null);
            if (counter < 1)
                throw new InvalidOperationException($"Count of anchors is less than 1.");

            return AddAction(new MoveToAnchorAction(AnchorPosition.Last));
        }

        public PatternBuilder MoveToNthAnchor(object anchor, int index)
        {
            ThrowIfArgument.IsNull(nameof(anchor), anchor);

            var counter = GetAnchorCounter(anchor);
            ThrowIfArgument.IsOutOfRange(nameof(index),
                                         index,
                                         0,
                                         counter - 1,
                                         "Index is out of range for anchor's times count.");

            return AddAction(new MoveToAnchorAction(anchor, AnchorPosition.Nth, index));
        }

        public PatternBuilder MoveToNthAnchor(int index)
        {
            var counter = GetAnchorCounter(null);
            ThrowIfArgument.IsOutOfRange(nameof(index),
                                         index,
                                         0,
                                         counter - 1,
                                         "Index is out of range for anchors count.");

            return AddAction(new MoveToAnchorAction(AnchorPosition.Nth, index));
        }

        #endregion

        #region Move

        public PatternBuilder StepForward(ILength step)
        {
            ThrowIfArgument.IsNull(nameof(step), step);

            return AddAction(new StepForwardAction(step));
        }

        public PatternBuilder StepForward()
        {
            return AddAction(new StepForwardAction(_defaultStep));
        }

        public PatternBuilder StepBack(ILength step)
        {
            ThrowIfArgument.IsNull(nameof(step), step);

            return AddAction(new StepBackAction(step));
        }

        public PatternBuilder StepBack()
        {
            return AddAction(new StepBackAction(_defaultStep));
        }

        public PatternBuilder MoveToTime(ITime time)
        {
            ThrowIfArgument.IsNull(nameof(time), time);

            return AddAction(new MoveToTimeAction(time));
        }

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
        /// Note that <see cref="DefaultNoteLength(ILength)"/>, <see cref="DefaultOctave(int)"/>,
        /// <see cref="DefaultStep(ILength)"/> and <see cref="DefaultVelocity(SevenBitNumber)"/> are not
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
        /// Note that <see cref="DefaultNoteLength(ILength)"/>, <see cref="DefaultOctave(int)"/>,
        /// <see cref="DefaultStep(ILength)"/> and <see cref="DefaultVelocity(SevenBitNumber)"/> are not
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
        /// Note that <see cref="DefaultNoteLength(ILength)"/>, <see cref="DefaultOctave(int)"/>,
        /// <see cref="DefaultStep(ILength)"/> and <see cref="DefaultVelocity(SevenBitNumber)"/> are not
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

        #region Default

        public PatternBuilder DefaultVelocity(SevenBitNumber velocity)
        {
            _defaultVelocity = velocity;
            return this;
        }

        public PatternBuilder DefaultNoteLength(ILength length)
        {
            ThrowIfArgument.IsNull(nameof(length), length);

            _defaultNoteLength = length;
            return this;
        }

        public PatternBuilder DefaultStep(ILength step)
        {
            ThrowIfArgument.IsNull(nameof(step), step);

            _defaultStep = step;
            return this;
        }

        public PatternBuilder DefaultOctave(int octave)
        {
            _defaultOctave = OctaveDefinition.Get(octave);
            return this;
        }

        #endregion

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
