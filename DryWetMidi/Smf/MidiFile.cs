using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Class that represents a MIDI file.
    /// </summary>
    public sealed class MidiFile
    {
        #region Constants

        private const string RiffChunkId = "RIFF";
        private const int RmidPreambleSize = 12; // RMID_size (4) + 'RMID' (4) + 'data' (4)

        #endregion

        #region Fields

        private ushort? _originalFormat;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MidiFile"/>.
        /// </summary>
        public MidiFile()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MidiFile"/> with the specified chunks.
        /// </summary>
        /// <param name="chunks">Chunks to add to the file.</param>
        /// <remarks>
        /// Note that header chunks cannot be added into the collection since it may cause inconsistence in the file structure.
        /// Header chunk with appropriate information will be written to a file automatically on
        /// <see cref="Write(string, bool, MidiFileFormat, WritingSettings)"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="chunks"/> is null.</exception>
        public MidiFile(IEnumerable<MidiChunk> chunks)
        {
            ThrowIfArgument.IsNull(nameof(chunks), chunks);

            Chunks.AddRange(chunks);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MidiFile"/> with the specified chunks.
        /// </summary>
        /// <param name="chunks">Chunks to add to the file.</param>
        /// <remarks>
        /// Note that header chunks cannot be added into the collection since it may cause inconsistence in the file structure.
        /// Header chunk with appropriate information will be written to a file automatically on
        /// <see cref="Write(string, bool, MidiFileFormat, WritingSettings)"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="chunks"/> is null.</exception>
        public MidiFile(params MidiChunk[] chunks)
            : this(chunks as IEnumerable<MidiChunk>)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a time division of a MIDI file.
        /// </summary>
        /// <remarks>
        /// Time division specifies the meaning of the delta-times of events. There are two types of
        /// the time division: ticks per quarter note and SMPTE. The first type represented by
        /// <see cref="TicksPerQuarterNoteTimeDivision"/> class and the second one represented by
        /// <see cref="SmpteTimeDivision"/> class.
        /// </remarks>
        public TimeDivision TimeDivision { get; set; } = new TicksPerQuarterNoteTimeDivision();

        /// <summary>
        /// Gets collection of chunks of a MIDI file.
        /// </summary>
        /// <remarks>
        /// MIDI Files are made up of chunks. Сollection returned by this property may contain chunks
        /// of the following types: <see cref="TrackChunk"/>, <see cref="UnknownChunk"/>, and any custom
        /// chunk types you've defined.
        /// </remarks>
        public ChunksCollection Chunks { get; } = new ChunksCollection();

        /// <summary>
        /// Gets original format of the file was read. This property returns null for the <see cref="MidiFile"/>
        /// created by constructor.
        /// </summary>
        /// <exception cref="UnknownFileFormatException">File format is unknown.</exception>
        /// <exception cref="InvalidOperationException">Unable to get original format of the file.</exception>
        public MidiFileFormat OriginalFormat
        {
            get
            {
                if (_originalFormat == null)
                    throw new InvalidOperationException("Unable to get original format of the file.");

                var formatValue = _originalFormat.Value;
                if (!Enum.IsDefined(typeof(MidiFileFormat), formatValue))
                    throw new UnknownFileFormatException($"File format {formatValue} is unknown.", formatValue);

                return (MidiFileFormat)formatValue;
            }
            internal set
            {
                _originalFormat = (ushort)value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Reads a MIDI file specified by its full path.
        /// </summary>
        /// <param name="filePath">Path to the file to read.</param>
        /// <param name="settings">Settings according to which the file must be read.</param>
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
        /// <exception cref="IOException">An I/O error occurred while reading the file.</exception>
        /// <exception cref="NotSupportedException"><paramref name="filePath"/> is in an invalid format.</exception>
        /// <exception cref="UnauthorizedAccessException">This operation is not supported on the current platform. -or-
        /// <paramref name="filePath"/> specified a directory. -or- The caller does not have the required permission.</exception>
        /// <exception cref="NoHeaderChunkException">There is no header chunk in a file.</exception>
        /// <exception cref="InvalidChunkSizeException">Actual header or track chunk's size differs from the one declared
        /// in its header and that should be treated as error according to the <paramref name="settings"/>.</exception>
        /// <exception cref="UnknownChunkException">Chunk to be read has unknown ID and that
        /// should be treated as error accordng to the <paramref name="settings"/>.</exception>
        /// <exception cref="UnexpectedTrackChunksCountException">Actual track chunks
        /// count differs from the expected one and that should be treated as error according to
        /// the specified <paramref name="settings"/>.</exception>
        /// <exception cref="UnknownFileFormatException">The header chunk contains unknown file format and
        /// <see cref="ReadingSettings.UnknownFileFormatPolicy"/> property of the <paramref name="settings"/> set to
        /// <see cref="UnknownFileFormatPolicy.Abort"/>.</exception>
        /// <exception cref="InvalidChannelEventParameterValueException">Value of a channel event's parameter
        /// just read is invalid.</exception>
        /// <exception cref="InvalidMetaEventParameterValueException">Value of a meta event's parameter
        /// just read is invalid.</exception>
        /// <exception cref="UnknownChannelEventException">Reader has encountered an unknown channel event.</exception>
        /// <exception cref="NotEnoughBytesException">MIDI file cannot be read since the reader's underlying stream doesn't
        /// have enough bytes.</exception>
        /// <exception cref="UnexpectedRunningStatusException">Unexpected running status is encountered.</exception>
        /// <exception cref="MissedEndOfTrackEventException">Track chunk doesn't end with End Of Track event and that
        /// should be treated as error accordng to the specified <paramref name="settings"/>.</exception>
        public static MidiFile Read(string filePath, ReadingSettings settings = null)
        {
            using (var fileStream = FileUtilities.OpenFileForRead(filePath))
            {
                return Read(fileStream, settings);
            }
        }

        /// <summary>
        /// Writes the MIDI file to location specified by full path.
        /// </summary>
        /// <param name="filePath">Full path of the file to write to.</param>
        /// <param name="overwriteFile">If true and file specified by <paramref name="filePath"/> already
        /// exists it will be overwritten; if false and the file exists exception will be thrown.</param>
        /// <param name="format">MIDI file format to write in.</param>
        /// <param name="settings">Settings according to which the file must be written.</param>
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
        /// <exception cref="IOException">An I/O error occurred while writing the file.</exception>
        /// <exception cref="NotSupportedException"><paramref name="filePath"/> is in an invalid format.</exception>
        /// <exception cref="UnauthorizedAccessException">This operation is not supported on the current platform.-or-
        /// <paramref name="filePath"/> specified a directory.-or- The caller does not have the required permission.</exception>
        /// <exception cref="InvalidOperationException">Time division is null.</exception>
        /// <exception cref="TooManyTrackChunksException">Count of track chunks presented in the file
        /// exceeds maximum value allowed for MIDI file.</exception>
        public void Write(string filePath, bool overwriteFile = false, MidiFileFormat format = MidiFileFormat.MultiTrack, WritingSettings settings = null)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(format), format);

            using (var fileStream = FileUtilities.OpenFileForWrite(filePath, overwriteFile))
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
        /// <remarks>
        /// Stream must be readable, seekable and be able to provide its position and length via <see cref="Stream.Position"/>
        /// and <see cref="Stream.Length"/> properties.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="stream"/> doesn't support reading. -or
        /// <paramref name="stream"/> doesn't support seeking. -or- <paramref name="stream"/> is already read.</exception>
        /// <exception cref="IOException">An I/O error occurred while reading from the stream.</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="stream"/> is disposed. -or-
        /// Underlying stream reader is disposed.</exception>
        /// <exception cref="NotSupportedException">Unable to get position of the <paramref name="stream"/>. -or
        /// Unable to get length of the <paramref name="stream"/>.</exception>
        /// <exception cref="NoHeaderChunkException">There is no header chunk in a file.</exception>
        /// <exception cref="InvalidChunkSizeException">Actual header or track chunk's size differs from the one declared
        /// in its header and that should be treated as error according to the <paramref name="settings"/>.</exception>
        /// <exception cref="UnknownChunkException">Chunk to be read has unknown ID and that
        /// should be treated as error accordng to the <paramref name="settings"/>.</exception>
        /// <exception cref="UnexpectedTrackChunksCountException">Actual track chunks
        /// count differs from the expected one and that should be treated as error according to
        /// the specified <paramref name="settings"/>.</exception>
        /// <exception cref="UnknownFileFormatException">The header chunk contains unknown file format and
        /// <see cref="ReadingSettings.UnknownFileFormatPolicy"/> property of the <paramref name="settings"/> set to
        /// <see cref="UnknownFileFormatPolicy.Abort"/>.</exception>
        /// <exception cref="NotEnoughBytesException">MIDI file cannot be read since the reader's underlying stream doesn't
        /// have enough bytes.</exception>
        /// <exception cref="UnexpectedRunningStatusException">Unexpected running status is encountered.</exception>
        /// <exception cref="MissedEndOfTrackEventException">Track chunk doesn't end with End Of Track event and that
        /// should be treated as error accordng to the specified <paramref name="settings"/>.</exception>
        /// <exception cref="InvalidChannelEventParameterValueException">Value of a channel event's parameter
        /// just read is invalid.</exception>
        /// <exception cref="InvalidMetaEventParameterValueException">Value of a meta event's parameter
        /// just read is invalid.</exception>
        public static MidiFile Read(Stream stream, ReadingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(stream), stream);

            if (!stream.CanRead)
                throw new ArgumentException("Stream doesn't support reading.", nameof(stream));

            if (!stream.CanSeek)
                throw new ArgumentException("Stream doesn't support seeking.", nameof(stream));

            if (stream.Position >= stream.Length)
                throw new ArgumentException("Stream is already read.", nameof(stream));

            //

            if (settings == null)
                settings = new ReadingSettings();

            var file = new MidiFile();

            int? expectedTrackChunksCount = null;
            int actualTrackChunksCount = 0;
            bool headerChunkIsRead = false;

            //

            try
            {
                using (var reader = new MidiReader(stream))
                {
                    // Read RIFF header

                    long? smfEndPosition = null;

                    var chunkId = reader.ReadString(RiffChunkId.Length);
                    if (chunkId == RiffChunkId)
                    {
                        reader.Position += RmidPreambleSize;
                        var smfSize = reader.ReadDword();
                        smfEndPosition = reader.Position + smfSize;
                    }
                    else
                        reader.Position -= chunkId.Length;

                    // Read SMF

                    while (!reader.EndReached && (smfEndPosition == null || reader.Position < smfEndPosition))
                    {
                        // Read chunk

                        var chunk = ReadChunk(reader, settings, actualTrackChunksCount, expectedTrackChunksCount);
                        if (chunk == null)
                            continue;

                        // Process header chunk

                        var headerChunk = chunk as HeaderChunk;
                        if (headerChunk != null)
                        {
                            if (!headerChunkIsRead)
                            {
                                expectedTrackChunksCount = headerChunk.TracksNumber;
                                file.TimeDivision = headerChunk.TimeDivision;
                                file._originalFormat = headerChunk.FileFormat;
                            }

                            headerChunkIsRead = true;
                            continue;
                        }

                        // Process track chunk

                        if (chunk is TrackChunk)
                            actualTrackChunksCount++;

                        // Add chunk to chunks collection of the file

                        file.Chunks.Add(chunk);
                    }

                    if (expectedTrackChunksCount != null && actualTrackChunksCount != expectedTrackChunksCount)
                        ReactOnUnexpectedTrackChunksCount(settings.UnexpectedTrackChunksCountPolicy, actualTrackChunksCount, expectedTrackChunksCount.Value);
                }

                // Process header chunks count

                if (!headerChunkIsRead)
                {
                    file.TimeDivision = null;

                    if (settings.NoHeaderChunkPolicy == NoHeaderChunkPolicy.Abort)
                        throw new NoHeaderChunkException();
                }
            }
            catch (NotEnoughBytesException ex)
            {
                ReactOnNotEnoughBytes(settings.NotEnoughBytesPolicy, ex);
            }
            catch (EndOfStreamException ex)
            {
                ReactOnNotEnoughBytes(settings.NotEnoughBytesPolicy, ex);
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
        /// <exception cref="ArgumentException"><paramref name="stream"/> doesn't support writing.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="format"/> specified an invalid value.</exception>
        /// <exception cref="InvalidOperationException">Time division is null.</exception>
        /// <exception cref="IOException">An I/O error occurred while writing to the stream.</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="stream"/> is disposed. -or-
        /// Underlying stream writer is disposed.</exception>
        /// <exception cref="TooManyTrackChunksException">Count of track chunks presented in the file
        /// exceeds maximum value allowed for MIDI file.</exception>
        public void Write(Stream stream, MidiFileFormat format = MidiFileFormat.MultiTrack, WritingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(stream), stream);
            ThrowIfArgument.IsInvalidEnumValue(nameof(format), format);

            if (TimeDivision == null)
                throw new InvalidOperationException("Time division is null.");

            if (!stream.CanWrite)
                throw new ArgumentException("Stream doesn't support writing.", nameof(stream));

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
        /// Clones MIDI file by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the MIDI file.</returns>
        public MidiFile Clone()
        {
            var result = new MidiFile(Chunks.Select(c => c.Clone()))
            {
                TimeDivision = TimeDivision.Clone()
            };
            result._originalFormat = _originalFormat;

            return result;
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
        /// <exception cref="UnknownChunkException">Chunk to be read has unknown ID and that
        /// should be treated as error accordng to the specified <paramref name="settings"/>.</exception>
        /// <exception cref="UnexpectedTrackChunksCountException">Actual track chunks
        /// count is greater than expected one and that should be treated as error according to
        /// the specified <paramref name="settings"/>.</exception>
        /// <exception cref="InvalidChunkSizeException">Actual chunk's size differs from the one declared
        /// in its header and that should be treated as error according to the specified
        /// <paramref name="settings"/>.</exception>
        /// <exception cref="UnknownChannelEventException">Reader has encountered an unknown channel event.</exception>
        /// <exception cref="NotEnoughBytesException">Value cannot be read since the reader's underlying stream
        /// doesn't have enough bytes.</exception>
        /// <exception cref="UnexpectedRunningStatusException">Unexpected running status is encountered.</exception>
        /// <exception cref="MissedEndOfTrackEventException">Track chunk doesn't end with End Of Track event and that
        /// should be treated as error accordng to the specified <paramref name="settings"/>.</exception>
        /// <exception cref="InvalidChannelEventParameterValueException">Value of a channel event's parameter
        /// just read is invalid.</exception>
        /// <exception cref="InvalidMetaEventParameterValueException">Value of a meta event's parameter
        /// just read is invalid.</exception>
        private static MidiChunk ReadChunk(MidiReader reader, ReadingSettings settings, int actualTrackChunksCount, int? expectedTrackChunksCount)
        {
            MidiChunk chunk = null;

            try
            {
                var chunkId = reader.ReadString(MidiChunk.IdLength);
                if (chunkId.Length < MidiChunk.IdLength)
                {
                    switch (settings.NotEnoughBytesPolicy)
                    {
                        case NotEnoughBytesPolicy.Abort:
                            throw new NotEnoughBytesException("Chunk ID cannot be read since the reader's underlying stream doesn't have enough bytes.",
                                                              MidiChunk.IdLength,
                                                              chunkId.Length);
                        case NotEnoughBytesPolicy.Ignore:
                            return null;
                    }
                }

                //

                switch (chunkId)
                {
                    case HeaderChunk.Id:
                        chunk = new HeaderChunk();
                        break;
                    case TrackChunk.Id:
                        chunk = new TrackChunk();
                        break;
                    default:
                        chunk = TryCreateChunk(chunkId, settings.CustomChunkTypes);
                        break;
                }

                //

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
                            throw new UnknownChunkException($"'{chunkId}' chunk ID is unknown.", chunkId);
                    }
                }

                //

                if (chunk is TrackChunk && expectedTrackChunksCount != null && actualTrackChunksCount >= expectedTrackChunksCount)
                {
                    ReactOnUnexpectedTrackChunksCount(settings.UnexpectedTrackChunksCountPolicy, actualTrackChunksCount, expectedTrackChunksCount.Value);

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

                //

                chunk.Read(reader, settings);
            }
            catch (NotEnoughBytesException ex)
            {
                ReactOnNotEnoughBytes(settings.NotEnoughBytesPolicy, ex);
            }
            catch (EndOfStreamException ex)
            {
                ReactOnNotEnoughBytes(settings.NotEnoughBytesPolicy, ex);
            }

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
        /// count and the actual one is unallowable due to the <paramref name="policy"/>.</exception>
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
        /// Does nothing if lack of bytes in the reader's underlying stream needed to read a value should
        /// not be treated as error; or throws the <see cref="NotEnoughBytesException"/> if this is
        /// unallowable.
        /// </summary>
        /// <param name="policy">The policy according to which the method should operate.</param>
        /// <param name="exception">Initial exception.</param>
        /// <exception cref="NotEnoughBytesException">Lack of bytes in the reader's underlying stream needed to
        /// read a value is unallowable due to the <paramref name="policy"/>.</exception>
        private static void ReactOnNotEnoughBytes(NotEnoughBytesPolicy policy, Exception exception)
        {
            if (policy == NotEnoughBytesPolicy.Abort)
                throw new NotEnoughBytesException("MIDI file cannot be read since the reader's underlying stream doesn't have enough bytes.", exception);
        }

        /// <summary>
        /// Tries to create an instance of a chunk type that has specified ID.
        /// </summary>
        /// <param name="chunkId">ID of the chunk that need to be created.</param>
        /// <param name="chunksTypes">Collection of the chunks types to search for the one with
        /// <paramref name="chunkId"/> ID.</param>
        /// <returns>An instance of the chunk type with the specified ID or null if <paramref name="chunksTypes"/>
        /// doesn't contain chunk type with it.</returns>
        private static MidiChunk TryCreateChunk(string chunkId, ChunkTypesCollection chunksTypes)
        {
            Type type = null;
            return chunksTypes?.TryGetType(chunkId, out type) == true && IsChunkType(type)
                ? (MidiChunk)Activator.CreateInstance(type)
                : null;
        }

        /// <summary>
        /// Checks if a type represents a MIDI-file chunk.
        /// </summary>
        /// <param name="type">Type to check whether it represents a chunk or not.</param>
        /// <returns>True if passed type represents a MIDI-file chunk; false - otherwise.</returns>
        /// <remarks>
        /// Type represents a chunk if it is derived from the <see cref="MidiChunk"/> class and has
        /// parameterless constructor.
        /// </remarks>
        private static bool IsChunkType(Type type)
        {
            return type != null &&
                   type.IsSubclassOf(typeof(MidiChunk)) &&
                   type.GetConstructor(Type.EmptyTypes) != null;
        }

        #endregion
    }
}
