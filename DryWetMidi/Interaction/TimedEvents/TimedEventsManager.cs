using System;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Provides a way to manage events of a MIDI file by their absolute time.
    /// </summary>
    [Obsolete("OBS11")]
    public sealed class TimedEventsManager : TimedObjectsManager<TimedEvent>
    {
        #region Constructor

        public TimedEventsManager(
            EventsCollection eventsCollection,
            Comparison<MidiEvent> sameTimeEventsComparison = null)
            : base(
                  eventsCollection,
                  ObjectType.TimedEvent,
                  null,
                  new TimedObjectsComparerOnSameEventTime(sameTimeEventsComparison))
        {
        }

        #endregion

        #region Properties

        public TimedObjectsCollection<TimedEvent> Events => Objects;

        #endregion
    }
}
