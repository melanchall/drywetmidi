using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Extension methods for managing MIDI events by their absolute time.
    /// </summary>
    public static class TimedEventsManagingUtilities
    {
        #region Methods

        /// <summary>
        /// Sets time of the specified timed event.
        /// </summary>
        /// <param name="timedEvent">Timed event to set time to.</param>
        /// <param name="time">Time to set to <paramref name="timedEvent"/>.</param>
        /// <param name="tempoMap">Tempo map that will be used for time conversion.</param>
        /// <returns>An input <paramref name="timedEvent"/> with new time.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="timedEvent"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="time"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static TimedEvent SetTime(this TimedEvent timedEvent, ITimeSpan time, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(timedEvent), timedEvent);
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            timedEvent.Time = TimeConverter.ConvertFrom(time, tempoMap);
            return timedEvent;
        }

        /// <summary>
        /// Creates an instance of the <see cref="TimedEventsManager"/> initializing it with the
        /// specified events collection and comparison delegate for events that have same time.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> that holds events to manage.</param>
        /// <param name="sameTimeEventsComparison">Delegate to compare events with the same absolute time.</param>
        /// <returns>An instance of the <see cref="TimedEventsManager"/> that can be used to manage
        /// events represented by the <paramref name="eventsCollection"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is <c>null</c>.</exception>
        public static TimedEventsManager ManageTimedEvents(this EventsCollection eventsCollection, Comparison<MidiEvent> sameTimeEventsComparison = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);

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
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is <c>null</c>.</exception>
        public static TimedEventsManager ManageTimedEvents(this TrackChunk trackChunk, Comparison<MidiEvent> sameTimeEventsComparison = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);

            return trackChunk.Events.ManageTimedEvents(sameTimeEventsComparison);
        }

        /// <summary>
        /// Gets timed events contained in the specified <see cref="EventsCollection"/>.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for events.</param>
        /// <returns>Collection of timed events contained in <paramref name="eventsCollection"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is <c>null</c>.</exception>
        public static ICollection<TimedEvent> GetTimedEvents(this EventsCollection eventsCollection)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);

            var result = new List<TimedEvent>(eventsCollection.Count);

            foreach (var timedEvent in eventsCollection.GetTimedEventsLazy())
            {
                result.Add(timedEvent);
            }

            return result;
        }

        /// <summary>
        /// Gets timed events contained in the specified <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for events.</param>
        /// <returns>Collection of timed events contained in <paramref name="trackChunk"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is <c>null</c>.</exception>
        public static ICollection<TimedEvent> GetTimedEvents(this TrackChunk trackChunk)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);

            return trackChunk.Events.GetTimedEvents();
        }

        /// <summary>
        /// Gets timed events contained in the specified collection of <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunks">Track chunks to search for events.</param>
        /// <returns>Collection of timed events contained in <paramref name="trackChunks"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunks"/> is <c>null</c>.</exception>
        public static ICollection<TimedEvent> GetTimedEvents(this IEnumerable<TrackChunk> trackChunks)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);

            var eventsCollections = trackChunks.Where(c => c != null).Select(c => c.Events).ToArray();
            var eventsCount = eventsCollections.Sum(c => c.Count);
            var result = new List<TimedEvent>(eventsCount);

            foreach (var timedEventTuple in eventsCollections.GetTimedEventsLazy(eventsCount))
            {
                result.Add(timedEventTuple.Item1);
            }

            return result;
        }

        /// <summary>
        /// Gets timed events contained in the specified <see cref="MidiFile"/>.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for events.</param>
        /// <returns>Collection of timed events contained in <paramref name="file"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="file"/> is <c>null</c>.</exception>
        public static ICollection<TimedEvent> GetTimedEvents(this MidiFile file)
        {
            ThrowIfArgument.IsNull(nameof(file), file);

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
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="eventsCollection"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="midiEvent"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="time"/> is negative.</exception>
        /// <exception cref="ArgumentException"><paramref name="midiEvent"/> is either system real-time or
        /// system common one.</exception>
        public static void AddEvent(this TimedEventsCollection eventsCollection, MidiEvent midiEvent, long time)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);
            ThrowIfArgument.IsNull(nameof(midiEvent), midiEvent);
            ThrowIfArgument.IsOfInvalidType<SystemRealTimeEvent, SystemCommonEvent>(nameof(midiEvent), midiEvent, "Event is either system real-time or system common one.");
            ThrowIfTimeArgument.IsNegative(nameof(time), time);

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
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="eventsCollection"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="midiEvent"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="time"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="midiEvent"/> is either system real-time or
        /// system common one.</exception>
        public static void AddEvent(this TimedEventsCollection eventsCollection, MidiEvent midiEvent, ITimeSpan time, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);
            ThrowIfArgument.IsNull(nameof(midiEvent), midiEvent);
            ThrowIfArgument.IsOfInvalidType<SystemRealTimeEvent, SystemCommonEvent>(nameof(midiEvent), midiEvent, "Event is either system real-time or system common one.");
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            eventsCollection.AddEvent(midiEvent, TimeConverter.ConvertFrom(time, tempoMap));
        }

        /// <summary>
        /// Performs the specified action on each <see cref="TimedEvent"/> contained in the <see cref="EventsCollection"/>.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for events to process.</param>
        /// <param name="action">The action to perform on each <see cref="TimedEvent"/> contained in the
        /// <paramref name="eventsCollection"/>.</param>
        /// <returns>Count of processed timed events.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="eventsCollection"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="action"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static int ProcessTimedEvents(this EventsCollection eventsCollection, Action<TimedEvent> action)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);
            ThrowIfArgument.IsNull(nameof(action), action);

            return eventsCollection.ProcessTimedEvents(action, timedEvent => true);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="TimedEvent"/> contained in the <see cref="EventsCollection"/>.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for events to process.</param>
        /// <param name="action">The action to perform on each <see cref="TimedEvent"/> contained in the
        /// <paramref name="eventsCollection"/>.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="TimedEvent"/> to process.</param>
        /// <returns>Count of processed timed events.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="eventsCollection"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="action"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="match"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static int ProcessTimedEvents(this EventsCollection eventsCollection, Action<TimedEvent> action, Predicate<TimedEvent> match)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);
            ThrowIfArgument.IsNull(nameof(action), action);
            ThrowIfArgument.IsNull(nameof(match), match);

            var iMatched = 0;

            var timesChanged = false;
            var timedEvents = new List<TimedEvent>(eventsCollection.Count);

            foreach (var timedEvent in eventsCollection.GetTimedEventsLazy(false))
            {
                if (match(timedEvent))
                {
                    var time = timedEvent.Time;
                    action(timedEvent);
                    timesChanged = timedEvent.Time != time;
                    iMatched++;
                }
                
                timedEvents.Add(timedEvent);
            }

            if (timesChanged)
                eventsCollection.SortAndUpdateEvents(timedEvents);

            return iMatched;
        }

        /// <summary>
        /// Performs the specified action on each <see cref="TimedEvent"/> contained in the <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for events to process.</param>
        /// <param name="action">The action to perform on each <see cref="TimedEvent"/> contained in the
        /// <paramref name="trackChunk"/>.</param>
        /// <returns>Count of processed timed events.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunk"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="action"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static int ProcessTimedEvents(this TrackChunk trackChunk, Action<TimedEvent> action)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(action), action);

            return trackChunk.ProcessTimedEvents(action, timedEvent => true);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="TimedEvent"/> contained in the <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for events to process.</param>
        /// <param name="action">The action to perform on each <see cref="TimedEvent"/> contained in the
        /// <paramref name="trackChunk"/>.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="TimedEvent"/> to process.</param>
        /// <returns>Count of processed timed events.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunk"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="action"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="match"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static int ProcessTimedEvents(this TrackChunk trackChunk, Action<TimedEvent> action, Predicate<TimedEvent> match)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(action), action);
            ThrowIfArgument.IsNull(nameof(match), match);

            return trackChunk.Events.ProcessTimedEvents(action, match);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="TimedEvent"/> contained in the collection of
        /// <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to search for events to process.</param>
        /// <param name="action">The action to perform on each <see cref="TimedEvent"/> contained in the
        /// <paramref name="trackChunks"/>.</param>
        /// <returns>Count of processed timed events.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunks"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="action"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static int ProcessTimedEvents(this IEnumerable<TrackChunk> trackChunks, Action<TimedEvent> action)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(action), action);

            return trackChunks.ProcessTimedEvents(action, timedEvent => true);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="TimedEvent"/> contained in the collection of
        /// <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to search for events to process.</param>
        /// <param name="action">The action to perform on each <see cref="TimedEvent"/> contained in the
        /// <paramref name="trackChunks"/>.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="TimedEvent"/> to process.</param>
        /// <returns>Count of processed timed events.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunks"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="action"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="match"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static int ProcessTimedEvents(this IEnumerable<TrackChunk> trackChunks, Action<TimedEvent> action, Predicate<TimedEvent> match)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(action), action);
            ThrowIfArgument.IsNull(nameof(match), match);

            var eventsCollections = trackChunks.Where(c => c != null).Select(c => c.Events).ToArray();
            var eventsCount = eventsCollections.Sum(c => c.Count);

            var iMatched = 0;

            var timesChanged = false;
            var timedEvents = new List<Tuple<TimedEvent, int>>(eventsCount);

            foreach (var timedEventTuple in eventsCollections.GetTimedEventsLazy(eventsCount, false))
            {
                var timedEvent = timedEventTuple.Item1;
                if (match(timedEvent))
                {
                    var deltaTime = timedEvent.Event.DeltaTime;
                    var time = timedEvent.Time;

                    action(timedEvent);
                    timedEvent.Event.DeltaTime = deltaTime;

                    timesChanged = timedEvent.Time != time;

                    iMatched++;
                }

                timedEvents.Add(timedEventTuple);
            }

            if (timesChanged)
                eventsCollections.SortAndUpdateEvents(timedEvents);

            return iMatched;
        }

        /// <summary>
        /// Performs the specified action on each <see cref="TimedEvent"/> contained in the <see cref="MidiFile"/>.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for events to process.</param>
        /// <param name="action">The action to perform on each <see cref="TimedEvent"/> contained in the
        /// <paramref name="file"/>.</param>
        /// <returns>Count of processed timed events.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="file"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="action"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static int ProcessTimedEvents(this MidiFile file, Action<TimedEvent> action)
        {
            ThrowIfArgument.IsNull(nameof(file), file);
            ThrowIfArgument.IsNull(nameof(action), action);

            return file.ProcessTimedEvents(action, timedEvent => true);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="TimedEvent"/> contained in the <see cref="MidiFile"/>.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for events to process.</param>
        /// <param name="action">The action to perform on each <see cref="TimedEvent"/> contained in the
        /// <paramref name="file"/>.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="TimedEvent"/> to process.</param>
        /// <returns>Count of processed timed events.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="file"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="action"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="match"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static int ProcessTimedEvents(this MidiFile file, Action<TimedEvent> action, Predicate<TimedEvent> match)
        {
            ThrowIfArgument.IsNull(nameof(file), file);
            ThrowIfArgument.IsNull(nameof(action), action);
            ThrowIfArgument.IsNull(nameof(match), match);

            return file.GetTrackChunks().ProcessTimedEvents(action, match);
        }

        /// <summary>
        /// Removes all the <see cref="TimedEvent"/> that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for events to remove.</param>
        /// <returns>Count of removed timed events.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is <c>null</c>.</exception>
        public static int RemoveTimedEvents(this EventsCollection eventsCollection)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);

            var result = eventsCollection.Count;
            eventsCollection.Clear();
            return result;
        }

        /// <summary>
        /// Removes all the <see cref="TimedEvent"/> that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for events to remove.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="TimedEvent"/> to remove.</param>
        /// <returns>Count of removed timed events.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="eventsCollection"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="match"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static int RemoveTimedEvents(this EventsCollection eventsCollection, Predicate<TimedEvent> match)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);
            ThrowIfArgument.IsNull(nameof(match), match);

            var eventsCount = eventsCollection.Count;

            var removedEventsCount = 0;
            var time = 0L;
            var latestTime = 0L;

            var events = eventsCollection._events;

            for (var i = 0; i < eventsCount; i++)
            {
                time += events[i].DeltaTime;
                var timedEvent = new TimedEvent(events[i], time);

                if (match(timedEvent))
                    removedEventsCount++;
                else
                {
                    events[i].DeltaTime = time - latestTime;
                    events[i - removedEventsCount] = events[i];
                    latestTime = time;
                }
            }

            if (removedEventsCount > 0)
                events.RemoveRange(eventsCount - removedEventsCount, removedEventsCount);

            return removedEventsCount;
        }

        /// <summary>
        /// Removes all the <see cref="TimedEvent"/> that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for events to remove.</param>
        /// <returns>Count of removed timed events.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is <c>null</c>.</exception>
        public static int RemoveTimedEvents(this TrackChunk trackChunk)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);

            var result = trackChunk.Events.Count;
            trackChunk.Events.Clear();
            return result;
        }

        /// <summary>
        /// Removes all the <see cref="TimedEvent"/> that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for events to remove.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="TimedEvent"/> to remove.</param>
        /// <returns>Count of removed timed events.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunk"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="match"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static int RemoveTimedEvents(this TrackChunk trackChunk, Predicate<TimedEvent> match)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(match), match);

            return trackChunk.Events.RemoveTimedEvents(match);
        }

        /// <summary>
        /// Removes all the <see cref="TimedEvent"/> that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to search for events to remove.</param>
        /// <returns>Count of removed timed events.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunks"/> is <c>null</c>.</exception>
        public static int RemoveTimedEvents(this IEnumerable<TrackChunk> trackChunks)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);

            var result = 0;

            foreach (var trackChunk in trackChunks)
            {
                result += trackChunk.RemoveTimedEvents();
            }

            return result;
        }

        /// <summary>
        /// Removes all the <see cref="TimedEvent"/> that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to search for events to remove.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="TimedEvent"/> to remove.</param>
        /// <returns>Count of removed timed events.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunks"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="match"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static int RemoveTimedEvents(this IEnumerable<TrackChunk> trackChunks, Predicate<TimedEvent> match)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(match), match);

            var eventsCollections = trackChunks.Where(c => c != null).Select(c => c.Events).ToArray();
            var eventsCount = eventsCollections.Sum(c => c.Count);

            var eventsCollectionsCount = eventsCollections.Length;

            if (eventsCollectionsCount == 0)
                return 0;

            if (eventsCollectionsCount == 1)
                return eventsCollections[0].RemoveTimedEvents(match);

            var eventsCollectionIndices = new int[eventsCollectionsCount];
            var eventsCollectionMaxIndices = eventsCollections.Select(c => c.Count - 1).ToArray();
            var eventsCollectionTimes = new long[eventsCollectionsCount];
            var eventsCollectionLatestTimes = new long[eventsCollectionsCount];
            var removedEventsCounts = new int[eventsCollectionsCount];

            for (var i = 0; i < eventsCount; i++)
            {
                var eventsCollectionIndex = 0;
                var minTime = long.MaxValue;

                for (var j = 0; j < eventsCollectionsCount; j++)
                {
                    var index = eventsCollectionIndices[j];
                    if (index > eventsCollectionMaxIndices[j])
                        continue;

                    var eventTime = eventsCollections[j][index].DeltaTime + eventsCollectionTimes[j];
                    if (eventTime < minTime)
                    {
                        minTime = eventTime;
                        eventsCollectionIndex = j;
                    }
                }

                var midiEvent = eventsCollections[eventsCollectionIndex][eventsCollectionIndices[eventsCollectionIndex]];

                var timedEvent = new TimedEvent(midiEvent, minTime);
                if (match(timedEvent))
                    removedEventsCounts[eventsCollectionIndex]++;
                else
                {
                    midiEvent.DeltaTime = minTime - eventsCollectionLatestTimes[eventsCollectionIndex];
                    eventsCollections[eventsCollectionIndex][eventsCollectionIndices[eventsCollectionIndex] - removedEventsCounts[eventsCollectionIndex]] = midiEvent;
                    eventsCollectionLatestTimes[eventsCollectionIndex] = minTime;
                }

                eventsCollectionTimes[eventsCollectionIndex] = minTime;
                eventsCollectionIndices[eventsCollectionIndex]++;
            }

            for (var i = 0; i < eventsCollectionsCount; i++)
            {
                var removedEventsCount = removedEventsCounts[i];
                if (removedEventsCount > 0)
                    eventsCollections[i]._events.RemoveRange(eventsCollections[i].Count - removedEventsCount, removedEventsCount);
            }

            return removedEventsCounts.Sum();
        }

        /// <summary>
        /// Removes all the <see cref="TimedEvent"/> that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for events to remove.</param>
        /// <returns>Count of removed timed events.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="file"/> is <c>null</c>.</exception>
        public static int RemoveTimedEvents(this MidiFile file)
        {
            ThrowIfArgument.IsNull(nameof(file), file);

            return file.GetTrackChunks().RemoveTimedEvents();
        }

        /// <summary>
        /// Removes all the <see cref="TimedEvent"/> that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for events to remove.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="TimedEvent"/> to remove.</param>
        /// <returns>Count of removed timed events.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="file"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="match"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static int RemoveTimedEvents(this MidiFile file, Predicate<TimedEvent> match)
        {
            ThrowIfArgument.IsNull(nameof(file), file);
            ThrowIfArgument.IsNull(nameof(match), match);

            return file.GetTrackChunks().RemoveTimedEvents(match);
        }

        internal static IEnumerable<Tuple<TimedEvent, int>> GetTimedEventsLazy(this IEnumerable<TrackChunk> trackChunks, bool cloneEvent = true)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);

            var eventsCollections = trackChunks.Where(c => c != null).Select(c => c.Events).ToArray();
            var eventsCount = eventsCollections.Sum(c => c.Count);

            return eventsCollections.GetTimedEventsLazy(eventsCount, cloneEvent);
        }

        internal static IEnumerable<Tuple<TimedEvent, int>> GetTimedEventsLazy(this EventsCollection[] eventsCollections, int eventsCount, bool cloneEvent = true)
        {
            var eventsCollectionsCount = eventsCollections.Length;

            if (eventsCollectionsCount == 0)
                yield break;

            if (eventsCollectionsCount == 1)
            {
                foreach (var timedEvent in eventsCollections[0].GetTimedEventsLazy(false))
                {
                    yield return Tuple.Create(timedEvent, 0);
                }

                yield break;
            }

            var eventsCollectionIndices = new int[eventsCollectionsCount];
            var eventsCollectionMaxIndices = eventsCollections.Select(c => c.Count - 1).ToArray();
            var eventsCollectionTimes = new long[eventsCollectionsCount];

            for (var i = 0; i < eventsCount; i++)
            {
                var eventsCollectionIndex = 0;
                var minTime = long.MaxValue;

                for (var j = 0; j < eventsCollectionsCount; j++)
                {
                    var index = eventsCollectionIndices[j];
                    if (index > eventsCollectionMaxIndices[j])
                        continue;

                    var eventTime = eventsCollections[j][index].DeltaTime + eventsCollectionTimes[j];
                    if (eventTime < minTime)
                    {
                        minTime = eventTime;
                        eventsCollectionIndex = j;
                    }
                }

                var midiEvent = eventsCollections[eventsCollectionIndex][eventsCollectionIndices[eventsCollectionIndex]];
                var timedEvent = new TimedEvent(cloneEvent ? midiEvent.Clone() : midiEvent);
                timedEvent._time = minTime;

                yield return Tuple.Create(timedEvent, eventsCollectionIndex);

                eventsCollectionTimes[eventsCollectionIndex] = minTime;
                eventsCollectionIndices[eventsCollectionIndex]++;
            }
        }

        internal static IEnumerable<TimedEvent> GetTimedEventsLazy(this IEnumerable<MidiEvent> events, bool cloneEvent = true)
        {
            var time = 0L;

            foreach (var midiEvent in events)
            {
                if (midiEvent == null)
                    continue;

                time += midiEvent.DeltaTime;

                var timedEvent = new TimedEvent(cloneEvent ? midiEvent.Clone() : midiEvent);
                timedEvent._time = time;

                yield return timedEvent;
            }
        }

        internal static void SortAndUpdateEvents(this EventsCollection eventsCollection, IEnumerable<TimedEvent> timedEvents)
        {
            var time = 0L;
            var i = 0;

            foreach (var e in timedEvents.OrderBy(e => e.Time))
            {
                var midiEvent = e.Event;
                midiEvent.DeltaTime = e.Time - time;
                eventsCollection[i++] = midiEvent;

                time = e.Time;
            }
        }

        internal static void SortAndUpdateEvents(this EventsCollection[] eventsCollections, IEnumerable<Tuple<TimedEvent, int>> timedEvents)
        {
            var times = new long[eventsCollections.Length];
            var indices = new int[eventsCollections.Length];

            foreach (var e in timedEvents.OrderBy(e => e.Item1.Time))
            {
                var midiEvent = e.Item1.Event;
                midiEvent.DeltaTime = e.Item1.Time - times[e.Item2];
                eventsCollections[e.Item2][indices[e.Item2]++] = midiEvent;

                times[e.Item2] = e.Item1.Time;
            }
        }

        #endregion
    }
}
