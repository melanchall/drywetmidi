using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class InteractionAsyncTests
    {
        private sealed class ImmediateProgress : IProgress<AsyncOperationProgress>
        {
            internal readonly List<AsyncOperationProgress> Reports = new List<AsyncOperationProgress>();

            public void Report(AsyncOperationProgress value) => Reports.Add(value);
        }

        private sealed class CancelingComparer : TimedObjectsComparer
        {
            internal Action OnCompare;

            public override int Compare(ITimedObject x, ITimedObject y)
            {
                OnCompare?.Invoke();
                return base.Compare(x, y);
            }
        }

        private static TrackChunk CreateTrack()
        {
            var track = new TrackChunk();
            track.Events.AddObjects(new ITimedObject[]
            {
                new Note((SevenBitNumber)60, 100, 0),
                new Note((SevenBitNumber)64, 100, 0),
                new TimedEvent(new TextEvent("marker"), 150),
                new Note((SevenBitNumber)60, 50, 200)
            });
            return track;
        }

        internal static MidiEvent[] CloneEvents(IEnumerable<MidiEvent> events) =>
            events.Select(e => e?.Clone()).ToArray();

        internal static ICollection<MidiEvent>[] CloneEvents(IEnumerable<IEnumerable<MidiEvent>> events) =>
            events.Select(collection => (ICollection<MidiEvent>)CloneEvents(collection)).ToArray();

        internal static async IAsyncEnumerable<T> AsAsync<T>(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                await Task.Yield();
                yield return item;
            }
        }

        internal static async Task<List<T>> CollectAsync<T>(IAsyncEnumerable<T> source)
        {
            var result = new List<T>();

            await foreach (var item in source)
            {
                result.Add(item);
            }

            return result;
        }

        private static Task<int> ProcessSynchronouslyAsync(
            IEnumerable<TrackChunk> tracks,
            string kind,
            Action<ITimedObject> action,
            CancellationToken cancellationToken)
        {
            switch (kind)
            {
                case "Notes":
                    return tracks.ProcessNotesAsync(action, cancellationToken: cancellationToken);
                case "Chords":
                    return tracks.ProcessChordsAsync(action, cancellationToken: cancellationToken);
                case "TimedEvents":
                    return tracks.ProcessTimedEventsAsync(action, cancellationToken: cancellationToken);
                default:
                    return tracks.ProcessObjectsAsync(ObjectType.Note | ObjectType.Chord | ObjectType.TimedEvent, action, cancellationToken: cancellationToken);
            }
        }

        private static Task<int> ProcessAwaitedAsync(
            IEnumerable<TrackChunk> tracks,
            string kind,
            Func<ITimedObject, CancellationToken, ValueTask> action)
        {
            switch (kind)
            {
                case "Notes":
                    return tracks.ProcessNotesAsync(action);
                case "Chords":
                    return tracks.ProcessChordsAsync(action);
                case "TimedEvents":
                    return tracks.ProcessTimedEventsAsync(action);
                default:
                    return tracks.ProcessObjectsAsync(ObjectType.Note | ObjectType.Chord | ObjectType.TimedEvent, action);
            }
        }
    }
}
