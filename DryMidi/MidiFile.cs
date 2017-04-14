using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Melanchall.DryMidi
{
    /// <summary>
    /// Class that represents a MIDI file.
    /// </summary>
    public sealed class MidiFile
    {
        #region Properties

        /// <summary>
        /// Gets or sets a time division of a MIDI file.
        /// </summary>
        /// <remarks>
        /// Time division specifies the meaning of the delta-times of messages. There are two types of
        /// the time division: ticks per quarter note and SMPTE. The first type represented by
        /// <see cref="TicksPerQuarterNoteTimeDivision"/> class and the second one represented by
        /// <see cref="SmpteTimeDivision"/> class.
        /// </remarks>
        public TimeDivision TimeDivision { get; set; }

        /// <summary>
        /// Gets collection of chunks of a MIDI file.
        /// </summary>
        /// <remarks>
        /// MIDI Files are made up of chunks. Сollection returned by this property may contain chunks
        /// of the following types: <see cref="TrackChunk"/>, <see cref="UnknownChunk"/>, and any custom
        /// chunk types you've defined.
        /// </remarks>
        public ChunksCollection Chunks { get; } = new ChunksCollection();

        #endregion

        #region Methods

        /// <summary>
        /// Loads a MIDI file specified by its full path.
        /// </summary>
        /// <param name="filePath">Path to the file to load.</param>
        /// <param name="settings">Settings according to which the file must be loaded.</param>
        /// <returns>An instance of the <see cref="MidiFile"/> representing a MIDI file.</returns>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is a zero-length string,
        /// contains only white space, or contains one or more invalid characters as defined by
        /// <see cref="Path.InvalidPathChars"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is null.</exception>
        /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined
        /// maximum length. For example, on Windows-based platforms, paths must be less than 248 characters,
        /// and file names must be less than 260 characters.</exception>
        /// <exception cref="DirectoryNotFoundException">The specified path is invalid, (for example,
        /// it is on an unmapped drive).</exception>
        /// <exception cref="IOException">An I/O error occurred while saving the file.</exception>
        /// <exception cref="NotSupportedException"><paramref name="filePath"/> is in an invalid format.</exception>
        /// <exception cref="UnauthorizedAccessException">This operation is not supported on the current platform.-or-
        /// <paramref name="filePath"/> specified a directory.-or- The caller does not have the required permission.</exception>
        /// <exception cref="NoHeaderChunkException">There is no header chunk in a file.</exception>
        /// <exception cref="InvalidChunkSizeException">Actual header or track chunk's size differs from the one declared
        /// in its header and that should be treated as error according to the <paramref name="settings"/>.
        /// <exception cref="UnknownChunkIdException">Chunk to be read has unknown ID and that
        /// should be treated as error accordng to the <paramref name="settings"/>.</exception>
        /// <exception cref="UnexpectedTrackChunksCountException">Actual track chunks
        /// count differs from the expected one and that should be treated as error according to
        /// the specified <paramref name="settings"/>.</exception>
        public static MidiFile Load(string filePath, ReadingSettings settings = null)
        {
            using (var fileStream = File.OpenRead(filePath))
            {
                return Read(fileStream, settings);
            }
        }

        /// <summary>
        /// Saves the MIDI file to location specified by full path.
        /// </summary>
        /// <param name="filePath">Full path of the file to save to.</param>
        /// <param name="overwriteFile">If true and file specified by <paramref name="filePath"/> already
        /// exists it will be overwritten; if false and the file exists exception will be thrown.</param>
        /// <param name="format">MIDI file format to save in.</param>
        /// <param name="settings">Settings according to which the file must be saved.</param>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is a zero-length string,
        /// contains only white space, or contains one or more invalid characters as defined by
        /// <see cref="Path.InvalidPathChars"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is null.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="format"/> specified an invalid value.</exception>
        /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined
        /// maximum length. For example, on Windows-based platforms, paths must be less than 248 characters,
        /// and file names must be less than 260 characters.</exception>
        /// <exception cref="DirectoryNotFoundException">The specified path is invalid, (for example,
        /// it is on an unmapped drive).</exception>
        /// <exception cref="IOException">An I/O error occurred while saving the file.</exception>
        /// <exception cref="NotSupportedException"><paramref name="filePath"/> is in an invalid format.</exception>
        /// <exception cref="UnauthorizedAccessException">This operation is not supported on the current platform.-or-
        /// <paramref name="filePath"/> specified a directory.-or- The caller does not have the required permission.</exception>
        /// <exception cref="TooManyTrackChunksException">Count of track chunks presented in the file
        /// exceeds maximum value allowed for MIDI file.</exception>
        public void Save(string filePath, bool overwriteFile = false, MidiFileFormat format = MidiFileFormat.MultiTrack, WritingSettings settings = null)
        {
            if (!Enum.IsDefined(typeof(MidiFileFormat), format))
                throw new InvalidEnumArgumentException(nameof(format), (int)format, typeof(MidiFileFormat));

            using (var fileStream = File.Open(filePath, overwriteFile ? FileMode.Create : FileMode.CreateNew))
            {
                Write(fileStream, format, settings);
            }
        }

        /// <summary>
        /// Reads a MIDI file from the stream.
        /// </summary>
        /// <param name="stream">Stream to read file from.</param>
        /// <param name="settings">Settings according to which the file must be read.</param>
        /// <returns>An instance of the <see cref="MidiFile"/> representing a MIDI file was read from the stream.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Position of the stream is placed at its end.</exception>
        /// <exception cref="ArgumentException"><paramref name="stream"/> does not support reading,
        /// or is already closed.</exception>
        /// <exception cref="IOException">An I/O error occurred while reading from the stream.</exception>
        /// <exception cref="NoHeaderChunkException">There is no header chunk in a file.</exception>
        /// <exception cref="InvalidChunkSizeException">Actual header or track chunk's size differs from the one declared
        /// in its header and that should be treated as error according to the <paramref name="settings"/>.
        /// <exception cref="UnknownChunkIdException">Chunk to be read has unknown ID and that
        /// should be treated as error accordng to the <paramref name="settings"/>.</exception>
        /// <exception cref="UnexpectedTrackChunksCountException">Actual track chunks
        /// count differs from the expected one and that should be treated as error according to
        /// the specified <paramref name="settings"/>.</exception>
        public static MidiFile Read(Stream stream, ReadingSettings settings = null)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (stream.Position >= stream.Length)
                throw new InvalidOperationException("Cannot read MIDI file from the stream with position at the end of it.");

            //

            if (settings == null)
                settings = new ReadingSettings();

            var file = new MidiFile();

            //

            using (var reader = new MidiReader(stream))
            {
                var headerChunk = ReadHeaderChunk(reader, settings);
                file.TimeDivision = headerChunk.TimeDivision;

                var expectedTrackChunksCount = headerChunk.TracksNumber;
                var actualTrackChunksCount = 0;

                while (!reader.EndReached)
                {
                    var chunk = ReadChunk(reader, settings, actualTrackChunksCount, expectedTrackChunksCount);
                    if (chunk == null)
                        continue;

                    if (chunk is TrackChunk)
                        actualTrackChunksCount++;

                    file.Chunks.Add(chunk);
                }

                ReactOnUnexpectedTrackChunksCount(settings.UnexpectedTrackChunksCountPolicy, actualTrackChunksCount, expectedTrackChunksCount);
            }

            //

            return file;
        }

        /// <summary>
        /// Writes current <see cref="MidiFile"/> to the stream.
        /// </summary>
        /// <param name="stream">Stream to write file's data to.</param>
        /// <param name="format">Format of the file to be written.</param>
        /// <param name="settings">Settings according to which the file must be written.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="stream"/> does not support writing,
        /// or is already closed.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="format"/> specified an invalid value.</exception>
        /// <exception cref="TooManyTrackChunksException">Count of track chunks presented in the file
        /// exceeds maximum value allowed for MIDI file.</exception>
        public void Write(Stream stream, MidiFileFormat format = MidiFileFormat.MultiTrack, WritingSettings settings = null)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (!Enum.IsDefined(typeof(MidiFileFormat), format))
                throw new InvalidEnumArgumentException(nameof(format), (int)format, typeof(MidiFileFormat));

            //

            if (settings == null)
                settings = new WritingSettings();

            using (var writer = new MidiWriter(stream))
            {
                var chunksConverter = ChunksConverterFactory.GetConverter(format);
                var chunks = chunksConverter.Convert(Chunks);

                var trackChunksCount = chunks.Count(c => c is TrackChunk);
                if (trackChunksCount > ushort.MaxValue)
                    throw new TooManyTrackChunksException(
                        $"Count of track chunks to be written ({trackChunksCount}) is greater than the valid maximum ({ushort.MaxValue}).",
                        trackChunksCount);

                var headerChunk = new HeaderChunk
                {
                    FileFormat = (ushort)format,
                    TimeDivision = TimeDivision,
                    TracksNumber = (ushort)trackChunksCount
                };
                headerChunk.Write(writer, settings);

                foreach (var chunk in chunks)
                {
                    if (settings.CompressionPolicy.HasFlag(CompressionPolicy.DeleteUnknownChunks) && chunk is UnknownChunk)
                        continue;

                    chunk.Write(writer, settings);
                }
            }
        }

        /// <summary>
        /// Reads a header chunk from a MIDI-file.
        /// </summary>
        /// <param name="reader">Reader to read a chunk with.</param>
        /// <param name="settings">Settings according to which a chunk must be read.</param>
        /// <returns>A MIDI-file header chunk.</returns>
        /// <exception cref="IOException">An I/O error occurred on the underlying stream.</exception>
        /// <exception cref="NoHeaderChunkException">There is no header chunk in a file.</exception>
        /// <exception cref="InvalidChunkSizeException">Actual chunk's size differs from the one declared
        /// in its header and that should be treated as error according to the specified
        /// <paramref name="settings"/>.</exception>
        private static HeaderChunk ReadHeaderChunk(MidiReader reader, ReadingSettings settings)
        {
            var chunkId = reader.ReadString(Chunk.IdLength);
            if (chunkId != HeaderChunk.Id)
                throw new NoHeaderChunkException($"'{chunkId}' is invalid header chunk's ID. It must be '{HeaderChunk.Id}'.");

            var headerChunk = new HeaderChunk();
            headerChunk.Read(reader, settings);
            return headerChunk;
        }

        /// <summary>
        /// Reads a chunk from a MIDI-file.
        /// </summary>
        /// <param name="reader">Reader to read a chunk with.</param>
        /// <param name="settings">Settings according to which a chunk must be read.</param>
        /// <param name="actualTrackChunksCount">Actual count of track chunks at the moment.</param>
        /// <param name="expectedTrackChunksCount">Expected count of track chunks.</param>
        /// <returns>A MIDI-file chunk.</returns>
        /// <exception cref="ObjectDisposedException">Method was called after the reader was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred on the underlying stream.</exception>
        /// <exception cref="UnknownChunkIdException">Chunk to be read has unknown ID and that
        /// should be treated as error accordng to the specified <paramref name="settings"/>.</exception>
        /// <exception cref="UnexpectedTrackChunksCountException">Actual track chunks
        /// count is greater than expected one and that should be treated as error according to
        /// the specified <paramref name="settings"/>.</exception>
        /// <exception cref="InvalidChunkSizeException">Actual chunk's size differs from the one declared
        /// in its header and that should be treated as error according to the specified
        /// <paramref name="settings"/>.</exception>
        private static Chunk ReadChunk(MidiReader reader, ReadingSettings settings, int actualTrackChunksCount, int expectedTrackChunksCount)
        {
            var chunkId = reader.ReadString(Chunk.IdLength);
            var chunk = chunkId == TrackChunk.Id
                ? new TrackChunk()
                : TryCreateChunk(chunkId, settings.CustomChunksTypes);

            if (chunk == null)
            {
                switch (settings.UnknownChunkIdPolicy)
                {
                    case UnknownChunkIdPolicy.ReadAsUnknownChunk:
                        chunk = new UnknownChunk(chunkId);
                        break;

                    case UnknownChunkIdPolicy.Skip:
                        var size = reader.ReadDword();
                        reader.Position += size;
                        return null;

                    case UnknownChunkIdPolicy.Abort:
                        throw new UnknownChunkIdException($"'{chunkId}' chunk ID is unknown.", chunkId);
                }
            }

            if (chunk is TrackChunk && actualTrackChunksCount >= expectedTrackChunksCount)
            {
                ReactOnUnexpectedTrackChunksCount(settings.UnexpectedTrackChunksCountPolicy, actualTrackChunksCount, expectedTrackChunksCount);

                switch (settings.ExtraTrackChunkPolicy)
                {
                    case ExtraTrackChunkPolicy.Read:
                        break;

                    case ExtraTrackChunkPolicy.Skip:
                        var size = reader.ReadDword();
                        reader.Position += size;
                        return null;
                }
            }

            chunk.Read(reader, settings);
            return chunk;
        }

        /// <summary>
        /// Does nothing if difference between expected track chunks count and the actual one should not
        /// be treated as error; or throws the <see cref="UnexpectedTrackChunksCountException"/> if this
        /// difference is unallowable.
        /// </summary>
        /// <param name="policy">The policy according to which the method should operate.</param>
        /// <param name="actualTrackChunksCount">Actual count of track chunks.</param>
        /// <param name="expectedTrackChunksCount">Expected count of track chunks.</param>
        /// <exception cref="UnexpectedTrackChunksCountException">Difference between expected track chunks
        /// count and the actual one is unallowable due to <paramref name="policy"/>.</exception>
        private static void ReactOnUnexpectedTrackChunksCount(UnexpectedTrackChunksCountPolicy policy, int actualTrackChunksCount, int expectedTrackChunksCount)
        {
            switch (policy)
            {
                case UnexpectedTrackChunksCountPolicy.Ignore:
                    break;

                case UnexpectedTrackChunksCountPolicy.Abort:
                    throw new UnexpectedTrackChunksCountException(
                        $"Count of track chunks is {actualTrackChunksCount} while {expectedTrackChunksCount} expected.",
                        actualTrackChunksCount,
                        expectedTrackChunksCount);
            }
        }

        /// <summary>
        /// Tries to create an instance of a chunk type that has specified ID.
        /// </summary>
        /// <param name="chunkId">ID of the chunk that need to be created.</param>
        /// <param name="chunksTypes">Collection of the <see cref="Type"/> objects to search for
        /// the chunk type with <paramref name="chunkId"/> ID.</param>
        /// <returns>An instance of the chunk type with the specified ID or null if <paramref name="chunksTypes"/>
        /// doesn;t contain chunk type with it.</returns>
        private static Chunk TryCreateChunk(string chunkId, IEnumerable<Type> chunksTypes)
        {
            if (chunksTypes == null || !chunksTypes.Any())
                return null;

            return chunksTypes.Where(IsChunkType)
                              .Select(Activator.CreateInstance)
                              .OfType<Chunk>()
                              .Where(c => c.ChunkId == chunkId)
                              .FirstOrDefault();
        }

        /// <summary>
        /// Checks if a type represents a MIDI-file chunk.
        /// </summary>
        /// <param name="type">Type to check whether it represents a chunk or not.</param>
        /// <returns>True if passed type represents a MIDI-file chunk; false - otherwise.</returns>
        /// <remarks>
        /// Type represents a chunk if it is derived from the <see cref="Chunk"/> class and has
        /// parameterless constructor.
        /// </remarks>
        private static bool IsChunkType(Type type)
        {
            return type != null &&
                   type.IsSubclassOf(typeof(Chunk)) &&
                   type.GetConstructor(Type.EmptyTypes) != null;
        }

        #endregion
    }
}
