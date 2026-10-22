using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Tools
{
    public static partial class CsvSerializer
    {
        /// <summary>Serializes a MIDI file using asynchronous stream writes and flush. The stream remains open.</summary>
        public static Task SerializeToCsvAsync(this MidiFile midiFile, Stream stream, CsvSerializationSettings? settings = null, ObjectType objectType = ObjectType.TimedEvent, ObjectDetectionSettings? objectDetectionSettings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => CsvAsyncBuffer.SerializeAsync(stream, buffer => SerializeToCsv(midiFile, buffer, settings, objectType, objectDetectionSettings), progress, cancellationToken);

        /// <summary>Serializes a MIDI file to a CSV file asynchronously.</summary>
        public static async Task SerializeToCsvAsync(this MidiFile midiFile, string filePath, bool overwriteFile, CsvSerializationSettings? settings = null, ObjectType objectType = ObjectType.TimedEvent, ObjectDetectionSettings? objectDetectionSettings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var stream = OpenCsvForWrite(filePath, overwriteFile);
            await SerializeToCsvAsync(midiFile, stream, settings, objectType, objectDetectionSettings, progress, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Serializes MIDI chunks using asynchronous stream writes and flush. The stream remains open.</summary>
        public static Task SerializeToCsvAsync(this IEnumerable<MidiChunk?> midiChunks, Stream stream, TempoMap tempoMap, CsvSerializationSettings? settings = null, ObjectType objectType = ObjectType.TimedEvent, ObjectDetectionSettings? objectDetectionSettings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => CsvAsyncBuffer.SerializeAsync(stream, buffer => SerializeToCsv(midiChunks, buffer, tempoMap, settings, objectType, objectDetectionSettings), progress, cancellationToken);

        /// <summary>Serializes MIDI chunks to a CSV file asynchronously.</summary>
        public static async Task SerializeToCsvAsync(this IEnumerable<MidiChunk?> midiChunks, string filePath, bool overwriteFile, TempoMap tempoMap, CsvSerializationSettings? settings = null, ObjectType objectType = ObjectType.TimedEvent, ObjectDetectionSettings? objectDetectionSettings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var stream = OpenCsvForWrite(filePath, overwriteFile);
            await SerializeToCsvAsync(midiChunks, stream, tempoMap, settings, objectType, objectDetectionSettings, progress, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Serializes a MIDI chunk using asynchronous stream writes and flush. The stream remains open.</summary>
        public static Task SerializeToCsvAsync(this MidiChunk midiChunk, Stream stream, TempoMap tempoMap, CsvSerializationSettings? settings = null, ObjectType objectType = ObjectType.TimedEvent, ObjectDetectionSettings? objectDetectionSettings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => CsvAsyncBuffer.SerializeAsync(stream, buffer => SerializeToCsv(midiChunk, buffer, tempoMap, settings, objectType, objectDetectionSettings), progress, cancellationToken);

        /// <summary>Serializes a MIDI chunk to a CSV file asynchronously.</summary>
        public static async Task SerializeToCsvAsync(this MidiChunk midiChunk, string filePath, bool overwriteFile, TempoMap tempoMap, CsvSerializationSettings? settings = null, ObjectType objectType = ObjectType.TimedEvent, ObjectDetectionSettings? objectDetectionSettings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var stream = OpenCsvForWrite(filePath, overwriteFile);
            await SerializeToCsvAsync(midiChunk, stream, tempoMap, settings, objectType, objectDetectionSettings, progress, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Serializes objects using asynchronous stream writes and flush. The stream remains open.</summary>
        public static Task SerializeToCsvAsync(this IEnumerable<ITimedObject?> timedObjects, Stream stream, TempoMap tempoMap, CsvSerializationSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => CsvAsyncBuffer.SerializeAsync(stream, buffer => SerializeToCsv(timedObjects, buffer, tempoMap, settings), progress, cancellationToken);

        /// <summary>Serializes objects to a CSV file asynchronously.</summary>
        public static async Task SerializeToCsvAsync(this IEnumerable<ITimedObject?> timedObjects, string filePath, bool overwriteFile, TempoMap tempoMap, CsvSerializationSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var stream = OpenCsvForWrite(filePath, overwriteFile);
            await SerializeToCsvAsync(timedObjects, stream, tempoMap, settings, progress, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Deserializes a MIDI file using asynchronous stream reads. The stream remains open.</summary>
        public static Task<MidiFile> DeserializeFileFromCsvAsync(Stream stream, CsvDeserializationSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => CsvAsyncBuffer.DeserializeAsync(stream, buffer => DeserializeFileFromCsv(buffer, settings), progress, cancellationToken);

        /// <summary>Deserializes a MIDI file from CSV asynchronously.</summary>
        public static async Task<MidiFile> DeserializeFileFromCsvAsync(string filePath, CsvDeserializationSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var stream = OpenCsvForRead(filePath);
            return await DeserializeFileFromCsvAsync(stream, settings, progress, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Deserializes MIDI chunks using asynchronous stream reads. The stream remains open.</summary>
        public static Task<ICollection<MidiChunk>> DeserializeChunksFromCsvAsync(Stream stream, TempoMap tempoMap, CsvDeserializationSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => CsvAsyncBuffer.DeserializeAsync(stream, buffer => DeserializeChunksFromCsv(buffer, tempoMap, settings), progress, cancellationToken);

        /// <summary>Deserializes MIDI chunks from CSV asynchronously.</summary>
        public static async Task<ICollection<MidiChunk>> DeserializeChunksFromCsvAsync(string filePath, TempoMap tempoMap, CsvDeserializationSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var stream = OpenCsvForRead(filePath);
            return await DeserializeChunksFromCsvAsync(stream, tempoMap, settings, progress, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Deserializes a MIDI chunk using asynchronous stream reads. The stream remains open.</summary>
        public static Task<MidiChunk> DeserializeChunkFromCsvAsync(Stream stream, TempoMap tempoMap, CsvDeserializationSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => CsvAsyncBuffer.DeserializeAsync(stream, buffer => DeserializeChunkFromCsv(buffer, tempoMap, settings), progress, cancellationToken);

        /// <summary>Deserializes a MIDI chunk from CSV asynchronously.</summary>
        public static async Task<MidiChunk> DeserializeChunkFromCsvAsync(string filePath, TempoMap tempoMap, CsvDeserializationSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var stream = OpenCsvForRead(filePath);
            return await DeserializeChunkFromCsvAsync(stream, tempoMap, settings, progress, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Deserializes objects using asynchronous stream reads. The stream remains open.</summary>
        public static Task<ICollection<ITimedObject>> DeserializeObjectsFromCsvAsync(Stream stream, TempoMap tempoMap, CsvDeserializationSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => CsvAsyncBuffer.DeserializeAsync(stream, buffer => DeserializeObjectsFromCsv(buffer, tempoMap, settings), progress, cancellationToken);

        /// <summary>Deserializes objects from CSV asynchronously.</summary>
        public static async Task<ICollection<ITimedObject>> DeserializeObjectsFromCsvAsync(string filePath, TempoMap tempoMap, CsvDeserializationSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var stream = OpenCsvForRead(filePath);
            return await DeserializeObjectsFromCsvAsync(stream, tempoMap, settings, progress, cancellationToken).ConfigureAwait(false);
        }

        private static FileStream OpenCsvForRead(string filePath)
        {
            ThrowIfArgument.IsNullOrEmptyString(nameof(filePath), filePath, "File path");
            return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
        }

        private static FileStream OpenCsvForWrite(string filePath, bool overwriteFile)
        {
            ThrowIfArgument.IsNullOrEmptyString(nameof(filePath), filePath, "File path");
            return new FileStream(filePath, overwriteFile ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
        }
    }
}
