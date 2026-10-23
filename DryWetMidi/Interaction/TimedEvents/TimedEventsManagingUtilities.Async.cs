using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Interaction
{
    public static partial class TimedEventsManagingUtilities
    {
        /// <inheritdoc cref="GetTimedEvents(EventsCollection, TimedEventDetectionSettings)" />
        /// <summary>
        /// Asynchronously detects timed events in an events collection.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ICollection<TimedEvent>> GetTimedEventsAsync(
            this EventsCollection eventsCollection,
            TimedEventDetectionSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => eventsCollection.GetTimedEvents(settings), cancellationToken, progress);

        /// <inheritdoc cref="GetTimedEvents(TrackChunk, TimedEventDetectionSettings)" />
        /// <summary>
        /// Asynchronously detects timed events in a track.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ICollection<TimedEvent>> GetTimedEventsAsync(
            this TrackChunk trackChunk,
            TimedEventDetectionSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunk.GetTimedEvents(settings), cancellationToken, progress);

        /// <inheritdoc cref="GetTimedEvents(IEnumerable{TrackChunk}, TimedEventDetectionSettings)" />
        /// <summary>
        /// Asynchronously detects timed events in tracks.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ICollection<TimedEvent>> GetTimedEventsAsync(
            this IEnumerable<TrackChunk> trackChunks,
            TimedEventDetectionSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunks.GetTimedEvents(settings), cancellationToken, progress);

        /// <inheritdoc cref="GetTimedEvents(MidiFile, TimedEventDetectionSettings)" />
        /// <summary>
        /// Asynchronously detects timed events in a MIDI file.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ICollection<TimedEvent>> GetTimedEventsAsync(
            this MidiFile file,
            TimedEventDetectionSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => file.GetTimedEvents(settings), cancellationToken, progress);

        /// <summary>
        /// Asynchronously detects timed events from an asynchronous MIDI event source.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="midiEvents"/> is null.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ICollection<TimedEvent>> GetTimedEventsAsync(
            this IAsyncEnumerable<MidiEvent> midiEvents,
            TimedEventDetectionSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(async () => (ICollection<TimedEvent>)(await InteractionAsyncUtilities.CollectAsync(midiEvents, cancellationToken).ConfigureAwait(false)).GetTimedEventsLazy(settings, 0).ToArray(), cancellationToken, progress);

        /// <inheritdoc cref="GetTimedEvents(IEnumerable{TrackChunk}, TimedEventDetectionSettings)" />
        /// <summary>
        /// Asynchronously detects timed events from an asynchronous track source.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ICollection<TimedEvent>> GetTimedEventsAsync(
            this IAsyncEnumerable<TrackChunk> trackChunks,
            TimedEventDetectionSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(async () => (await InteractionAsyncUtilities.CollectAsync(trackChunks, cancellationToken).ConfigureAwait(false)).GetTimedEvents(settings), cancellationToken, progress);

        /// <inheritdoc cref="ManageTimedEvents(EventsCollection, TimedEventDetectionSettings, TimedObjectsComparer)" />
        /// <summary>
        /// Asynchronously creates a timed events manager.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<TimedObjectsManager<TimedEvent>> ManageTimedEventsAsync(
            this EventsCollection eventsCollection,
            TimedEventDetectionSettings? settings = null,
            TimedObjectsComparer? comparer = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => eventsCollection.ManageTimedEvents(settings, comparer), cancellationToken, progress);

        /// <inheritdoc cref="ManageTimedEvents(TrackChunk, TimedEventDetectionSettings, TimedObjectsComparer)" />
        /// <summary>
        /// Asynchronously creates a timed events manager for a track.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<TimedObjectsManager<TimedEvent>> ManageTimedEventsAsync(
            this TrackChunk trackChunk,
            TimedEventDetectionSettings? settings = null,
            TimedObjectsComparer? comparer = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunk.ManageTimedEvents(settings, comparer), cancellationToken, progress);

        /// <inheritdoc cref="ProcessTimedEvents(EventsCollection, Action{TimedEvent}, TimedEventDetectionSettings, TimedEventProcessingHint)" />
        /// <summary>
        /// Asynchronously processes timed events.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessTimedEventsAsync(
            this EventsCollection eventsCollection,
            Action<TimedEvent> action,
            TimedEventDetectionSettings? settings = null,
            TimedEventProcessingHint hint = TimedEventProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => eventsCollection.ProcessTimedEvents(action, settings, hint), cancellationToken, progress);

        /// <inheritdoc cref="ProcessTimedEvents(EventsCollection, Action{TimedEvent}, Predicate{TimedEvent}, TimedEventDetectionSettings, TimedEventProcessingHint)" />
        /// <summary>
        /// Asynchronously processes matching timed events.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessTimedEventsAsync(
            this EventsCollection eventsCollection,
            Action<TimedEvent> action,
            Predicate<TimedEvent> match,
            TimedEventDetectionSettings? settings = null,
            TimedEventProcessingHint hint = TimedEventProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => eventsCollection.ProcessTimedEvents(action, match, settings, hint), cancellationToken, progress);

        /// <inheritdoc cref="ProcessTimedEvents(TrackChunk, Action{TimedEvent}, TimedEventDetectionSettings, TimedEventProcessingHint)" />
        /// <summary>
        /// Asynchronously processes timed events in a track.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessTimedEventsAsync(
            this TrackChunk trackChunk,
            Action<TimedEvent> action,
            TimedEventDetectionSettings? settings = null,
            TimedEventProcessingHint hint = TimedEventProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunk.ProcessTimedEvents(action, settings, hint), cancellationToken, progress);

        /// <inheritdoc cref="ProcessTimedEvents(TrackChunk, Action{TimedEvent}, Predicate{TimedEvent}, TimedEventDetectionSettings, TimedEventProcessingHint)" />
        /// <summary>
        /// Asynchronously processes matching timed events in a track.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessTimedEventsAsync(
            this TrackChunk trackChunk,
            Action<TimedEvent> action,
            Predicate<TimedEvent> match,
            TimedEventDetectionSettings? settings = null,
            TimedEventProcessingHint hint = TimedEventProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunk.ProcessTimedEvents(action, match, settings, hint), cancellationToken, progress);

        /// <inheritdoc cref="ProcessTimedEvents(IEnumerable{TrackChunk}, Action{TimedEvent}, TimedEventDetectionSettings, TimedEventProcessingHint)" />
        /// <summary>
        /// Asynchronously processes timed events in tracks.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessTimedEventsAsync(
            this IEnumerable<TrackChunk> trackChunks,
            Action<TimedEvent> action,
            TimedEventDetectionSettings? settings = null,
            TimedEventProcessingHint hint = TimedEventProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunks.ProcessTimedEvents(action, settings, hint), cancellationToken, progress);

        /// <inheritdoc cref="ProcessTimedEvents(IEnumerable{TrackChunk}, Action{TimedEvent}, Predicate{TimedEvent}, TimedEventDetectionSettings, TimedEventProcessingHint)" />
        /// <summary>
        /// Asynchronously processes matching timed events in tracks.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessTimedEventsAsync(
            this IEnumerable<TrackChunk> trackChunks,
            Action<TimedEvent> action,
            Predicate<TimedEvent> match,
            TimedEventDetectionSettings? settings = null,
            TimedEventProcessingHint hint = TimedEventProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunks.ProcessTimedEvents(action, match, settings, hint), cancellationToken, progress);

        /// <inheritdoc cref="ProcessTimedEvents(MidiFile, Action{TimedEvent}, TimedEventDetectionSettings, TimedEventProcessingHint)" />
        /// <summary>
        /// Asynchronously processes timed events in a MIDI file.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessTimedEventsAsync(
            this MidiFile file,
            Action<TimedEvent> action,
            TimedEventDetectionSettings? settings = null,
            TimedEventProcessingHint hint = TimedEventProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => file.ProcessTimedEvents(action, settings, hint), cancellationToken, progress);

        /// <inheritdoc cref="ProcessTimedEvents(MidiFile, Action{TimedEvent}, Predicate{TimedEvent}, TimedEventDetectionSettings, TimedEventProcessingHint)" />
        /// <summary>
        /// Asynchronously processes matching timed events in a MIDI file.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessTimedEventsAsync(
            this MidiFile file,
            Action<TimedEvent> action,
            Predicate<TimedEvent> match,
            TimedEventDetectionSettings? settings = null,
            TimedEventProcessingHint hint = TimedEventProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => file.ProcessTimedEvents(action, match, settings, hint), cancellationToken, progress);

        /// <inheritdoc cref="ProcessTimedEvents(EventsCollection, Action{TimedEvent}, TimedEventDetectionSettings, TimedEventProcessingHint)" />
        /// <summary>
        /// Sequentially awaits an asynchronous action for each timed event.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessTimedEventsAsync(
            this EventsCollection eventsCollection,
            Func<TimedEvent, CancellationToken, ValueTask> action,
            TimedEventDetectionSettings? settings = null,
            TimedEventProcessingHint hint = TimedEventProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            eventsCollection.ProcessTimedEventsAsync(action, null, settings, hint, progress, cancellationToken);

        /// <inheritdoc cref="ProcessTimedEvents(TrackChunk, Action{TimedEvent}, TimedEventDetectionSettings, TimedEventProcessingHint)" />
        /// <summary>
        /// Sequentially awaits an asynchronous action for each timed event in a track.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessTimedEventsAsync(
            this TrackChunk trackChunk,
            Func<TimedEvent, CancellationToken, ValueTask> action,
            TimedEventDetectionSettings? settings = null,
            TimedEventProcessingHint hint = TimedEventProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            trackChunk.ProcessTimedEventsAsync(action, null, settings, hint, progress, cancellationToken);

        /// <inheritdoc cref="ProcessTimedEvents(IEnumerable{TrackChunk}, Action{TimedEvent}, TimedEventDetectionSettings, TimedEventProcessingHint)" />
        /// <summary>
        /// Sequentially awaits an asynchronous action for each timed event in tracks.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessTimedEventsAsync(
            this IEnumerable<TrackChunk> trackChunks,
            Func<TimedEvent, CancellationToken, ValueTask> action,
            TimedEventDetectionSettings? settings = null,
            TimedEventProcessingHint hint = TimedEventProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            trackChunks.ProcessTimedEventsAsync(action, null, settings, hint, progress, cancellationToken);

        /// <inheritdoc cref="ProcessTimedEvents(MidiFile, Action{TimedEvent}, TimedEventDetectionSettings, TimedEventProcessingHint)" />
        /// <summary>
        /// Sequentially awaits an asynchronous action for each timed event in a MIDI file.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessTimedEventsAsync(
            this MidiFile file,
            Func<TimedEvent, CancellationToken, ValueTask> action,
            TimedEventDetectionSettings? settings = null,
            TimedEventProcessingHint hint = TimedEventProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            file.ProcessTimedEventsAsync(action, null, settings, hint, progress, cancellationToken);

        /// <inheritdoc cref="ProcessTimedEvents(EventsCollection, Action{TimedEvent}, TimedEventDetectionSettings, TimedEventProcessingHint)" />
        /// <summary>
        /// Sequentially awaits an asynchronous action for matching timed events.
        /// A null match processes all events.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessTimedEventsAsync(
            this EventsCollection eventsCollection,
            Func<TimedEvent, CancellationToken, ValueTask> action,
            Predicate<TimedEvent>? match,
            TimedEventDetectionSettings? settings = null,
            TimedEventProcessingHint hint = TimedEventProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => ProcessTimedEventsAwaitedAsync(InteractionAsyncUtilities.Collections(eventsCollection), action, match, settings, hint, cancellationToken), cancellationToken, progress);

        /// <inheritdoc cref="ProcessTimedEvents(TrackChunk, Action{TimedEvent}, TimedEventDetectionSettings, TimedEventProcessingHint)" />
        /// <summary>
        /// Sequentially awaits an asynchronous action for matching timed events in a track.
        /// A null match processes all events.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessTimedEventsAsync(
            this TrackChunk trackChunk,
            Func<TimedEvent, CancellationToken, ValueTask> action,
            Predicate<TimedEvent>? match,
            TimedEventDetectionSettings? settings = null,
            TimedEventProcessingHint hint = TimedEventProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => ProcessTimedEventsAwaitedAsync(InteractionAsyncUtilities.Collections(trackChunk), action, match, settings, hint, cancellationToken), cancellationToken, progress);

        /// <inheritdoc cref="ProcessTimedEvents(IEnumerable{TrackChunk}, Action{TimedEvent}, TimedEventDetectionSettings, TimedEventProcessingHint)" />
        /// <summary>
        /// Sequentially awaits an asynchronous action for matching timed events in tracks.
        /// A null match processes all events.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessTimedEventsAsync(
            this IEnumerable<TrackChunk> trackChunks,
            Func<TimedEvent, CancellationToken, ValueTask> action,
            Predicate<TimedEvent>? match,
            TimedEventDetectionSettings? settings = null,
            TimedEventProcessingHint hint = TimedEventProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => ProcessTimedEventsAwaitedAsync(InteractionAsyncUtilities.Collections(trackChunks), action, match, settings, hint, cancellationToken), cancellationToken, progress);

        /// <inheritdoc cref="ProcessTimedEvents(MidiFile, Action{TimedEvent}, TimedEventDetectionSettings, TimedEventProcessingHint)" />
        /// <summary>
        /// Sequentially awaits an asynchronous action for matching timed events in a MIDI file.
        /// A null match processes all events.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessTimedEventsAsync(
            this MidiFile file,
            Func<TimedEvent, CancellationToken, ValueTask> action,
            Predicate<TimedEvent>? match,
            TimedEventDetectionSettings? settings = null,
            TimedEventProcessingHint hint = TimedEventProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => ProcessTimedEventsAwaitedAsync(InteractionAsyncUtilities.Collections(file), action, match, settings, hint, cancellationToken), cancellationToken, progress);

        /// <inheritdoc cref="RemoveTimedEvents(EventsCollection)" />
        /// <summary>
        /// Asynchronously removes all timed events.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> RemoveTimedEventsAsync(
            this EventsCollection eventsCollection,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => eventsCollection.RemoveTimedEvents(), cancellationToken, progress);

        /// <inheritdoc cref="RemoveTimedEvents(EventsCollection, Predicate{TimedEvent}, TimedEventDetectionSettings)" />
        /// <summary>
        /// Asynchronously removes matching timed events.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> RemoveTimedEventsAsync(
            this EventsCollection eventsCollection,
            Predicate<TimedEvent> match,
            TimedEventDetectionSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => eventsCollection.RemoveTimedEvents(match, settings), cancellationToken, progress);

        /// <inheritdoc cref="RemoveTimedEvents(TrackChunk)" />
        /// <summary>
        /// Asynchronously removes all timed events from a track.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> RemoveTimedEventsAsync(
            this TrackChunk trackChunk,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunk.RemoveTimedEvents(), cancellationToken, progress);

        /// <inheritdoc cref="RemoveTimedEvents(TrackChunk, Predicate{TimedEvent}, TimedEventDetectionSettings)" />
        /// <summary>
        /// Asynchronously removes matching timed events from a track.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> RemoveTimedEventsAsync(
            this TrackChunk trackChunk,
            Predicate<TimedEvent> match,
            TimedEventDetectionSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunk.RemoveTimedEvents(match, settings), cancellationToken, progress);

        /// <inheritdoc cref="RemoveTimedEvents(IEnumerable{TrackChunk})" />
        /// <summary>
        /// Asynchronously removes all timed events from tracks.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> RemoveTimedEventsAsync(
            this IEnumerable<TrackChunk> trackChunks,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunks.RemoveTimedEvents(), cancellationToken, progress);

        /// <inheritdoc cref="RemoveTimedEvents(IEnumerable{TrackChunk}, Predicate{TimedEvent}, TimedEventDetectionSettings)" />
        /// <summary>
        /// Asynchronously removes matching timed events from tracks.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> RemoveTimedEventsAsync(
            this IEnumerable<TrackChunk> trackChunks,
            Predicate<TimedEvent> match,
            TimedEventDetectionSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunks.RemoveTimedEvents(match, settings), cancellationToken, progress);

        /// <inheritdoc cref="RemoveTimedEvents(MidiFile)" />
        /// <summary>
        /// Asynchronously removes all timed events from a MIDI file.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> RemoveTimedEventsAsync(
            this MidiFile file,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => file.RemoveTimedEvents(), cancellationToken, progress);

        /// <inheritdoc cref="RemoveTimedEvents(MidiFile, Predicate{TimedEvent}, TimedEventDetectionSettings)" />
        /// <summary>
        /// Asynchronously removes matching timed events from a MIDI file.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> RemoveTimedEventsAsync(
            this MidiFile file,
            Predicate<TimedEvent> match,
            TimedEventDetectionSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => file.RemoveTimedEvents(match, settings), cancellationToken, progress);

        private static async Task<int> ProcessTimedEventsAwaitedAsync(
            IEnumerable<EventsCollection> eventsCollections,
            Func<TimedEvent, CancellationToken, ValueTask> action,
            Predicate<TimedEvent>? match,
            TimedEventDetectionSettings? settings,
            TimedEventProcessingHint hint,
            CancellationToken cancellationToken)
        {
            ThrowIfArgument.IsNull(nameof(action), action);

            var count = 0;
            var index = 0;

            foreach (var eventsCollection in eventsCollections)
            {
                AsyncOperation.Checkpoint();
                var timedEvents = eventsCollection.GetTimedEventsLazy(settings, index++, false).ToArray();
                var changed = false;

                foreach (var timedEvent in timedEvents)
                {
                    AsyncOperation.Checkpoint();
                    if (match != null && !match(timedEvent))
                        continue;

                    var time = timedEvent.Time;
                    var deltaTime = timedEvent.Event.DeltaTime;
                    await action(timedEvent, cancellationToken).ConfigureAwait(false);
                    timedEvent.Event.DeltaTime = deltaTime;
                    cancellationToken.ThrowIfCancellationRequested();
                    changed |= time != timedEvent.Time;
                    count++;
                }

                if (changed && hint.HasFlag(TimedEventProcessingHint.TimeCanBeChanged))
                    eventsCollection.SortAndUpdateEvents(timedEvents);
            }

            return count;
        }
    }
}
