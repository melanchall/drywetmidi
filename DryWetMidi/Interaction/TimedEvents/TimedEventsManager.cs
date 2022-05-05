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

        /// <summary>
        /// Initializes a new instance of the <see cref="TimedEventsManager"/> with the specified events
        /// collection and comparison delegate for events that have same time.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> that holds events to manage.</param>
        /// <param name="timedEventDetectionSettings">Settings accoridng to which timed events should be detected
        /// and built.</param>
        /// <param name="sameTimeEventsComparison">Delegate to compare events with the same absolute time.</param>
        /// <remarks>
        /// If the <paramref name="sameTimeEventsComparison"/> is not specified events with the same time
        /// will be placed into the underlying events collection in order of adding them through the manager.
        /// If you want to specify custom order of such events you need to specify appropriate comparison delegate.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is <c>null</c>.</exception>
        public TimedEventsManager(
            EventsCollection eventsCollection,
            TimedEventDetectionSettings timedEventDetectionSettings = null,
            Comparison<MidiEvent> sameTimeEventsComparison = null)
            : base(
                  eventsCollection,
                  ObjectType.TimedEvent,
                  new ObjectDetectionSettings
                  {
                      TimedEventDetectionSettings = timedEventDetectionSettings
                  },
                  new TimedObjectsComparerOnSameEventTime(sameTimeEventsComparison))
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="TimedObjectsCollection{TObject}"/> with all timed events managed by the current
        /// <see cref="TimedEventsManager"/>.
        /// </summary>
        public TimedObjectsCollection<TimedEvent> Events => Objects;

        #endregion
    }
}
