using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Extension methods for managing MIDI events by their absolute time.
    /// </summary>
    public static class TimedEventsManagingUtilities
    {
        #region Methods

        /// <summary>
        /// Creates an instance of the <see cref="TimedEventsManager"/> initializing it with the
        /// specified events collection and comparison delegate for events that have same time.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> that holds events to manage.</param>
        /// <param name="sameTimeEventsComparison">Delegate to compare events with the same absolute time.</param>
        /// <returns>An instance of the <see cref="TimedEventsManager"/> that can be used to manage
        /// events represented by the <paramref name="eventsCollection"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is null.</exception>
        public static TimedEventsManager ManageTimedEvents(this EventsCollection eventsCollection, Comparison<MidiEvent> sameTimeEventsComparison = null)
        {
            if (eventsCollection == null)
                throw new ArgumentNullException(nameof(eventsCollection));

            return new TimedEventsManager(eventsCollection, sameTimeEventsComparison);
        }

        /// <summary>
        /// Creates an instance of the <see cref="TimedEventsManager"/> initializing it with the
        /// events collection of the specified track chunk and comparison delegate for events
        /// that have same time.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> that holds events to manage.</param>
        /// <param name="sameTimeEventsComparison">Delegate to compare events with the same absolute time.</param>
        /// <returns>An instance of the <see cref="TimedEventsManager"/> that can be used to manage
        /// events represented by the <paramref name="trackChunk"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is null.</exception>
        public static TimedEventsManager ManageTimedEvents(this TrackChunk trackChunk, Comparison<MidiEvent> sameTimeEventsComparison = null)
        {
            if (trackChunk == null)
                throw new ArgumentNullException(nameof(trackChunk));

            return trackChunk.Events.ManageTimedEvents(sameTimeEventsComparison);
        }

        public static IEnumerable<TimedEvent> GetTimedEvents(this EventsCollection eventsCollection)
        {
            if (eventsCollection == null)
                throw new ArgumentNullException(nameof(eventsCollection));

            return eventsCollection.ManageTimedEvents().Events;
        }

        /// <summary>
        /// Gets timed events contained in the specified track chunk.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for events.</param>
        /// <returns>Collection of timed events contained in <paramref name="trackChunk"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is null.</exception>
        public static IEnumerable<TimedEvent> GetTimedEvents(this TrackChunk trackChunk)
        {
            if (trackChunk == null)
                throw new ArgumentNullException(nameof(trackChunk));

            return trackChunk.Events.GetTimedEvents();
        }

        /// <summary>
        /// Gets timed events contained in the specified track chunks.
        /// </summary>
        /// <param name="trackChunks">Track chunks to search for events.</param>
        /// <returns>Collection of timed events contained in <paramref name="trackChunks"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunks"/> is null.</exception>
        public static IEnumerable<TimedEvent> GetTimedEvents(this IEnumerable<TrackChunk> trackChunks)
        {
            if (trackChunks == null)
                throw new ArgumentNullException(nameof(trackChunks));

            return trackChunks.Where(c => c != null)
                              .SelectMany(GetTimedEvents)
                              .OrderBy(n => n.Time);
        }

        /// <summary>
        /// Gets timed events contained in the specified MIDI file.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for events.</param>
        /// <returns>Collection of timed events contained in <paramref name="file"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="file"/> is null.</exception>
        public static IEnumerable<TimedEvent> GetTimedEvents(this MidiFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            return file.GetTrackChunks().GetTimedEvents();
        }

        /// <summary>
        /// Adds a <see cref="MidiEvent"/> into a <see cref="TimedEventsCollection"/> with the specified
        /// absolute time.
        /// </summary>
        /// <param name="eventsCollection"><see cref="TimedEventsCollection"/> to add an event into.</param>
        /// <param name="midiEvent">Event to add into the <paramref name="eventsCollection"/>.</param>
        /// <param name="time">Absolute time that will be assigned to the <paramref name="midiEvent"/>
        /// when it will be placed into the <paramref name="eventsCollection"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is null. -or-
        /// <paramref name="midiEvent"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="time"/> is negative.</exception>
        public static void AddEvent(this TimedEventsCollection eventsCollection, MidiEvent midiEvent, long time)
        {
            if (eventsCollection == null)
                throw new ArgumentNullException(nameof(eventsCollection));

            if (midiEvent == null)
                throw new ArgumentNullException(nameof(midiEvent));

            if (time < 0)
                throw new ArgumentOutOfRangeException(nameof(time), time, "Time is negative.");

            eventsCollection.Add(new TimedEvent(midiEvent, time));
        }

        /// <summary>
        /// Adds a <see cref="MidiEvent"/> into a <see cref="TimedEventsCollection"/> with the specified
        /// absolute time.
        /// </summary>
        /// <param name="eventsCollection"><see cref="TimedEventsCollection"/> to add an event into.</param>
        /// <param name="midiEvent">Event to add into the <paramref name="eventsCollection"/>.</param>
        /// <param name="time">Absolute time that will be assigned to the <paramref name="midiEvent"/>
        /// when it will be placed into the <paramref name="eventsCollection"/>.</param>
        /// <param name="tempoMap">Tempo map used to place <paramref name="midiEvent"/> into the
        /// <paramref name="eventsCollection"/> with the specified time.</param>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is null. -or-
        /// <paramref name="midiEvent"/> is null. -or- <paramref name="time"/> is null. -or-
        /// <paramref name="tempoMap"/> is null.</exception>
        public static void AddEvent(this TimedEventsCollection eventsCollection, MidiEvent midiEvent, ITime time, TempoMap tempoMap)
        {
            if (eventsCollection == null)
                throw new ArgumentNullException(nameof(eventsCollection));

            if (midiEvent == null)
                throw new ArgumentNullException(nameof(midiEvent));

            if (time == null)
                throw new ArgumentNullException(nameof(time));

            if (tempoMap == null)
                throw new ArgumentNullException(nameof(tempoMap));

            eventsCollection.AddEvent(midiEvent, TimeConverter.ConvertFrom(time, tempoMap));
        }

        public static void ProcessTimedEvents(this EventsCollection eventsCollection, Action<TimedEvent> action, Predicate<TimedEvent> match = null)
        {
            if (eventsCollection == null)
                throw new ArgumentNullException(nameof(eventsCollection));

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            using (var timedEventsManager = eventsCollection.ManageTimedEvents())
            {
                foreach (var timedEvent in timedEventsManager.Events.Where(e => match?.Invoke(e) != false))
                {
                    action(timedEvent);
                }
            }
        }

        public static void ProcessTimedEvents(this TrackChunk trackChunk, Action<TimedEvent> action, Predicate<TimedEvent> match = null)
        {
            if (trackChunk == null)
                throw new ArgumentNullException(nameof(trackChunk));

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            trackChunk.Events.ProcessTimedEvents(action, match);
        }

        public static void ProcessTimedEvents(this IEnumerable<TrackChunk> trackChunks, Action<TimedEvent> action, Predicate<TimedEvent> match = null)
        {
            if (trackChunks == null)
                throw new ArgumentNullException(nameof(trackChunks));

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            foreach (var trackChunk in trackChunks)
            {
                trackChunk?.ProcessTimedEvents(action, match);
            }
        }

        public static void ProcessTimedEvents(this MidiFile file, Action<TimedEvent> action, Predicate<TimedEvent> match = null)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            file.GetTrackChunks().ProcessTimedEvents(action, match);
        }

        public static void RemoveTimedEvents(this EventsCollection eventsCollection, Predicate<TimedEvent> match = null)
        {
            if (eventsCollection == null)
                throw new ArgumentNullException(nameof(eventsCollection));

            using (var timedEventsManager = eventsCollection.ManageTimedEvents())
            {
                timedEventsManager.Events.RemoveAll(match ?? (e => true));
            }
        }

        public static void RemoveTimedEvents(this TrackChunk trackChunk, Predicate<TimedEvent> match = null)
        {
            if (trackChunk == null)
                throw new ArgumentNullException(nameof(trackChunk));

            trackChunk.Events.RemoveTimedEvents(match);
        }

        public static void RemoveTimedEvents(this IEnumerable<TrackChunk> trackChunks, Predicate<TimedEvent> match = null)
        {
            if (trackChunks == null)
                throw new ArgumentNullException(nameof(trackChunks));

            foreach (var trackChunk in trackChunks)
            {
                trackChunk?.RemoveTimedEvents(match);
            }
        }

        public static void RemoveTimedEvents(this MidiFile file, Predicate<TimedEvent> match = null)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            file.GetTrackChunks().RemoveTimedEvents(match);
        }

        #endregion
    }
}
