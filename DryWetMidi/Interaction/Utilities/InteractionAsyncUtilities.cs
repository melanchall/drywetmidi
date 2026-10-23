using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Interaction
{
    internal static class InteractionAsyncUtilities
    {
        internal static EventsCollection GetEvents(TrackChunk trackChunk)
        {
            AsyncOperation.Checkpoint();
            return trackChunk.Events;
        }

        internal static IEnumerable<T> CheckpointItems<T>(IEnumerable<T> source)
        {
            foreach (var item in source)
            {
                AsyncOperation.Checkpoint();
                yield return item;
            }
        }

        internal static void CollectEventsForRemoval(ITimedObject obj, ISet<MidiEvent> events)
        {
            switch (obj)
            {
                case TimedEvent timedEvent:
                    events.Add(timedEvent.Event);
                    break;
                case Note note:
                    events.Add(note.TimedNoteOnEvent.Event);
                    events.Add(note.TimedNoteOffEvent.Event);
                    break;
                case Chord chord:
                    foreach (var chordNote in chord.Notes)
                    {
                        AsyncOperation.Checkpoint();
                        CollectEventsForRemoval(chordNote, events);
                    }
                    break;
            }
        }

        internal static List<(MidiEvent Event, long DeltaTime)> PrepareReplacement(
            IEnumerable<TimedEvent> timedEvents,
            bool removeMarked = false,
            bool smfOnly = false)
        {
            var replacement = new List<(MidiEvent, long)>();
            var time = 0L;
            foreach (var timedEvent in timedEvents)
            {
                AsyncOperation.Checkpoint();
                var midiEvent = timedEvent.Event;
                if ((removeMarked && midiEvent.MustBeRemoved) ||
                    (smfOnly && !(midiEvent is ChannelEvent || midiEvent is MetaEvent || midiEvent is SysExEvent)))
                    continue;
                replacement.Add((midiEvent, timedEvent.Time - time));
                time = timedEvent.Time;
            }
            return replacement;
        }

        internal static void CommitReplacement(
            EventsCollection eventsCollection,
            IEnumerable<(MidiEvent Event, long DeltaTime)> replacement)
        {
            // All cancellation checks belong to preparation, before the destructive rewrite.
            eventsCollection.Clear();
            foreach (var item in replacement)
            {
                item.Event.DeltaTime = item.DeltaTime;
                eventsCollection.AddInternal(item.Event);
            }
        }

        internal static IEnumerable<EventsCollection> Collections(EventsCollection eventsCollection)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);
            return new[] { eventsCollection };
        }

        internal static IEnumerable<EventsCollection> Collections(TrackChunk trackChunk)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            return Collections(trackChunk.Events);
        }

        internal static IEnumerable<EventsCollection> Collections(IEnumerable<TrackChunk> trackChunks)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            return CheckpointItems(trackChunks).Where(t => t != null).Select(GetEvents);
        }

        internal static IEnumerable<EventsCollection> Collections(MidiFile file)
        {
            ThrowIfArgument.IsNull(nameof(file), file);
            return Collections(file.GetTrackChunks());
        }

        internal static async Task<List<T>> CollectAsync<T>(IAsyncEnumerable<T> source, CancellationToken cancellationToken)
        {
            ThrowIfArgument.IsNull(nameof(source), source);
            var result = new List<T>();
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                AsyncOperation.Checkpoint();
                result.Add(item);
            }
            return result;
        }

        internal static async IAsyncEnumerable<T> EnumerateAsync<T>(
            Func<IEnumerable<T>> source,
            IProgress<AsyncOperationProgress>? progress,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            using var ready = new SemaphoreSlim(0);
            using var consumed = new SemaphoreSlim(0);
            var completed = false;
            T current = default!;
            var producer = AsyncOperation.Run(async () =>
            {
                try
                {
                    foreach (var item in source())
                    {
                        AsyncOperation.Checkpoint();
                        current = item;
                        ready.Release();
                        await consumed.WaitAsync(lifetime.Token).ConfigureAwait(false);
                    }
                    return true;
                }
                finally
                {
                    completed = true;
                    ready.Release();
                }
            }, lifetime.Token, progress);
            try
            {
                while (true)
                {
                    await ready.WaitAsync(cancellationToken).ConfigureAwait(false);
                    if (completed)
                    {
                        await producer.ConfigureAwait(false);
                        yield break;
                    }
                    cancellationToken.ThrowIfCancellationRequested();
                    yield return current;
                    consumed.Release();
                }
            }
            finally
            {
                lifetime.Cancel();
                try { await producer.ConfigureAwait(false); }
                catch (OperationCanceledException) when (lifetime.IsCancellationRequested) { }
            }
        }
    }
}
