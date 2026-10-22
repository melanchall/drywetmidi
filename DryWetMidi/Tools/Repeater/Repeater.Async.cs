using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Tools
{
    public partial class Repeater
    {
        /// <summary>Asynchronously repeats a MIDI file, preserving custom processing hooks.</summary>
        public Task<MidiFile> RepeatAsync(MidiFile midiFile, int repeatsNumber, RepeatingSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => Repeat(midiFile, repeatsNumber, settings), cancellationToken, progress);

        /// <summary>Asynchronously repeats tracks, preserving custom processing hooks.</summary>
        public Task<ICollection<TrackChunk>> RepeatAsync(IEnumerable<TrackChunk?> trackChunks, int repeatsNumber, TempoMap tempoMap, RepeatingSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => Repeat(trackChunks, repeatsNumber, tempoMap, settings), cancellationToken, progress);

        /// <summary>Asynchronously repeats a track, preserving custom processing hooks.</summary>
        public Task<TrackChunk> RepeatAsync(TrackChunk trackChunk, int repeatsNumber, TempoMap tempoMap, RepeatingSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => Repeat(trackChunk, repeatsNumber, tempoMap, settings), cancellationToken, progress);

        /// <summary>Asynchronously repeats objects, preserving custom processing hooks.</summary>
        public Task<ICollection<ITimedObject>> RepeatAsync(IEnumerable<ITimedObject?> timedObjects, int repeatsNumber, TempoMap tempoMap, RepeatingSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => Repeat(timedObjects, repeatsNumber, tempoMap, settings), cancellationToken, progress);
    }

    public static partial class RepeaterUtilities
    {
        /// <summary>Asynchronously repeats a MIDI file.</summary>
        public static Task<MidiFile> RepeatAsync(this MidiFile midiFile, int repeatsNumber, RepeatingSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => new Repeater().RepeatAsync(midiFile, repeatsNumber, settings, progress, cancellationToken);

        /// <summary>Asynchronously repeats a track.</summary>
        public static Task<TrackChunk> RepeatAsync(this TrackChunk trackChunk, int repeatsNumber, TempoMap tempoMap, RepeatingSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => new Repeater().RepeatAsync(trackChunk, repeatsNumber, tempoMap, settings, progress, cancellationToken);

        /// <summary>Asynchronously repeats tracks.</summary>
        public static Task<ICollection<TrackChunk>> RepeatAsync(this IEnumerable<TrackChunk?> trackChunks, int repeatsNumber, TempoMap tempoMap, RepeatingSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => new Repeater().RepeatAsync(trackChunks, repeatsNumber, tempoMap, settings, progress, cancellationToken);

        /// <summary>Asynchronously repeats timed objects.</summary>
        public static Task<ICollection<ITimedObject>> RepeatAsync(this IEnumerable<ITimedObject?> timedObjects, int repeatsNumber, TempoMap tempoMap, RepeatingSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => new Repeater().RepeatAsync(timedObjects, repeatsNumber, tempoMap, settings, progress, cancellationToken);
    }
}
