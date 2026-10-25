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
        internal sealed class RemovalScope : IDisposable
        {
            private static readonly AsyncLocal<RemovalScope?> Current = new AsyncLocal<RemovalScope?>();

            private readonly RemovalScope? _previous;
            private readonly List<MidiEvent> _markedEvents = new List<MidiEvent>();
            private readonly HashSet<MidiEvent>? _selectedEvents;

            public void Dispose()
            {
                // Cleanup must finish even after cancellation or a callback failure.
                foreach (var midiEvent in _markedEvents)
                {
                    midiEvent.MustBeRemoved = false;
                }

                Current.Value = _previous;
            }

            internal RemovalScope()
            {
                _previous = Current.Value;
                _selectedEvents = _previous != null ? new HashSet<MidiEvent>() : null;
                Current.Value = this;
            }

            internal bool IsMarked(TimedEvent timedEvent)
            {
                return _selectedEvents != null
                    ? _selectedEvents.Contains(timedEvent.Event)
                    : timedEvent.Event.MustBeRemoved;
            }

            internal void Mark(ITimedObject obj)
            {
                switch (obj)
                {
                    case TimedEvent timedEvent:
                        MarkEvent(timedEvent.Event);
                        break;
                    case Note note:
                        MarkEvent(note.TimedNoteOnEvent.Event);
                        MarkEvent(note.TimedNoteOffEvent.Event);
                        break;
                    case Chord chord:

                        foreach (var chordNote in chord.Notes)
                        {
                            AsyncOperation.Checkpoint();
                            Mark(chordNote);
                        }

                        break;
                }
            }

            private void MarkEvent(MidiEvent midiEvent)
            {
                _selectedEvents?.Add(midiEvent);

                if (midiEvent.MustBeRemoved)
                    return;

                _markedEvents.Add(midiEvent);
                midiEvent.MustBeRemoved = true;
            }
        }

        internal static EventsCollection GetEvents(TrackChunk trackChunk)
        {
            AsyncOperation.Checkpoint();
            return trackChunk.Events;
        }

        internal static IEnumerable<T> CheckpointItems<T>(IEnumerable<T> source)
        {
            return AsyncOperation.IsActive ? EnumerateWithCheckpoints(source) : source;
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
            return new[] { eventsCollection };
        }

        internal static IEnumerable<EventsCollection> Collections(TrackChunk trackChunk)
        {
            return Collections(trackChunk.Events);
        }

        internal static IEnumerable<EventsCollection> Collections(IEnumerable<TrackChunk> trackChunks)
        {
            return CheckpointItems(trackChunks).Where(t => t != null).Select(GetEvents);
        }

        internal static IEnumerable<EventsCollection> Collections(MidiFile file)
        {
            return Collections(file.GetTrackChunks());
        }

        internal static async Task<List<T>> CollectAsync<T>(
            IAsyncEnumerable<T> source,
            CancellationToken cancellationToken)
        {
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

                try
                {
                    await producer.ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (lifetime.IsCancellationRequested)
                {
                }
            }
        }

        private static IEnumerable<T> EnumerateWithCheckpoints<T>(IEnumerable<T> source)
        {
            foreach (var item in source)
            {
                AsyncOperation.Checkpoint();
                yield return item;
            }
        }
    }
}
