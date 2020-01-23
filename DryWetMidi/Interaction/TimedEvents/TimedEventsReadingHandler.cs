using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    public sealed class TimedEventsReadingHandler : ReadingHandler
    {
        #region Fields

        private readonly bool _sortEvents;
        private readonly List<TimedEvent> _timedEvents = new List<TimedEvent>();

        private IEnumerable<TimedEvent> _timedEventsProcessed;

        #endregion

        #region Constructor

        public TimedEventsReadingHandler(bool sortEvents)
            : base(TargetScope.Event)
        {
            _sortEvents = sortEvents;
        }

        #endregion

        #region Properties

        public IEnumerable<TimedEvent> TimedEvents => _timedEventsProcessed ??
            (_timedEventsProcessed = (_sortEvents ? _timedEvents.OrderBy(e => e.Time) : (IEnumerable<TimedEvent>)_timedEvents).ToList());

        #endregion

        #region Overrides

        public override void Initialize()
        {
            _timedEvents.Clear();
            _timedEventsProcessed = null;
        }

        public override void OnFinishEventReading(MidiEvent midiEvent, long absoluteTime)
        {
            _timedEvents.Add(new TimedEvent(midiEvent, absoluteTime));
        }

        #endregion
    }
}
