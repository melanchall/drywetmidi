using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Melanchall.DryMidi
{
    public sealed class MidiFile
    {
        #region Properties

        public TimeDivision TimeDivision { get; set; }

        public ChunksCollection Chunks { get; } = new ChunksCollection();

        #endregion

        #region Methods

        public static MidiFile Load(string filePath, ReadingSettings settings = null)
        {
            using (var fileStream = File.OpenRead(filePath))
            {
                return Read(fileStream, settings);
            }
        }

        public void Save(string filePath, bool overwriteFile = false, MidiFileFormat format = MidiFileFormat.MultiTrack, WritingSettings settings = null)
        {
            using (var fileStream = File.Open(filePath, overwriteFile ? FileMode.Create : FileMode.CreateNew))
            {
                Write(fileStream, format, settings);
            }
        }

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

                ProcessUnexpectedTrackChunksCount(settings.UnexpectedTrackChunksCountPolicy, actualTrackChunksCount, expectedTrackChunksCount);
            }

            //

            return file;
        }

        public void Write(Stream stream, MidiFileFormat format = MidiFileFormat.MultiTrack, WritingSettings settings = null)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

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

        private static HeaderChunk ReadHeaderChunk(MidiReader reader, ReadingSettings settings)
        {
            var chunkId = reader.ReadString(Chunk.IdLength);
            if (chunkId != HeaderChunk.Id)
                throw new NoHeaderChunkException($"'{chunkId}' is invalid header chunk's ID. It must be '{HeaderChunk.Id}'.");

            var headerChunk = new HeaderChunk();
            headerChunk.Read(reader, settings);
            return headerChunk;
        }

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
                ProcessUnexpectedTrackChunksCount(settings.UnexpectedTrackChunksCountPolicy, actualTrackChunksCount, expectedTrackChunksCount);

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

        private static void ProcessUnexpectedTrackChunksCount(UnexpectedTrackChunksCountPolicy policy, int actualTrackChunksCount, int expectedTrackChunksCount)
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

        private static Chunk TryCreateChunk(string chunkId, IEnumerable<Type> chunksTypes)
        {
            if (chunksTypes == null || !chunksTypes.Any())
                return null;

            return chunksTypes.Where(IsTypeRepresentCustomChunk)
                              .Select(Activator.CreateInstance)
                              .OfType<Chunk>()
                              .Where(c => c.ChunkId == chunkId)
                              .FirstOrDefault();
        }

        private static bool IsTypeRepresentCustomChunk(Type type)
        {
            return type != null && type.IsSubclassOf(typeof(Chunk)) && type.GetConstructor(Type.EmptyTypes) != null;
        }

        #endregion
    }
}
