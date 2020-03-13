using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Collects timed events during MIDI data reading.
    /// </summary>
    /// <remarks>
    /// This handler can be added to the <see cref="ReadingSettings.ReadingHandlers"/> collection to
    /// collect timed events along with MIDI data reading. Scope of the handler is
    /// <see cref="ReadingHandler.TargetScope.Event"/>.
    /// </remarks>
    public sealed class TimedEventsReadingHandler : ReadingHandler
    {
        #region Fields

        private readonly bool _sortEvents;
        private readonly List<TimedEvent> _timedEvents = new List<TimedEvent>();

        private IEnumerable<TimedEvent> _timedEventsProcessed;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TimedEventsReadingHandler"/> with the specified
        /// value indicating whether timed events should be automatically sorted on <see cref="TimedEvents"/> get.
        /// </summary>
        /// <param name="sortEvents">A value indicating whether timed events should be automatically sorted
        /// on <see cref="TimedEvents"/> get.</param>
        public TimedEventsReadingHandler(bool sortEvents)
            : base(TargetScope.Event)
        {
            _sortEvents = sortEvents;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the timed events collected during MIDI data reading. If the current <see cref="TimedEventsReadingHandler"/>
        /// was created with <c>sortEvents</c> set to <c>true</c>, the returned timed events will be sorted by time.
        /// </summary>
        public IEnumerable<TimedEvent> TimedEvents => _timedEventsProcessed ??
            (_timedEventsProcessed = (_sortEvents ? _timedEvents.OrderBy(e => e.Time) : (IEnumerable<TimedEvent>)_timedEvents).ToList());

        #endregion

        #region Overrides

        /// <summary>
        /// Initializes handler. This method will be called before reading MIDI data.
        /// </summary>
        public override void Initialize()
        {
            _timedEvents.Clear();
            _timedEventsProcessed = null;
        }

        /// <summary>
        /// Handles finish of MIDI event reading. Called after MIDI event is read and before
        /// putting it to <see cref="TrackChunk.Events"/> collection.
        /// </summary>
        /// <param name="midiEvent">MIDI event read.</param>
        /// <param name="absoluteTime">Absolute time of <paramref name="midiEvent"/>.</param>
        public override void OnFinishEventReading(MidiEvent midiEvent, long absoluteTime)
        {
            _timedEvents.Add(new TimedEvent(midiEvent, absoluteTime));
        }

        #endregion
    }
}
