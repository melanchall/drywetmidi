using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Tools
{
    public static partial class CsvSerializer
    {
        /// <summary>
        /// Serializes a MIDI file using asynchronous stream writes and flush. The stream remains open.
        /// </summary>
        /// <inheritdoc cref="SerializeToCsv(MidiFile, Stream, CsvSerializationSettings, ObjectType, ObjectDetectionSettings)" path="/member/exception"/>
        /// <param name="midiFile">MIDI file to serialize.</param>
        /// <param name="stream">Writable stream, which remains open.</param>
        /// <param name="settings">CSV serialization settings.</param>
        /// <param name="objectType">Types of objects to serialize from track chunks.</param>
        /// <param name="objectDetectionSettings">Settings used to detect objects.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing serialization and stream flushing.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        /// <exception cref="ObjectDisposedException">The stream was disposed during writing.</exception>
        /// <exception cref="IOException">Writing or flushing the stream failed.</exception>
        public static Task SerializeToCsvAsync(
            this MidiFile midiFile,
            Stream stream,
            CsvSerializationSettings? settings = null,
            ObjectType objectType = ObjectType.TimedEvent,
            ObjectDetectionSettings? objectDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => RunCsvAsync(async () =>
            {
                ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
                ValidateCsvStream(stream, true);

                var serializationSettings = settings ?? new CsvSerializationSettings();
                using var writer = new CsvWriter(serializationSettings);

                WriteHeaderChunk(midiFile, writer, serializationSettings, 0);
                await writer.DrainAsync(stream, cancellationToken).ConfigureAwait(false);
                await WriteChunksAsync(
                    midiFile.Chunks,
                    stream,
                    writer,
                    midiFile.GetTempoMap(),
                    serializationSettings,
                    objectType,
                    objectDetectionSettings ?? new ObjectDetectionSettings(),
                    1,
                    cancellationToken).ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            },
            progress,
            cancellationToken);

        /// <summary>
        /// Serializes a MIDI file to a CSV file asynchronously.
        /// </summary>
        /// <inheritdoc cref="SerializeToCsv(MidiFile, string, bool, CsvSerializationSettings, ObjectType, ObjectDetectionSettings)" path="/member/exception"/>
        /// <param name="midiFile">MIDI file to serialize.</param>
        /// <param name="filePath">Destination file path.</param>
        /// <param name="overwriteFile">Whether an existing destination may be overwritten.</param>
        /// <param name="settings">CSV serialization settings.</param>
        /// <param name="objectType">Types of objects to serialize from track chunks.</param>
        /// <param name="objectDetectionSettings">Settings used to detect objects.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing serialization and file closure.</returns>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is null, empty or invalid.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        /// <exception cref="IOException">Opening, writing or flushing the file failed, or it exists and overwriting is disabled.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to the file was denied.</exception>
        public static async Task SerializeToCsvAsync(
            this MidiFile midiFile,
            string filePath,
            bool overwriteFile,
            CsvSerializationSettings? settings = null,
            ObjectType objectType = ObjectType.TimedEvent,
            ObjectDetectionSettings? objectDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);

            using var stream = OpenCsvForWrite(filePath, overwriteFile);

            await SerializeToCsvAsync(
                midiFile,
                stream,
                settings,
                objectType,
                objectDetectionSettings,
                progress,
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Serializes MIDI chunks using asynchronous stream writes and flush. The stream remains open.
        /// </summary>
        /// <inheritdoc cref="SerializeToCsv(IEnumerable{MidiChunk}, Stream, TempoMap, CsvSerializationSettings, ObjectType, ObjectDetectionSettings)" path="/member/exception"/>
        /// <param name="midiChunks">Chunks to serialize.</param>
        /// <param name="stream">Writable stream, which remains open.</param>
        /// <param name="tempoMap">Tempo map used to convert times and lengths.</param>
        /// <param name="settings">CSV serialization settings.</param>
        /// <param name="objectType">Types of objects to serialize from track chunks.</param>
        /// <param name="objectDetectionSettings">Settings used to detect objects.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing serialization and stream flushing.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        /// <exception cref="ObjectDisposedException">The stream was disposed during writing.</exception>
        /// <exception cref="IOException">Writing or flushing the stream failed.</exception>
        public static Task SerializeToCsvAsync(
            this IEnumerable<MidiChunk?> midiChunks,
            Stream stream,
            TempoMap tempoMap,
            CsvSerializationSettings? settings = null,
            ObjectType objectType = ObjectType.TimedEvent,
            ObjectDetectionSettings? objectDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => RunCsvAsync(async () =>
            {
                ThrowIfArgument.IsNull(nameof(midiChunks), midiChunks);
                ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
                ValidateCsvStream(stream, true);

                var serializationSettings = settings ?? new CsvSerializationSettings();
                using var writer = new CsvWriter(serializationSettings);
                await WriteChunksAsync(
                    midiChunks,
                    stream,
                    writer,
                    tempoMap,
                    serializationSettings,
                    objectType,
                    objectDetectionSettings ?? new ObjectDetectionSettings(),
                    0,
                    cancellationToken).ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            },
            progress,
            cancellationToken);

        /// <summary>
        /// Serializes MIDI chunks to a CSV file asynchronously.
        /// </summary>
        /// <inheritdoc cref="SerializeToCsv(IEnumerable{MidiChunk}, string, bool, TempoMap, CsvSerializationSettings, ObjectType, ObjectDetectionSettings)" path="/member/exception"/>
        /// <param name="midiChunks">Chunks to serialize.</param>
        /// <param name="filePath">Destination file path.</param>
        /// <param name="overwriteFile">Whether an existing destination may be overwritten.</param>
        /// <param name="tempoMap">Tempo map used to convert times and lengths.</param>
        /// <param name="settings">CSV serialization settings.</param>
        /// <param name="objectType">Types of objects to serialize from track chunks.</param>
        /// <param name="objectDetectionSettings">Settings used to detect objects.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing serialization and file closure.</returns>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is null, empty or invalid.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        /// <exception cref="IOException">Opening, writing or flushing the file failed, or it exists and overwriting is disabled.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to the file was denied.</exception>
        public static async Task SerializeToCsvAsync(
            this IEnumerable<MidiChunk?> midiChunks,
            string filePath,
            bool overwriteFile,
            TempoMap tempoMap,
            CsvSerializationSettings? settings = null,
            ObjectType objectType = ObjectType.TimedEvent,
            ObjectDetectionSettings? objectDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfArgument.IsNull(nameof(midiChunks), midiChunks);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            using var stream = OpenCsvForWrite(filePath, overwriteFile);

            await SerializeToCsvAsync(
                midiChunks,
                stream,
                tempoMap,
                settings,
                objectType,
                objectDetectionSettings,
                progress,
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Serializes a MIDI chunk using asynchronous stream writes and flush. The stream remains open.
        /// </summary>
        /// <inheritdoc cref="SerializeToCsv(MidiChunk, Stream, TempoMap, CsvSerializationSettings, ObjectType, ObjectDetectionSettings)" path="/member/exception"/>
        /// <param name="midiChunk">Chunk to serialize.</param>
        /// <param name="stream">Writable stream, which remains open.</param>
        /// <param name="tempoMap">Tempo map used to convert times and lengths.</param>
        /// <param name="settings">CSV serialization settings.</param>
        /// <param name="objectType">Types of objects to serialize from track chunks.</param>
        /// <param name="objectDetectionSettings">Settings used to detect objects.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing serialization and stream flushing.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        /// <exception cref="ObjectDisposedException">The stream was disposed during writing.</exception>
        /// <exception cref="IOException">Writing or flushing the stream failed.</exception>
        public static Task SerializeToCsvAsync(
            this MidiChunk midiChunk,
            Stream stream,
            TempoMap tempoMap,
            CsvSerializationSettings? settings = null,
            ObjectType objectType = ObjectType.TimedEvent,
            ObjectDetectionSettings? objectDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => RunCsvAsync(async () =>
            {
                ThrowIfArgument.IsNull(nameof(midiChunk), midiChunk);
                ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
                ValidateCsvStream(stream, true);

                var serializationSettings = settings ?? new CsvSerializationSettings();
                using var writer = new CsvWriter(serializationSettings);
                await WriteChunksAsync(
                    new[] { midiChunk },
                    stream,
                    writer,
                    tempoMap,
                    serializationSettings,
                    objectType,
                    objectDetectionSettings ?? new ObjectDetectionSettings(),
                    0,
                    cancellationToken).ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            },
            progress,
            cancellationToken);

        /// <summary>
        /// Serializes a MIDI chunk to a CSV file asynchronously.
        /// </summary>
        /// <inheritdoc cref="SerializeToCsv(MidiChunk, string, bool, TempoMap, CsvSerializationSettings, ObjectType, ObjectDetectionSettings)" path="/member/exception"/>
        /// <param name="midiChunk">Chunk to serialize.</param>
        /// <param name="filePath">Destination file path.</param>
        /// <param name="overwriteFile">Whether an existing destination may be overwritten.</param>
        /// <param name="tempoMap">Tempo map used to convert times and lengths.</param>
        /// <param name="settings">CSV serialization settings.</param>
        /// <param name="objectType">Types of objects to serialize from track chunks.</param>
        /// <param name="objectDetectionSettings">Settings used to detect objects.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing serialization and file closure.</returns>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is null, empty or invalid.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        /// <exception cref="IOException">Opening, writing or flushing the file failed, or it exists and overwriting is disabled.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to the file was denied.</exception>
        public static async Task SerializeToCsvAsync(
            this MidiChunk midiChunk,
            string filePath,
            bool overwriteFile,
            TempoMap tempoMap,
            CsvSerializationSettings? settings = null,
            ObjectType objectType = ObjectType.TimedEvent,
            ObjectDetectionSettings? objectDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfArgument.IsNull(nameof(midiChunk), midiChunk);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            using var stream = OpenCsvForWrite(filePath, overwriteFile);

            await SerializeToCsvAsync(
                midiChunk,
                stream,
                tempoMap,
                settings,
                objectType,
                objectDetectionSettings,
                progress,
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Serializes objects using asynchronous stream writes and flush. The stream remains open.
        /// </summary>
        /// <inheritdoc cref="SerializeToCsv(IEnumerable{ITimedObject}, Stream, TempoMap, CsvSerializationSettings)" path="/member/exception"/>
        /// <param name="timedObjects">Objects to serialize.</param>
        /// <param name="stream">Writable stream, which remains open.</param>
        /// <param name="tempoMap">Tempo map used to convert times and lengths.</param>
        /// <param name="settings">CSV serialization settings.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing serialization and stream flushing.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        /// <exception cref="ObjectDisposedException">The stream was disposed during writing.</exception>
        /// <exception cref="IOException">Writing or flushing the stream failed.</exception>
        public static Task SerializeToCsvAsync(
            this IEnumerable<ITimedObject?> timedObjects,
            Stream stream,
            TempoMap tempoMap,
            CsvSerializationSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => RunCsvAsync(async () =>
            {
                ThrowIfArgument.IsNull(nameof(timedObjects), timedObjects);
                ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
                ValidateCsvStream(stream, true);

                var serializationSettings = settings ?? new CsvSerializationSettings();
                using var writer = new CsvWriter(serializationSettings);
                await WriteObjectsAsync(
                    timedObjects,
                    stream,
                    writer,
                    tempoMap,
                    serializationSettings,
                    null,
                    null,
                    cancellationToken).ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            },
            progress,
            cancellationToken);

        /// <summary>
        /// Serializes objects to a CSV file asynchronously.
        /// </summary>
        /// <inheritdoc cref="SerializeToCsv(IEnumerable{ITimedObject}, string, bool, TempoMap, CsvSerializationSettings)" path="/member/exception"/>
        /// <param name="timedObjects">Objects to serialize.</param>
        /// <param name="filePath">Destination file path.</param>
        /// <param name="overwriteFile">Whether an existing destination may be overwritten.</param>
        /// <param name="tempoMap">Tempo map used to convert times and lengths.</param>
        /// <param name="settings">CSV serialization settings.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing serialization and file closure.</returns>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is null, empty or invalid.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        /// <exception cref="IOException">Opening, writing or flushing the file failed, or it exists and overwriting is disabled.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to the file was denied.</exception>
        public static async Task SerializeToCsvAsync(
            this IEnumerable<ITimedObject?> timedObjects,
            string filePath,
            bool overwriteFile,
            TempoMap tempoMap,
            CsvSerializationSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfArgument.IsNull(nameof(timedObjects), timedObjects);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            using var stream = OpenCsvForWrite(filePath, overwriteFile);

            await SerializeToCsvAsync(
                timedObjects,
                stream,
                tempoMap,
                settings,
                progress,
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Deserializes a MIDI file using asynchronous stream reads. The stream remains open.
        /// </summary>
        /// <inheritdoc cref="DeserializeFileFromCsv(Stream, CsvDeserializationSettings)" path="/member/exception"/>
        /// <param name="stream">Readable stream, which remains open.</param>
        /// <param name="settings">CSV deserialization settings.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task returning the deserialized MIDI file.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        /// <exception cref="ObjectDisposedException">The stream was disposed during reading.</exception>
        /// <exception cref="IOException">Reading the stream failed.</exception>
        /// <exception cref="InvalidOperationException">The CSV data contains no header.</exception>
        public static Task<MidiFile> DeserializeFileFromCsvAsync(
            Stream stream,
            CsvDeserializationSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(async () =>
            {
                ValidateCsvStream(stream, false);

                var deserializationSettings = settings ?? new CsvDeserializationSettings();
                using var reader = new CsvReader(stream, deserializationSettings);
                var chunks = await ReadChunksAsync(
                    reader,
                    deserializationSettings,
                    (objects, readChunks) => GetTempoMap(objects, readChunks.OfType<HeaderChunk>().First().TimeDivision),
                    cancellationToken).ConfigureAwait(false);

                if (!chunks.Any())
                    throw CsvError.BadFormat("No CSV data.");

                var header = chunks.OfType<HeaderChunk>().First();
                var file = new MidiFile { TimeDivision = header.TimeDivision };
                file.Chunks.AddRange(chunks.Where(chunk => !(chunk is HeaderChunk)));
                return file;
            },
            cancellationToken,
            progress);

        /// <summary>
        /// Deserializes a MIDI file from CSV asynchronously.
        /// </summary>
        /// <inheritdoc cref="DeserializeFileFromCsv(string, CsvDeserializationSettings)" path="/member/exception"/>
        /// <param name="filePath">Source CSV file path.</param>
        /// <param name="settings">CSV deserialization settings.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task returning the deserialized MIDI file after closing the source file.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        /// <exception cref="IOException">Opening or reading the file failed, or the file or its directory does not exist.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to the file was denied.</exception>
        /// <exception cref="InvalidOperationException">The CSV data contains no header.</exception>
        public static async Task<MidiFile> DeserializeFileFromCsvAsync(
            string filePath,
            CsvDeserializationSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var stream = OpenCsvForRead(filePath);

            return await DeserializeFileFromCsvAsync(stream, settings, progress, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Deserializes MIDI chunks using asynchronous stream reads. The stream remains open.
        /// </summary>
        /// <inheritdoc cref="DeserializeChunksFromCsv(Stream, TempoMap, CsvDeserializationSettings)" path="/member/exception"/>
        /// <param name="stream">Readable stream, which remains open.</param>
        /// <param name="tempoMap">Tempo map used to convert times and lengths.</param>
        /// <param name="settings">CSV deserialization settings.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task returning the deserialized chunks.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        /// <exception cref="ObjectDisposedException">The stream was disposed during reading.</exception>
        /// <exception cref="IOException">Reading the stream failed.</exception>
        public static Task<ICollection<MidiChunk>> DeserializeChunksFromCsvAsync(
            Stream stream,
            TempoMap tempoMap,
            CsvDeserializationSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(async () =>
            {
                ValidateCsvStream(stream, false);
                ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

                var deserializationSettings = settings ?? new CsvDeserializationSettings();
                using var reader = new CsvReader(stream, deserializationSettings);
                return await ReadChunksAsync(
                    reader,
                    deserializationSettings,
                    (objects, chunks) => tempoMap,
                    cancellationToken).ConfigureAwait(false);
            },
            cancellationToken,
            progress);

        /// <summary>
        /// Deserializes MIDI chunks from CSV asynchronously.
        /// </summary>
        /// <inheritdoc cref="DeserializeChunksFromCsv(string, TempoMap, CsvDeserializationSettings)" path="/member/exception"/>
        /// <param name="filePath">Source CSV file path.</param>
        /// <param name="tempoMap">Tempo map used to convert times and lengths.</param>
        /// <param name="settings">CSV deserialization settings.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task returning the deserialized chunks after closing the source file.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        /// <exception cref="IOException">Opening or reading the file failed, or the file or its directory does not exist.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to the file was denied.</exception>
        public static async Task<ICollection<MidiChunk>> DeserializeChunksFromCsvAsync(
            string filePath,
            TempoMap tempoMap,
            CsvDeserializationSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            using var stream = OpenCsvForRead(filePath);

            return await DeserializeChunksFromCsvAsync(stream, tempoMap, settings, progress, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Deserializes a MIDI chunk using asynchronous stream reads. The stream remains open.
        /// </summary>
        /// <inheritdoc cref="DeserializeChunkFromCsv(Stream, TempoMap, CsvDeserializationSettings)" path="/member/exception"/>
        /// <param name="stream">Readable stream, which remains open.</param>
        /// <param name="tempoMap">Tempo map used to convert times and lengths.</param>
        /// <param name="settings">CSV deserialization settings.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task returning the deserialized chunk.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        /// <exception cref="ObjectDisposedException">The stream was disposed during reading.</exception>
        /// <exception cref="IOException">Reading the stream failed.</exception>
        public static Task<MidiChunk> DeserializeChunkFromCsvAsync(
            Stream stream,
            TempoMap tempoMap,
            CsvDeserializationSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(async () =>
            {
                ValidateCsvStream(stream, false);
                ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

                var deserializationSettings = settings ?? new CsvDeserializationSettings();
                using var reader = new CsvReader(stream, deserializationSettings);
                var chunks = await ReadChunksAsync(
                    reader,
                    deserializationSettings,
                    (objects, readChunks) => tempoMap,
                    cancellationToken).ConfigureAwait(false);

                if (!chunks.Any())
                    throw CsvError.BadFormat("No CSV data.");

                return chunks.First();
            },
            cancellationToken,
            progress);

        /// <summary>
        /// Deserializes a MIDI chunk from CSV asynchronously.
        /// </summary>
        /// <inheritdoc cref="DeserializeChunkFromCsv(string, TempoMap, CsvDeserializationSettings)" path="/member/exception"/>
        /// <param name="filePath">Source CSV file path.</param>
        /// <param name="tempoMap">Tempo map used to convert times and lengths.</param>
        /// <param name="settings">CSV deserialization settings.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task returning the deserialized chunk after closing the source file.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        /// <exception cref="IOException">Opening or reading the file failed, or the file or its directory does not exist.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to the file was denied.</exception>
        public static async Task<MidiChunk> DeserializeChunkFromCsvAsync(
            string filePath,
            TempoMap tempoMap,
            CsvDeserializationSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            using var stream = OpenCsvForRead(filePath);

            return await DeserializeChunkFromCsvAsync(stream, tempoMap, settings, progress, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Deserializes objects using asynchronous stream reads. The stream remains open.
        /// </summary>
        /// <inheritdoc cref="DeserializeObjectsFromCsv(Stream, TempoMap, CsvDeserializationSettings)" path="/member/exception"/>
        /// <param name="stream">Readable stream, which remains open.</param>
        /// <param name="tempoMap">Tempo map used to convert times and lengths.</param>
        /// <param name="settings">CSV deserialization settings.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task returning the deserialized objects.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        /// <exception cref="ObjectDisposedException">The stream was disposed during reading.</exception>
        /// <exception cref="IOException">Reading the stream failed.</exception>
        public static Task<ICollection<ITimedObject>> DeserializeObjectsFromCsvAsync(
            Stream stream,
            TempoMap tempoMap,
            CsvDeserializationSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(async () =>
            {
                ValidateCsvStream(stream, false);
                ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

                var deserializationSettings = settings ?? new CsvDeserializationSettings();
                using var reader = new CsvReader(stream, deserializationSettings);
                var objects = new List<CsvObject>();
                var chords = new Dictionary<(int?, int?), CsvChord>();
                CsvRecord? csvRecord;

                while ((csvRecord = await reader.ReadRecordAsync(cancellationToken).ConfigureAwait(false)) != null)
                {
                    AsyncOperation.Checkpoint();

                    var record = ParseRecord(csvRecord, false)!;
                    var type = GetRecordType(record.RecordType, csvRecord.LineNumber, deserializationSettings.UnknownRecordPolicy);
                    if (type != null)
                        ReadObject(record, type, objects, chords, deserializationSettings, false);
                }

                if (!objects.Any())
                    throw CsvError.BadFormat("No CSV data.");

                return GetTimedObjects(objects, tempoMap).FirstOrDefault() ?? (ICollection<ITimedObject>)Array.Empty<ITimedObject>();
            },
            cancellationToken,
            progress);

        /// <summary>
        /// Deserializes objects from CSV asynchronously.
        /// </summary>
        /// <inheritdoc cref="DeserializeObjectsFromCsv(string, TempoMap, CsvDeserializationSettings)" path="/member/exception"/>
        /// <param name="filePath">Source CSV file path.</param>
        /// <param name="tempoMap">Tempo map used to convert times and lengths.</param>
        /// <param name="settings">CSV deserialization settings.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task returning the deserialized objects after closing the source file.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        /// <exception cref="IOException">Opening or reading the file failed, or the file or its directory does not exist.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to the file was denied.</exception>
        public static async Task<ICollection<ITimedObject>> DeserializeObjectsFromCsvAsync(
            string filePath,
            TempoMap tempoMap,
            CsvDeserializationSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            using var stream = OpenCsvForRead(filePath);

            return await DeserializeObjectsFromCsvAsync(stream, tempoMap, settings, progress, cancellationToken).ConfigureAwait(false);
        }

        private static Task RunCsvAsync(
            Func<Task> action,
            IProgress<AsyncOperationProgress>? progress,
            CancellationToken cancellationToken)
            => AsyncOperation.Run(async () =>
            {
                await action().ConfigureAwait(false);
                return true;
            },
            cancellationToken,
            progress);

        private static void ValidateCsvStream(Stream stream, bool writing)
        {
            ThrowIfArgument.IsNull(nameof(stream), stream);

            if (writing ? !stream.CanWrite : !stream.CanRead)
                throw new ArgumentException(writing ? "Stream doesn't support writing." : "Stream doesn't support reading.", nameof(stream));
        }

        private static async Task WriteChunksAsync(
            IEnumerable<MidiChunk?> chunks,
            Stream stream,
            CsvWriter writer,
            TempoMap tempoMap,
            CsvSerializationSettings settings,
            ObjectType objectType,
            ObjectDetectionSettings detectionSettings,
            int chunkIndex,
            CancellationToken cancellationToken)
        {
            foreach (var chunk in chunks.OfType<MidiChunk>())
            {
                AsyncOperation.Checkpoint();

                if (chunk is TrackChunk track)
                {
                    await WriteObjectsAsync(
                        track.GetObjects(objectType, detectionSettings),
                        stream,
                        writer,
                        tempoMap,
                        settings,
                        chunkIndex,
                        TrackChunk.Id,
                        cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    WriteChunk(chunk, writer, settings, tempoMap, objectType, detectionSettings, chunkIndex);
                    await writer.DrainAsync(stream, cancellationToken).ConfigureAwait(false);
                }

                chunkIndex++;
            }
        }

        private static async Task WriteObjectsAsync(
            IEnumerable<ITimedObject?> objects,
            Stream stream,
            CsvWriter writer,
            TempoMap tempoMap,
            CsvSerializationSettings settings,
            int? chunkIndex,
            string? chunkId,
            CancellationToken cancellationToken)
        {
            int objectIndex = 0;

            foreach (var obj in objects.OfType<ITimedObject>())
            {
                AsyncOperation.Checkpoint();

                if (obj is Chord chord)
                {
                    foreach (var note in chord.Notes)
                    {
                        AsyncOperation.Checkpoint();

                        WriteNote(note, writer, settings, tempoMap, chunkIndex, chunkId, objectIndex);
                        await writer.DrainAsync(stream, cancellationToken).ConfigureAwait(false);
                    }
                }
                else if (obj is RegisteredParameter parameter)
                {
                    foreach (var timedEvent in parameter.GetTimedEvents())
                    {
                        AsyncOperation.Checkpoint();

                        WriteTimedEvent(timedEvent, writer, settings, tempoMap, chunkIndex, chunkId, objectIndex);
                        await writer.DrainAsync(stream, cancellationToken).ConfigureAwait(false);
                    }
                }
                else
                {
                    WriteObject(obj, writer, settings, tempoMap, chunkIndex, chunkId, objectIndex);
                    await writer.DrainAsync(stream, cancellationToken).ConfigureAwait(false);
                }

                objectIndex++;
            }
        }

        private static async Task<ICollection<MidiChunk>> ReadChunksAsync(
            CsvReader reader,
            CsvDeserializationSettings settings,
            Func<ICollection<CsvObject>, ICollection<MidiChunk>, TempoMap> getTempoMap,
            CancellationToken cancellationToken)
        {
            var result = new List<MidiChunk>();
            var objects = new List<CsvObject>();
            var chords = new Dictionary<(int?, int?), CsvChord>();
            CsvRecord? csvRecord;

            while ((csvRecord = await reader.ReadRecordAsync(cancellationToken).ConfigureAwait(false)) != null)
            {
                AsyncOperation.Checkpoint();

                var record = ParseRecord(csvRecord, true)!;
                var type = GetRecordType(record.RecordType, csvRecord.LineNumber, settings.UnknownRecordPolicy);
                if (type == null || (record.ChunkId != TrackChunk.Id && record.ChunkId != HeaderChunk.Id))
                    continue;

                switch (type)
                {
                    case RecordType.Header:
                        result.Add(ParseHeader(record));
                        break;
                    case RecordType.Event:
                    case RecordType.Note:
                        ReadObject(record, type, objects, chords, settings, true);
                        break;
                }
            }

            if (!objects.Any())
                return result;

            var tempoMap = getTempoMap(objects, result);

            foreach (var chunkObjects in GetTimedObjects(objects, tempoMap))
            {
                AsyncOperation.Checkpoint();

                result.Add(chunkObjects.ToTrackChunk());
            }

            return result;
        }

        private static FileStream OpenCsvForRead(string filePath)
        {
            ThrowIfArgument.IsNullOrEmptyString(nameof(filePath), filePath, "File path");

            return new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                4096,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
        }

        private static FileStream OpenCsvForWrite(string filePath, bool overwriteFile)
        {
            ThrowIfArgument.IsNullOrEmptyString(nameof(filePath), filePath, "File path");

            return new FileStream(
                filePath,
                overwriteFile ? FileMode.Create : FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                4096,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
        }
    }
}
