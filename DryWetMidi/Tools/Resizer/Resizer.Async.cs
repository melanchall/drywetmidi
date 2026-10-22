using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Tools
{
    public static partial class Resizer
    {
        /// <summary>Asynchronously resizes a track using the specified length.</summary>
        public static Task ResizeAsync(this TrackChunk trackChunk, ITimeSpan length, TempoMap tempoMap, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => Resize(trackChunk, length, tempoMap), cancellationToken, progress);

        /// <summary>Asynchronously resizes a track using the specified ratio.</summary>
        public static Task ResizeAsync(this TrackChunk trackChunk, double ratio, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => Resize(trackChunk, ratio), cancellationToken, progress);

        /// <summary>Asynchronously resizes tracks using the specified length.</summary>
        public static Task ResizeAsync(this IEnumerable<TrackChunk?> trackChunks, ITimeSpan length, TempoMap tempoMap, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => Resize(trackChunks, length, tempoMap), cancellationToken, progress);

        /// <summary>Asynchronously resizes tracks using the specified ratio.</summary>
        public static Task ResizeAsync(this IEnumerable<TrackChunk?> trackChunks, double ratio, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => Resize(trackChunks, ratio), cancellationToken, progress);

        /// <summary>Asynchronously resizes a MIDI file using the specified length.</summary>
        public static Task ResizeAsync(this MidiFile midiFile, ITimeSpan length, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => Resize(midiFile, length), cancellationToken, progress);

        /// <summary>Asynchronously resizes a MIDI file using the specified ratio.</summary>
        public static Task ResizeAsync(this MidiFile midiFile, double ratio, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => Resize(midiFile, ratio), cancellationToken, progress);

        /// <summary>Asynchronously resizes a group of objects using the specified length.</summary>
        public static Task ResizeObjectsGroupAsync(this IEnumerable<ITimedObject?> objects, ITimeSpan length, TempoMap tempoMap, ObjectsGroupResizingSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => ResizeObjectsGroup(objects, length, tempoMap, settings), cancellationToken, progress);

        /// <summary>Asynchronously resizes a group of objects using the specified ratio.</summary>
        public static Task ResizeObjectsGroupAsync(this IEnumerable<ITimedObject?> objects, double ratio, TempoMap tempoMap, ObjectsGroupResizingSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => ResizeObjectsGroup(objects, ratio, tempoMap, settings), cancellationToken, progress);
    }
}
