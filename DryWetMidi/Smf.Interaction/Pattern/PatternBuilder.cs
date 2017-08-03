using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class PatternBuilder
    {
        #region Fields

        private readonly List<IPatternAction> _actions = new List<IPatternAction>();
        private readonly Dictionary<object, int> _anchorCounters = new Dictionary<object, int>();

        #endregion

        #region Methods

        public PatternBuilder Note(NoteDefinition noteDefinition, ILength length)
        {
            ThrowIfArgument.IsNull(nameof(noteDefinition), noteDefinition);
            ThrowIfArgument.IsNull(nameof(length), length);

            return AddAction(new AddNoteAction(noteDefinition, length));
        }

        public PatternBuilder Pattern(Pattern pattern)
        {
            ThrowIfArgument.IsNull(nameof(pattern), pattern);

            return AddAction(new AddPatternAction(pattern));
        }

        public PatternBuilder Anchor(object anchor)
        {
            ThrowIfArgument.IsNull(nameof(anchor), anchor);

            if (!_anchorCounters.ContainsKey(anchor))
                _anchorCounters.Add(anchor, 0);

            _anchorCounters[anchor]++;

            return AddAction(new AddAnchorAction(anchor));
        }

        public PatternBuilder StepForward(ILength step)
        {
            ThrowIfArgument.IsNull(nameof(step), step);

            return AddAction(new StepForwardAction(step));
        }

        public PatternBuilder StepBack(ILength step)
        {
            ThrowIfArgument.IsNull(nameof(step), step);

            return AddAction(new StepBackAction(step));
        }

        public PatternBuilder StepBack()
        {
            return AddAction(new StepBackAction());
        }

        public PatternBuilder MoveToTime(ITime time)
        {
            ThrowIfArgument.IsNull(nameof(time), time);

            return AddAction(new MoveToTimeAction(time));
        }

        public PatternBuilder MoveToFirstAnchor(object anchor)
        {
            ThrowIfArgument.IsNull(nameof(anchor), anchor);

            var counter = GetAnchorCounter(anchor);
            Debug.Assert(counter >= 1, "Count of anchor's times is less than 1.");

            return AddAction(new MoveToAnchorAction(anchor, AnchorPosition.First));
        }

        public PatternBuilder MoveToLastAnchor(object anchor)
        {
            ThrowIfArgument.IsNull(nameof(anchor), anchor);

            var counter = GetAnchorCounter(anchor);
            Debug.Assert(counter >= 1, "Count of anchor's times is less than 1.");

            return AddAction(new MoveToAnchorAction(anchor, AnchorPosition.Last));
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
            int counter;
            if (!_anchorCounters.TryGetValue(anchor, out counter))
                throw new ArgumentException($"Anchor {anchor} doesn't exist.", nameof(anchor));

            return counter;
        }

        #endregion
    }
}
