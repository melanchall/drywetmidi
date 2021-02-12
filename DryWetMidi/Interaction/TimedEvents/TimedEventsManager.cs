using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Provides a way to manage events of a MIDI file by their absolute time.
    /// </summary>
    /// <remarks>
    /// To start manage events you need to get an instance of the <see cref="TimedEventsManager"/>. To
    /// finish managing you need to call the <see cref="SaveChanges"/> or <see cref="Dispose()"/> method.
    /// Since the manager implements <see cref="IDisposable"/> it is recommended to manage events within
    /// using block.
    /// </remarks>
    public sealed class TimedEventsManager : IDisposable
    {
        #region Fields

        private readonly EventsCollection _eventsCollection;

        private bool _disposed;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TimedEventsManager"/> with the specified events
        /// collection and comparison delegate for events that have same time.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> that holds events to manage.</param>
        /// <param name="sameTimeEventsComparison">Delegate to compare events with the same absolute time.</param>
        /// <remarks>
        /// If the <paramref name="sameTimeEventsComparison"/> is not specified events with the same time
        /// will be placed into the underlying events collection in order of adding them through the manager.
        /// If you want to specify custom order of such events you need to specify appropriate comparison delegate.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is <c>null</c>.</exception>
        public TimedEventsManager(EventsCollection eventsCollection, Comparison<MidiEvent> sameTimeEventsComparison = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);

            _eventsCollection = eventsCollection;
            Events = new TimedEventsCollection(eventsCollection.GetTimedEventsLazy(), sameTimeEventsComparison);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets current collection of timed events reflecting all changes made by the current
        /// <see cref="TimedEventsManager"/>.
        /// </summary>
        public TimedEventsCollection Events { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Saves all events that were managed with the current <see cref="TimedEventsManager"/> updating
        /// underlying events collection.
        /// </summary>
        /// <remarks>
        /// This method will rewrite content of the events collection was used to construct the current
        /// <see cref="TimedEventsManager"/> with events were managed by this manager. Also all delta-times
        /// of wrapped events will be recalculated according to the <see cref="TimedEvent.Time"/> of
        /// event wrappers.
        /// </remarks>
        public void SaveChanges()
        {
            _eventsCollection.Clear();

            var time = 0L;

            foreach (var e in Events)
            {
                var midiEvent = e.Event;
                midiEvent.DeltaTime = e.Time - time;
                _eventsCollection.Add(midiEvent);

                time = e.Time;
            }
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                SaveChanges();

            _disposed = true;
        }

        #endregion
    }
}
