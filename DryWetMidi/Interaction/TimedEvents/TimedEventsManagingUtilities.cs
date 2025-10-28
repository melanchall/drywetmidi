using System;
using System.Collections.Generic;
using System.Data;
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
        /// Creates an instance of the <see cref="TimedObjectsManager{TimedEvent}"/> initializing it with the
        /// specified events collection. More info in the <see href="xref:a_managers">Objects managers</see> article.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> that holds events to manage.</param>
        /// <param name="settings">Settings according to which timed events should be detected and built.</param>
        /// <param name="comparer">Comparer that will be used to order objects on enumerating and saving objects
        /// back to the <paramref name="eventsCollection"/> via <see cref="TimedObjectsManager{TObject}.SaveChanges"/>
        /// or <see cref="TimedObjectsManager{TObject}.Dispose()"/>.</param>
        /// <returns>An instance of the <see cref="TimedObjectsManager{TimedEvent}"/> that can be used to manage
        /// events represented by the <paramref name="eventsCollection"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is <c>null</c>.</exception>
        public static TimedObjectsManager<TimedEvent> ManageTimedEvents(this EventsCollection eventsCollection, TimedEventDetectionSettings settings = null, TimedObjectsComparer comparer = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);

            return new TimedObjectsManager<TimedEvent>(
                eventsCollection,
                new ObjectDetectionSettings
                {
                    TimedEventDetectionSettings = settings
                },
                comparer);
        }

        /// <summary>
        /// Creates an instance of the <see cref="TimedObjectsManager{TimedEvent}"/> initializing it with the
        /// events collection of the specified track chunk. More info in the
        /// <see href="xref:a_managers">Objects managers</see> article.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> that holds events to manage.</param>
        /// <param name="settings">Settings according to which timed events should be detected and built.</param>
        /// <param name="comparer">Comparer that will be used to order objects on enumerating and saving objects
        /// back to the <paramref name="trackChunk"/> via <see cref="TimedObjectsManager{TObject}.SaveChanges"/>
        /// or <see cref="TimedObjectsManager{TObject}.Dispose()"/>.</param>
        /// <returns>An instance of the <see cref="TimedObjectsManager{TimedEvent}"/> that can be used to manage
        /// events represented by the <paramref name="trackChunk"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is <c>null</c>.</exception>
        public static TimedObjectsManager<TimedEvent> ManageTimedEvents(this TrackChunk trackChunk, TimedEventDetectionSettings settings = null, TimedObjectsComparer comparer = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);

            return trackChunk.Events.ManageTimedEvents(settings, comparer);
        }

        /// <summary>
        /// Gets timed events contained in the specified <see cref="EventsCollection"/>. More info in the
        /// <see href="xref:a_getting_objects#gettimedevents">Getting objects: GetTimedEvents</see> article.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for events.</param>
        /// <param name="settings">Settings according to which timed events should be detected and built.</param>
        /// <returns>Collection of timed events contained in <paramref name="eventsCollection"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is <c>null</c>.</exception>
        /// <seealso cref="ProcessTimedEvents(EventsCollection, Action{TimedEvent}, Predicate{TimedEvent}, TimedEventDetectionSettings, TimedEventProcessingHint)"/>
        /// <seealso cref="ProcessTimedEvents(EventsCollection, Action{TimedEvent}, TimedEventDetectionSettings, TimedEventProcessingHint)"/>
        /// <seealso cref="RemoveTimedEvents(EventsCollection)"/>
        /// <seealso cref="RemoveTimedEvents(EventsCollection, Predicate{TimedEvent}, TimedEventDetectionSettings)"/>
        /// <seealso cref="GetObjectsUtilities"/>
        public static ICollection<TimedEvent> GetTimedEvents(this EventsCollection eventsCollection, TimedEventDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);

            var result = new List<TimedEvent>(eventsCollection.Count);

            foreach (var timedEvent in eventsCollection.GetTimedEventsLazy(settings, 0))
            {
                result.Add(timedEvent);
            }

            return new SortedImmutableCollection<TimedEvent>(result);
        }

        /// <summary>
        /// Gets timed events contained in the specified <see cref="TrackChunk"/>. More info in the
        /// <see href="xref:a_getting_objects#gettimedevents">Getting objects: GetTimedEvents</see> article.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for events.</param>
        /// <param name="settings">Settings according to which timed events should be detected and built.</param>
        /// <returns>Collection of timed events contained in <paramref name="trackChunk"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is <c>null</c>.</exception>
        /// <seealso cref="ProcessTimedEvents(TrackChunk, Action{TimedEvent}, Predicate{TimedEvent}, TimedEventDetectionSettings, TimedEventProcessingHint)"/>
        /// <seealso cref="ProcessTimedEvents(TrackChunk, Action{TimedEvent}, TimedEventDetectionSettings, TimedEventProcessingHint)"/>
        /// <seealso cref="RemoveTimedEvents(TrackChunk)"/>
        /// <seealso cref="RemoveTimedEvents(TrackChunk, Predicate{TimedEvent}, TimedEventDetectionSettings)"/>
        /// <seealso cref="GetObjectsUtilities"/>
        public static ICollection<TimedEvent> GetTimedEvents(this TrackChunk trackChunk, TimedEventDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);

            return trackChunk.Events.GetTimedEvents(settings);
        }

        /// <summary>
        /// Gets timed events contained in the specified collection of <see cref="TrackChunk"/>. More info in the
        /// <see href="xref:a_getting_objects#gettimedevents">Getting objects: GetTimedEvents</see> article.
        /// </summary>
        /// <param name="trackChunks">Track chunks to search for events.</param>
        /// <param name="settings">Settings according to which timed events should be detected and built.</param>
        /// <returns>Collection of timed events contained in <paramref name="trackChunks"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunks"/> is <c>null</c>.</exception>
        /// <seealso cref="ProcessTimedEvents(IEnumerable{TrackChunk}, Action{TimedEvent}, Predicate{TimedEvent}, TimedEventDetectionSettings, TimedEventProcessingHint)"/>
        /// <seealso cref="ProcessTimedEvents(IEnumerable{TrackChunk}, Action{TimedEvent}, TimedEventDetectionSettings, TimedEventProcessingHint)"/>
        /// <seealso cref="RemoveTimedEvents(IEnumerable{TrackChunk})"/>
        /// <seealso cref="RemoveTimedEvents(IEnumerable{TrackChunk}, Predicate{TimedEvent}, TimedEventDetectionSettings)"/>
        /// <seealso cref="GetObjectsUtilities"/>
        public static ICollection<TimedEvent> GetTimedEvents(this IEnumerable<TrackChunk> trackChunks, TimedEventDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);

            var eventsCollections = trackChunks.Where(c => c != null).Select(c => c.Events).ToArray();
            var result = new List<TimedEvent>();

            foreach (var timedEvent in eventsCollections.GetTimedEventsLazy(settings))
            {
                result.Add(timedEvent);
            }

            return new SortedImmutableCollection<TimedEvent>(result);
        }

        /// <summary>
        /// Gets timed events contained in the specified <see cref="MidiFile"/>. More info in the
        /// <see href="xref:a_getting_objects#gettimedevents">Getting objects: GetTimedEvents</see> article.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for events.</param>
        /// <param name="settings">Settings according to which timed events should be detected and built.</param>
        /// <returns>Collection of timed events contained in <paramref name="file"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="file"/> is <c>null</c>.</exception>
        /// <seealso cref="ProcessTimedEvents(MidiFile, Action{TimedEvent}, Predicate{TimedEvent}, TimedEventDetectionSettings, TimedEventProcessingHint)"/>
        /// <seealso cref="ProcessTimedEvents(MidiFile, Action{TimedEvent}, TimedEventDetectionSettings, TimedEventProcessingHint)"/>
        /// <seealso cref="RemoveTimedEvents(MidiFile)"/>
        /// <seealso cref="RemoveTimedEvents(MidiFile, Predicate{TimedEvent}, TimedEventDetectionSettings)"/>
        /// <seealso cref="GetObjectsUtilities"/>
        public static ICollection<TimedEvent> GetTimedEvents(this MidiFile file, TimedEventDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(file), file);

            return file.GetTrackChunks().GetTimedEvents(settings);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="TimedEvent"/> contained in the <see cref="EventsCollection"/>.
        /// More info in the <see href="xref:a_processing_objects#processtimedevents">Processing objects: ProcessTimedEvents</see> article.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for events to process.</param>
        /// <param name="action">The action to perform on each <see cref="TimedEvent"/> contained in the
        /// <paramref name="eventsCollection"/>.</param>
        /// <param name="settings">Settings according to which timed events should be detected and built.</param>
        /// <param name="hint">Hint which tells the processing algorithm how it can optimize its performance.
        /// The default value is <see cref="TimedEventProcessingHint.Default"/>.</param>
        /// <remarks>
        /// Note that you can always use <see href="xref:a_managers">an object manager</see> to
        /// perform any manipulations with timed events but dedicated methods of the <see cref="TimedEventsManagingUtilities"/> will
        /// always be faster and will consume less memory.
        /// </remarks>
        /// <returns>Count of processed timed events.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="eventsCollection"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="action"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <seealso cref="TimedObjectUtilities.ProcessObjects(EventsCollection, ObjectType, Action{ITimedObject}, ObjectDetectionSettings, ObjectProcessingHint)"/>
        public static int ProcessTimedEvents(this EventsCollection eventsCollection, Action<TimedEvent> action, TimedEventDetectionSettings settings = null, TimedEventProcessingHint hint = TimedEventProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);
            ThrowIfArgument.IsNull(nameof(action), action);

            return eventsCollection.ProcessTimedEvents(action, timedEvent => true, settings, hint);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="TimedEvent"/> contained in the <see cref="EventsCollection"/>.
        /// More info in the <see href="xref:a_processing_objects#processtimedevents">Processing objects: ProcessTimedEvents</see> article.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for events to process.</param>
        /// <param name="action">The action to perform on each <see cref="TimedEvent"/> contained in the
        /// <paramref name="eventsCollection"/>.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="TimedEvent"/> to process.</param>
        /// <param name="settings">Settings according to which timed events should be detected and built.</param>
        /// <param name="hint">Hint which tells the processing algorithm how it can optimize its performance.
        /// The default value is <see cref="TimedEventProcessingHint.Default"/>.</param>
        /// <remarks>
        /// Note that you can always use <see href="xref:a_managers">an object manager</see> to
        /// perform any manipulations with timed events but dedicated methods of the <see cref="TimedEventsManagingUtilities"/> will
        /// always be faster and will consume less memory.
        /// </remarks>
        /// <returns>Count of processed timed events.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
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
        /// <seealso cref="TimedObjectUtilities.ProcessObjects(EventsCollection, ObjectType, Action{ITimedObject}, Predicate{ITimedObject}, ObjectDetectionSettings, ObjectProcessingHint)"/>
        public static int ProcessTimedEvents(this EventsCollection eventsCollection, Action<TimedEvent> action, Predicate<TimedEvent> match, TimedEventDetectionSettings settings = null, TimedEventProcessingHint hint = TimedEventProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);
            ThrowIfArgument.IsNull(nameof(action), action);
            ThrowIfArgument.IsNull(nameof(match), match);

            return new[] { eventsCollection }.ProcessTimedEventsInternal(action, match, settings, hint);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="TimedEvent"/> contained in the <see cref="TrackChunk"/>.
        /// More info in the <see href="xref:a_processing_objects#processtimedevents">Processing objects: ProcessTimedEvents</see> article.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for events to process.</param>
        /// <param name="action">The action to perform on each <see cref="TimedEvent"/> contained in the
        /// <paramref name="trackChunk"/>.</param>
        /// <param name="settings">Settings according to which timed events should be detected and built.</param>
        /// <param name="hint">Hint which tells the processing algorithm how it can optimize its performance.
        /// The default value is <see cref="TimedEventProcessingHint.Default"/>.</param>
        /// <remarks>
        /// Note that you can always use <see href="xref:a_managers">an object manager</see> to
        /// perform any manipulations with timed events but dedicated methods of the <see cref="TimedEventsManagingUtilities"/> will
        /// always be faster and will consume less memory.
        /// </remarks>
        /// <returns>Count of processed timed events.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunk"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="action"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <seealso cref="TimedObjectUtilities.ProcessObjects(TrackChunk, ObjectType, Action{ITimedObject}, ObjectDetectionSettings, ObjectProcessingHint)"/>
        public static int ProcessTimedEvents(this TrackChunk trackChunk, Action<TimedEvent> action, TimedEventDetectionSettings settings = null, TimedEventProcessingHint hint = TimedEventProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(action), action);

            return trackChunk.ProcessTimedEvents(action, timedEvent => true, settings, hint);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="TimedEvent"/> contained in the <see cref="TrackChunk"/>.
        /// More info in the <see href="xref:a_processing_objects#processtimedevents">Processing objects: ProcessTimedEvents</see> article.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for events to process.</param>
        /// <param name="action">The action to perform on each <see cref="TimedEvent"/> contained in the
        /// <paramref name="trackChunk"/>.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="TimedEvent"/> to process.</param>
        /// <param name="settings">Settings according to which timed events should be detected and built.</param>
        /// <param name="hint">Hint which tells the processing algorithm how it can optimize its performance.
        /// The default value is <see cref="TimedEventProcessingHint.Default"/>.</param>
        /// <remarks>
        /// Note that you can always use <see href="xref:a_managers">an object manager</see> to
        /// perform any manipulations with timed events but dedicated methods of the <see cref="TimedEventsManagingUtilities"/> will
        /// always be faster and will consume less memory.
        /// </remarks>
        /// <returns>Count of processed timed events.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
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
        /// <seealso cref="TimedObjectUtilities.ProcessObjects(TrackChunk, ObjectType, Action{ITimedObject}, Predicate{ITimedObject}, ObjectDetectionSettings, ObjectProcessingHint)"/>
        public static int ProcessTimedEvents(this TrackChunk trackChunk, Action<TimedEvent> action, Predicate<TimedEvent> match, TimedEventDetectionSettings settings = null, TimedEventProcessingHint hint = TimedEventProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(action), action);
            ThrowIfArgument.IsNull(nameof(match), match);

            return trackChunk.Events.ProcessTimedEvents(action, match, settings, hint);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="TimedEvent"/> contained in the collection of
        /// <see cref="TrackChunk"/>. More info in the
        /// <see href="xref:a_processing_objects#processtimedevents">Processing objects: ProcessTimedEvents</see> article.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to search for events to process.</param>
        /// <param name="action">The action to perform on each <see cref="TimedEvent"/> contained in the
        /// <paramref name="trackChunks"/>.</param>
        /// <param name="settings">Settings according to which timed events should be detected and built.</param>
        /// <param name="hint">Hint which tells the processing algorithm how it can optimize its performance.
        /// The default value is <see cref="TimedEventProcessingHint.Default"/>.</param>
        /// <remarks>
        /// Note that you can always use <see href="xref:a_managers">an object manager</see> to
        /// perform any manipulations with timed events but dedicated methods of the <see cref="TimedEventsManagingUtilities"/> will
        /// always be faster and will consume less memory.
        /// </remarks>
        /// <returns>Count of processed timed events.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunks"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="action"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <seealso cref="TimedObjectUtilities.ProcessObjects(IEnumerable{TrackChunk}, ObjectType, Action{ITimedObject}, ObjectDetectionSettings, ObjectProcessingHint)"/>
        public static int ProcessTimedEvents(this IEnumerable<TrackChunk> trackChunks, Action<TimedEvent> action, TimedEventDetectionSettings settings = null, TimedEventProcessingHint hint = TimedEventProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(action), action);

            return trackChunks.ProcessTimedEvents(action, timedEvent => true, settings, hint);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="TimedEvent"/> contained in the collection of
        /// <see cref="TrackChunk"/>. More info in the
        /// <see href="xref:a_processing_objects#processtimedevents">Processing objects: ProcessTimedEvents</see> article.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to search for events to process.</param>
        /// <param name="action">The action to perform on each <see cref="TimedEvent"/> contained in the
        /// <paramref name="trackChunks"/>.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="TimedEvent"/> to process.</param>
        /// <param name="settings">Settings according to which timed events should be detected and built.</param>
        /// <param name="hint">Hint which tells the processing algorithm how it can optimize its performance.
        /// The default value is <see cref="TimedEventProcessingHint.Default"/>.</param>
        /// <remarks>
        /// Note that you can always use <see href="xref:a_managers">an object manager</see> to
        /// perform any manipulations with timed events but dedicated methods of the <see cref="TimedEventsManagingUtilities"/> will
        /// always be faster and will consume less memory.
        /// </remarks>
        /// <returns>Count of processed timed events.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
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
        /// <seealso cref="TimedObjectUtilities.ProcessObjects(IEnumerable{TrackChunk}, ObjectType, Action{ITimedObject}, Predicate{ITimedObject}, ObjectDetectionSettings, ObjectProcessingHint)"/>
        public static int ProcessTimedEvents(this IEnumerable<TrackChunk> trackChunks, Action<TimedEvent> action, Predicate<TimedEvent> match, TimedEventDetectionSettings settings = null, TimedEventProcessingHint hint = TimedEventProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(action), action);
            ThrowIfArgument.IsNull(nameof(match), match);

            return trackChunks
                .Where(c => c != null)
                .Select(c => c.Events)
                .ProcessTimedEventsInternal(action, match, settings, hint);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="TimedEvent"/> contained in the <see cref="MidiFile"/>.
        /// More info in the <see href="xref:a_processing_objects#processtimedevents">Processing objects: ProcessTimedEvents</see> article.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for events to process.</param>
        /// <param name="action">The action to perform on each <see cref="TimedEvent"/> contained in the
        /// <paramref name="file"/>.</param>
        /// <param name="settings">Settings according to which timed events should be detected and built.</param>
        /// <param name="hint">Hint which tells the processing algorithm how it can optimize its performance.
        /// The default value is <see cref="TimedEventProcessingHint.Default"/>.</param>
        /// <remarks>
        /// Note that you can always use <see href="xref:a_managers">an object manager</see> to
        /// perform any manipulations with timed events but dedicated methods of the <see cref="TimedEventsManagingUtilities"/> will
        /// always be faster and will consume less memory.
        /// </remarks>
        /// <returns>Count of processed timed events.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="file"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="action"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <seealso cref="TimedObjectUtilities.ProcessObjects(MidiFile, ObjectType, Action{ITimedObject}, ObjectDetectionSettings, ObjectProcessingHint)"/>
        public static int ProcessTimedEvents(this MidiFile file, Action<TimedEvent> action, TimedEventDetectionSettings settings = null, TimedEventProcessingHint hint = TimedEventProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(file), file);
            ThrowIfArgument.IsNull(nameof(action), action);

            return file.ProcessTimedEvents(action, timedEvent => true, settings, hint);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="TimedEvent"/> contained in the <see cref="MidiFile"/>.
        /// More info in the <see href="xref:a_processing_objects#processtimedevents">Processing objects: ProcessTimedEvents</see> article.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for events to process.</param>
        /// <param name="action">The action to perform on each <see cref="TimedEvent"/> contained in the
        /// <paramref name="file"/>.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="TimedEvent"/> to process.</param>
        /// <param name="settings">Settings according to which timed events should be detected and built.</param>
        /// <param name="hint">Hint which tells the processing algorithm how it can optimize its performance.
        /// The default value is <see cref="TimedEventProcessingHint.Default"/>.</param>
        /// <remarks>
        /// Note that you can always use <see href="xref:a_managers">an object manager</see> to
        /// perform any manipulations with timed events but dedicated methods of the <see cref="TimedEventsManagingUtilities"/> will
        /// always be faster and will consume less memory.
        /// </remarks>
        /// <returns>Count of processed timed events.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
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
        /// <seealso cref="TimedObjectUtilities.ProcessObjects(MidiFile, ObjectType, Action{ITimedObject}, Predicate{ITimedObject}, ObjectDetectionSettings, ObjectProcessingHint)"/>
        public static int ProcessTimedEvents(this MidiFile file, Action<TimedEvent> action, Predicate<TimedEvent> match, TimedEventDetectionSettings settings = null, TimedEventProcessingHint hint = TimedEventProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(file), file);
            ThrowIfArgument.IsNull(nameof(action), action);
            ThrowIfArgument.IsNull(nameof(match), match);

            return file.GetTrackChunks().ProcessTimedEvents(action, match, settings, hint);
        }

        /// <summary>
        /// Removes all timed events from the <see cref="EventsCollection"/>.
        /// More info in the <see href="xref:a_removing_objects#removetimedevents">Removing objects: RemoveTimedEvents</see> article.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for events to remove.</param>
        /// <returns>Count of removed timed events.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is <c>null</c>.</exception>
        /// <seealso cref="TimedObjectUtilities.RemoveObjects(EventsCollection, ObjectType, ObjectDetectionSettings)"/>
        public static int RemoveTimedEvents(this EventsCollection eventsCollection)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);

            var result = eventsCollection.Count;
            eventsCollection.Clear();
            return result;
        }

        /// <summary>
        /// Removes timed events that match the specified conditions from the <see cref="EventsCollection"/>.
        /// More info in the <see href="xref:a_removing_objects#removetimedevents">Removing objects: RemoveTimedEvents</see> article.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for events to remove.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="TimedEvent"/> to remove.</param>
        /// <param name="settings">Settings according to which timed events should be detected and built.</param>
        /// <returns>Count of removed timed events.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="eventsCollection"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="match"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <seealso cref="TimedObjectUtilities.RemoveObjects(EventsCollection, ObjectType, Predicate{ITimedObject}, ObjectDetectionSettings)"/>
        public static int RemoveTimedEvents(this EventsCollection eventsCollection, Predicate<TimedEvent> match, TimedEventDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);
            ThrowIfArgument.IsNull(nameof(match), match);

            var eventsCount = eventsCollection.Count;

            var removedEventsCount = 0;
            var time = 0L;
            var latestTime = 0L;

            var constructor = settings?.Constructor;
            var useCustomConstructor = constructor != null;

            for (var i = 0; i < eventsCount; i++)
            {
                var midiEvent = eventsCollection.GetByIndexInternal(i);
                time += midiEvent.DeltaTime;

                var timedEvent = useCustomConstructor
                    ? constructor(new TimedEventData(midiEvent, time, 0, i))
                    : new TimedEvent(midiEvent, time);

                if (match(timedEvent))
                    removedEventsCount++;
                else
                {
                    midiEvent.DeltaTime = time - latestTime;
                    eventsCollection.SetByIndexInternal(i - removedEventsCount, midiEvent);
                    latestTime = time;
                }
            }

            if (removedEventsCount > 0)
                eventsCollection.RemoveRangeInternal(eventsCount - removedEventsCount, removedEventsCount);

            return removedEventsCount;
        }

        /// <summary>
        /// Removes all timed events from the <see cref="TrackChunk"/>.
        /// More info in the <see href="xref:a_removing_objects#removetimedevents">Removing objects: RemoveTimedEvents</see> article.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for events to remove.</param>
        /// <returns>Count of removed timed events.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is <c>null</c>.</exception>
        /// <seealso cref="TimedObjectUtilities.RemoveObjects(TrackChunk, ObjectType, ObjectDetectionSettings)"/>
        public static int RemoveTimedEvents(this TrackChunk trackChunk)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);

            var result = trackChunk.Events.Count;
            trackChunk.Events.Clear();
            return result;
        }

        /// <summary>
        /// Removes timed events that match the specified conditions from the <see cref="TrackChunk"/>.
        /// More info in the <see href="xref:a_removing_objects#removetimedevents">Removing objects: RemoveTimedEvents</see> article.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for events to remove.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="TimedEvent"/> to remove.</param>
        /// <param name="settings">Settings according to which timed events should be detected and built.</param>
        /// <returns>Count of removed timed events.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunk"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="match"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <seealso cref="TimedObjectUtilities.RemoveObjects(TrackChunk, ObjectType, Predicate{ITimedObject}, ObjectDetectionSettings)"/>
        public static int RemoveTimedEvents(this TrackChunk trackChunk, Predicate<TimedEvent> match, TimedEventDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(match), match);

            return trackChunk.Events.RemoveTimedEvents(match, settings);
        }

        /// <summary>
        /// Removes all timed events from the collection of <see cref="TrackChunk"/>.
        /// More info in the <see href="xref:a_removing_objects#removetimedevents">Removing objects: RemoveTimedEvents</see> article.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to search for events to remove.</param>
        /// <returns>Count of removed timed events.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunks"/> is <c>null</c>.</exception>
        /// <seealso cref="TimedObjectUtilities.RemoveObjects(IEnumerable{TrackChunk}, ObjectType, ObjectDetectionSettings)"/>
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
        /// Removes timed events that match the specified conditions from the collection of <see cref="TrackChunk"/>.
        /// More info in the <see href="xref:a_removing_objects#removetimedevents">Removing objects: RemoveTimedEvents</see> article.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to search for events to remove.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="TimedEvent"/> to remove.</param>
        /// <param name="settings">Settings according to which timed events should be detected and built.</param>
        /// <returns>Count of removed timed events.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunks"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="match"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <seealso cref="TimedObjectUtilities.RemoveObjects(IEnumerable{TrackChunk}, ObjectType, Predicate{ITimedObject}, ObjectDetectionSettings)"/>
        public static int RemoveTimedEvents(this IEnumerable<TrackChunk> trackChunks, Predicate<TimedEvent> match, TimedEventDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(match), match);

            var eventsCollections = trackChunks.Where(c => c != null).Select(c => c.Events).ToArray();
            var eventsCount = eventsCollections.Sum(c => c.Count);

            var eventsCollectionsCount = eventsCollections.Length;

            if (eventsCollectionsCount == 0)
                return 0;

            if (eventsCollectionsCount == 1)
                return eventsCollections[0].RemoveTimedEvents(match, settings);

            var eventsCollectionIndices = new int[eventsCollectionsCount];
            var eventsCollectionMaxIndices = eventsCollections.Select(c => c.Count - 1).ToArray();
            var eventsCollectionTimes = new long[eventsCollectionsCount];
            var eventsCollectionLatestTimes = new long[eventsCollectionsCount];
            var removedEventsCounts = new int[eventsCollectionsCount];

            var constructor = settings?.Constructor;
            var useCustomConstructor = constructor != null;

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

                var timedEvent = useCustomConstructor
                    ? constructor(new TimedEventData(midiEvent, minTime, eventsCollectionIndex, eventsCollectionIndices[eventsCollectionIndex]))
                    : new TimedEvent(midiEvent, minTime);
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
                    eventsCollections[i].RemoveRangeInternal(eventsCollections[i].Count - removedEventsCount, removedEventsCount);
            }

            return removedEventsCounts.Sum();
        }

        /// <summary>
        /// Removes all timed events from the <see cref="MidiFile"/>.
        /// More info in the <see href="xref:a_removing_objects#removetimedevents">Removing objects: RemoveTimedEvents</see> article.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for events to remove.</param>
        /// <returns>Count of removed timed events.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="file"/> is <c>null</c>.</exception>
        /// <seealso cref="TimedObjectUtilities.RemoveObjects(MidiFile, ObjectType, ObjectDetectionSettings)"/>
        public static int RemoveTimedEvents(this MidiFile file)
        {
            ThrowIfArgument.IsNull(nameof(file), file);

            return file.GetTrackChunks().RemoveTimedEvents();
        }

        /// <summary>
        /// Removes timed events that match the specified conditions from the <see cref="MidiFile"/>.
        /// More info in the <see href="xref:a_removing_objects#removetimedevents">Removing objects: RemoveTimedEvents</see> article.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for events to remove.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="TimedEvent"/> to remove.</param>
        /// <param name="settings">Settings according to which timed events should be detected and built.</param>
        /// <returns>Count of removed timed events.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="file"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="match"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <seealso cref="TimedObjectUtilities.RemoveObjects(MidiFile, ObjectType, Predicate{ITimedObject}, ObjectDetectionSettings)"/>
        public static int RemoveTimedEvents(this MidiFile file, Predicate<TimedEvent> match, TimedEventDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(file), file);
            ThrowIfArgument.IsNull(nameof(match), match);

            return file.GetTrackChunks().RemoveTimedEvents(match, settings);
        }

        internal static int ProcessTimedEventsInternal(
            this IEnumerable<EventsCollection> eventsCollectionsIn,
            Action<TimedEvent> action,
            Predicate<TimedEvent> match,
            TimedEventDetectionSettings settings,
            TimedEventProcessingHint hint)
        {
            var processedCount = 0;
            var eventsCollectionIndex = 0;

            var timeCanBeChanged = hint.HasFlag(TimedEventProcessingHint.TimeCanBeChanged);

            foreach (var eventsCollection in eventsCollectionsIn)
            {
                var eventsCount = eventsCollection.Count;
                var matchedCount = 0;

                var timesChanged = false;
                var timedEvents = eventsCollection.GetTimedEventsLazy(settings, eventsCollectionIndex, false).ToArray();

                foreach (var timedEvent in timedEvents)
                {
                    if (!match(timedEvent))
                        continue;

                    var deltaTime = timedEvent.Event.DeltaTime;
                    var time = timedEvent.Time;

                    action(timedEvent);
                    timedEvent.Event.DeltaTime = deltaTime;

                    timesChanged |= timedEvent.Time != time;

                    matchedCount++;
                }

                if (timeCanBeChanged && timesChanged)
                    eventsCollection.SortAndUpdateEvents(timedEvents);

                processedCount += matchedCount;
                eventsCollectionIndex++;
            }

            return processedCount;
        }

        internal static IEnumerable<TimedEvent> GetTimedEventsLazy(
            this IEnumerable<TrackChunk> trackChunks,
            TimedEventDetectionSettings settings,
            bool cloneEvent = true)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);

            var eventsCollections = trackChunks.Where(c => c != null).Select(c => c.Events).ToArray();
            return new SortedLazyCollection<TimedEvent>(eventsCollections.GetTimedEventsLazy(settings, cloneEvent));
        }

        internal static IEnumerable<TimedEvent> GetTimedEventsLazy(
            this EventsCollection[] eventsCollections,
            TimedEventDetectionSettings settings,
            bool cloneEvent = true)
        {
            return new SortedLazyCollection<TimedEvent>(GetSortedTimedEventsLazy(
                eventsCollections,
                settings,
                cloneEvent));
        }

        internal static IEnumerable<TimedEvent> GetTimedEventsLazy(
            this IEnumerable<MidiEvent> events,
            TimedEventDetectionSettings settings,
            int eventsCollectionIndex,
            bool cloneEvent = true)
        {
            return new SortedLazyCollection<TimedEvent>(GetSortedTimedEventsLazy(
                events,
                settings,
                eventsCollectionIndex,
                cloneEvent));
        }

        internal static void SortAndUpdateEvents(
            this EventsCollection eventsCollection,
            IEnumerable<TimedEvent> timedEvents)
        {
            var time = 0L;

            eventsCollection.Clear();

            foreach (var timedEvent in timedEvents.OrderBy(e => e.Time))
            {
                if (timedEvent.Event.MustBeRemoved)
                    continue;

                var midiEvent = timedEvent.Event;
                midiEvent.DeltaTime = timedEvent.Time - time;

                eventsCollection.AddInternal(midiEvent);
                time = timedEvent.Time;
            }
        }

        private static IEnumerable<TimedEvent> GetSortedTimedEventsLazy(
            this EventsCollection[] eventsCollections,
            TimedEventDetectionSettings settings,
            bool cloneEvent = true)
        {
            return eventsCollections
                .Select((eventsCollection, eventsCollectionIndex) => eventsCollection.GetSortedTimedEventsLazy(settings, eventsCollectionIndex, cloneEvent))
                .MergeSortedObjectsCollections();
        }

        private static IEnumerable<TimedEvent> GetSortedTimedEventsLazy(
            this IEnumerable<MidiEvent> events,
            TimedEventDetectionSettings settings,
            int eventsCollectionIndex,
            bool cloneEvent = true)
        {
            var constructor = settings?.Constructor;
            var useCustomConstructor = constructor != null;

            var time = 0L;
            var index = 0;

            foreach (var midiEvent in events)
            {
                if (midiEvent == null)
                    continue;

                time += midiEvent.DeltaTime;

                if (useCustomConstructor)
                {
                    yield return constructor(new TimedEventData(
                        cloneEvent ? midiEvent.Clone() : midiEvent,
                        time,
                        eventsCollectionIndex,
                        index));
                }
                else
                {
                    var timedEvent = new TimedEvent(cloneEvent ? midiEvent.Clone() : midiEvent);
                    timedEvent._time = time;
                    yield return timedEvent;
                }

                index++;
            }
        }

        #endregion
    }
}
