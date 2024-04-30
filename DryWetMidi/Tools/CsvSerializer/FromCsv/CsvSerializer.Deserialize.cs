using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Melanchall.DryWetMidi.Tools
{
    public static partial class CsvSerializer
    {
        #region Enums

        private enum RecordType
        {
            Header,
            Event,
            Note,
        }

        #endregion

        #region Constants

        private static readonly Dictionary<string, RecordType> RecordLabelsToRecordTypes =
            new Dictionary<string, RecordType>(StringComparer.OrdinalIgnoreCase)
            {
                [Record.HeaderType] = RecordType.Header,
                [Record.NoteType] = RecordType.Note,
            };

        private static readonly string[] EventsNames = Enum
            .GetValues(typeof(MidiEventType))
            .Cast<MidiEventType>()
            .Select(t => t.ToString())
            .ToArray();

        #endregion

        #region Methods

        /// <summary>
        /// Reads a <see cref="MidiFile"/> represented as CSV from the specified stream.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> to read the file from.</param>
        /// <param name="settings">Settings according to which <see cref="MidiFile"/> should be deserialized.</param>
        /// <returns>An instance of the <see cref="MidiFile"/> read from CSV representation from
        /// <paramref name="stream"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="stream"/> doesn't support reading.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <c>null</c>.</exception>
        /// <exception cref="FormatException">Invalid CSV representation.</exception>
        public static MidiFile DeserializeFileFromCsv(
            Stream stream,
            CsvSerializationSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(stream), stream);

            if (!stream.CanRead)
                throw new ArgumentException("Stream doesn't support reading.", nameof(stream));

            settings = settings ?? new CsvSerializationSettings();

            var midiFile = new MidiFile();

            using (var reader = new CsvReader(stream, settings))
            {
                var chunks = ReadChunks(
                    reader,
                    settings,
                    (objects, readChunks) => GetTempoMap(objects, readChunks.OfType<HeaderChunk>().First().TimeDivision),
                    true);

                var headerChunk = chunks.OfType<HeaderChunk>().First();

                midiFile.TimeDivision = headerChunk.TimeDivision;
                midiFile.Chunks.AddRange(chunks.Where(c => !(c is HeaderChunk)));
            }

            return midiFile;
        }

        /// <summary>
        /// Reads a <see cref="MidiFile"/> represented as CSV from the specified file.
        /// </summary>
        /// <param name="filePath">Path to the file to read the file from.</param>
        /// <param name="settings">Settings according to which <see cref="MidiFile"/> should be deserialized.</param>
        /// <returns>An instance of the <see cref="MidiFile"/> read from CSV representation from
        /// <paramref name="filePath"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is <c>null</c>, a zero-length string,
        /// contains only white space, or contains one or more invalid characters as defined by
        /// <see cref="Path.InvalidPathChars"/>.</exception>
        /// <exception cref="FormatException">Invalid CSV representation.</exception>
        public static MidiFile DeserializeFileFromCsv(
            string filePath,
            CsvSerializationSettings settings = null)
        {
            ThrowIfArgument.IsNullOrEmptyString(nameof(filePath), filePath, "File path");

            using (var fileStream = FileUtilities.OpenFileForRead(filePath))
            {
                return DeserializeFileFromCsv(fileStream, settings);
            }
        }

        /// <summary>
        /// Reads a collection of <see cref="MidiChunk"/> represented as CSV from the specified stream.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> to read chunks from.</param>
        /// <param name="tempoMap"><see cref="TempoMap"/> to use for time/length conversions.</param>
        /// <param name="settings">Settings according to which collection of <see cref="MidiChunk"/> should
        /// be deserialized.</param>
        /// <returns>Collection of <see cref="MidiChunk"/> read from CSV representation from
        /// <paramref name="stream"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="stream"/> doesn't support reading.</exception>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="stream"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="FormatException">Invalid CSV representation.</exception>
        public static ICollection<MidiChunk> DeserializeChunksFromCsv(
            Stream stream,
            TempoMap tempoMap,
            CsvSerializationSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(stream), stream);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            if (!stream.CanRead)
                throw new ArgumentException("Stream doesn't support reading.", nameof(stream));

            settings = settings ?? new CsvSerializationSettings();

            using (var reader = new CsvReader(stream, settings))
            {
                return ReadChunks(
                    reader,
                    settings,
                    (objects, readChunks) => tempoMap,
                    true);
            }
        }

        /// <summary>
        /// Reads a collection of <see cref="MidiChunk"/> represented as CSV from the specified file.
        /// </summary>
        /// <param name="filePath">Path to the file to read chunks from.</param>
        /// <param name="tempoMap"><see cref="TempoMap"/> to use for time/length conversions.</param>
        /// <param name="settings">Settings according to which collection of <see cref="MidiChunk"/> should
        /// be deserialized.</param>
        /// <returns>Collection of <see cref="MidiChunk"/> read from CSV representation from
        /// <paramref name="filePath"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is <c>null</c>, a zero-length string,
        /// contains only white space, or contains one or more invalid characters as defined by
        /// <see cref="Path.InvalidPathChars"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="tempoMap"/> is <c>null</c>.</exception>
        /// <exception cref="FormatException">Invalid CSV representation.</exception>
        public static ICollection<MidiChunk> DeserializeChunksFromCsv(
            string filePath,
            TempoMap tempoMap,
            CsvSerializationSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNullOrEmptyString(nameof(filePath), filePath, "File path");

            using (var fileStream = FileUtilities.OpenFileForRead(filePath))
            {
                return DeserializeChunksFromCsv(fileStream, tempoMap, settings);
            }
        }

        /// <summary>
        /// Reads a <see cref="MidiChunk"/> represented as CSV from the specified stream.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> to read a chunk from.</param>
        /// <param name="tempoMap"><see cref="TempoMap"/> to use for time/length conversions.</param>
        /// <param name="settings">Settings according to which <see cref="MidiChunk"/> should
        /// be deserialized.</param>
        /// <returns>An instance of the <see cref="MidiChunk"/> read from CSV representation from
        /// <paramref name="stream"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="stream"/> doesn't support reading.</exception>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="stream"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="FormatException">Invalid CSV representation.</exception>
        public static MidiChunk DeserializeChunkFromCsv(
            Stream stream,
            TempoMap tempoMap,
            CsvSerializationSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(stream), stream);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            if (!stream.CanRead)
                throw new ArgumentException("Stream doesn't support reading.", nameof(stream));

            settings = settings ?? new CsvSerializationSettings();

            using (var reader = new CsvReader(stream, settings))
            {
                var chunks = ReadChunks(
                    reader,
                    settings,
                    (objects, readChunks) => tempoMap,
                    true);

                if (chunks.Count > 1)
                    CsvError.ThrowBadFormat("More than one chunk.");

                return chunks.First();
            }
        }

        /// <summary>
        /// Reads a <see cref="MidiChunk"/> represented as CSV from the specified file.
        /// </summary>
        /// <param name="filePath">Path to the file to read a chunk from.</param>
        /// <param name="tempoMap"><see cref="TempoMap"/> to use for time/length conversions.</param>
        /// <param name="settings">Settings according to which <see cref="MidiChunk"/> should
        /// be deserialized.</param>
        /// <returns>An instance of the <see cref="MidiChunk"/> read from CSV representation from
        /// <paramref name="filePath"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is <c>null</c>, a zero-length string,
        /// contains only white space, or contains one or more invalid characters as defined by
        /// <see cref="Path.InvalidPathChars"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="tempoMap"/> is <c>null</c>.</exception>
        /// <exception cref="FormatException">Invalid CSV representation.</exception>
        public static MidiChunk DeserializeChunkFromCsv(
            string filePath,
            TempoMap tempoMap,
            CsvSerializationSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNullOrEmptyString(nameof(filePath), filePath, "File path");

            using (var fileStream = FileUtilities.OpenFileForRead(filePath))
            {
                return DeserializeChunkFromCsv(fileStream, tempoMap, settings);
            }
        }

        /// <summary>
        /// Reads collection of <see cref="ITimedObject"/> represented as CSV from the specified stream.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> to read objects from.</param>
        /// <param name="tempoMap"><see cref="TempoMap"/> to use for time/length conversions.</param>
        /// <param name="settings">Settings according to which collection of <see cref="ITimedObject"/> should
        /// be deserialized.</param>
        /// <returns>Collection of <see cref="ITimedObject"/> read from CSV representation from
        /// <paramref name="stream"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="stream"/> doesn't support reading.</exception>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="stream"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="FormatException">Invalid CSV representation.</exception>
        public static ICollection<ITimedObject> DeserializeObjectsFromCsv(
            Stream stream,
            TempoMap tempoMap,
            CsvSerializationSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(stream), stream);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            if (!stream.CanRead)
                throw new ArgumentException("Stream doesn't support reading.", nameof(stream));

            settings = settings ?? new CsvSerializationSettings();

            using (var reader = new CsvReader(stream, settings))
            {
                var objects = ReadObjects(
                    reader,
                    settings,
                    tempoMap,
                    false);

                if (objects.Count > 1)
                    CsvError.ThrowBadFormat("More than one chunk.");

                return objects.First();
            }
        }

        /// <summary>
        /// Reads collection of <see cref="ITimedObject"/> represented as CSV from the specified file.
        /// </summary>
        /// <param name="filePath">Path to the file to read objects from.</param>
        /// <param name="tempoMap"><see cref="TempoMap"/> to use for time/length conversions.</param>
        /// <param name="settings">Settings according to which collection of <see cref="ITimedObject"/> should
        /// be deserialized.</param>
        /// <returns>Collection of <see cref="ITimedObject"/> read from CSV representation from
        /// <paramref name="filePath"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is <c>null</c>, a zero-length string,
        /// contains only white space, or contains one or more invalid characters as defined by
        /// <see cref="Path.InvalidPathChars"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="tempoMap"/> is <c>null</c>.</exception>
        /// <exception cref="FormatException">Invalid CSV representation.</exception>
        public static ICollection<ITimedObject> DeserializeObjectsFromCsv(
            string filePath,
            TempoMap tempoMap,
            CsvSerializationSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNullOrEmptyString(nameof(filePath), filePath, "File path");

            using (var fileStream = FileUtilities.OpenFileForRead(filePath))
            {
                return DeserializeObjectsFromCsv(fileStream, tempoMap, settings);
            }
        }

        private static ICollection<MidiChunk> ReadChunks(
            CsvReader reader,
            CsvSerializationSettings settings,
            Func<ICollection<CsvObject>, ICollection<MidiChunk>, TempoMap> getTempoMap,
            bool readChunkId)
        {
            var result = new List<MidiChunk>();

            var objects = new List<CsvObject>();
            var chords = new Dictionary<Tuple<int?, int?>, CsvChord>();

            Record record;

            while ((record = ReadRecord(reader, readChunkId)) != null)
            {
                var lineNumber = record.CsvRecord.LineNumber;

                var recordType = GetRecordType(record.RecordType);
                if (recordType == null)
                    CsvError.ThrowBadFormat(lineNumber, "Unknown record.");

                if (readChunkId && record.ChunkId != TrackChunk.Id && record.ChunkId != HeaderChunk.Id)
                    continue;

                switch (recordType)
                {
                    case RecordType.Header:
                        {
                            var headerChunk = ParseHeader(record);
                            result.Add(headerChunk);
                        }
                        break;
                    case RecordType.Event:
                        {
                            var csvEvent = ParseEvent(record, settings, readChunkId);
                            objects.Add(csvEvent);
                        }
                        break;
                    case RecordType.Note:
                        {
                            var csvNote = ParseNote(record, settings, readChunkId);
                            var id = Tuple.Create(csvNote.ChunkIndex, csvNote.ObjectIndex);

                            CsvChord csvChord;
                            if (!chords.TryGetValue(id, out csvChord))
                            {
                                chords.Add(
                                    id,
                                    csvChord = new CsvChord(csvNote.ChunkIndex, csvNote.ChunkId, csvNote.ObjectIndex));

                                objects.Add(csvChord);
                            }

                            csvChord.Notes.Add(csvNote);
                        }
                        break;
                }
            }

            if (!objects.Any())
                return result;

            var tempoMap = getTempoMap(objects, result);
            var timedObjects = GetTimedObjects(objects, tempoMap);

            result.AddRange(timedObjects
                .Select(obj => obj.ToTrackChunk())
                .ToArray());

            return result;
        }

        private static ICollection<ICollection<ITimedObject>> ReadObjects(
            CsvReader reader,
            CsvSerializationSettings settings,
            TempoMap tempoMap,
            bool readChunkId)
        {
            var objects = new List<CsvObject>();
            var chords = new Dictionary<Tuple<int?, int?>, CsvChord>();

            Record record;

            while ((record = ReadRecord(reader, readChunkId)) != null)
            {
                var lineNumber = record.CsvRecord.LineNumber;

                var recordType = GetRecordType(record.RecordType);
                if (recordType == null)
                    CsvError.ThrowBadFormat(lineNumber, "Unknown record.");

                switch (recordType)
                {
                    case RecordType.Event:
                        {
                            var csvEvent = ParseEvent(record, settings, readChunkId);
                            objects.Add(csvEvent);
                        }
                        break;
                    case RecordType.Note:
                        {
                            var csvNote = ParseNote(record, settings, readChunkId);

                            CsvChord csvChord;
                            if (!chords.TryGetValue(Tuple.Create(csvNote.ChunkIndex, csvNote.ObjectIndex), out csvChord))
                            {
                                chords.Add(
                                    Tuple.Create(csvNote.ChunkIndex, csvNote.ObjectIndex),
                                    csvChord = new CsvChord(csvNote.ChunkIndex, csvNote.ChunkId, csvNote.ObjectIndex));

                                objects.Add(csvChord);
                            }

                            csvChord.Notes.Add(csvNote);
                        }
                        break;
                }
            }

            if (!objects.Any())
                return new ITimedObject[0][];

            return GetTimedObjects(objects, tempoMap);
        }

        private static ICollection<ICollection<ITimedObject>> GetTimedObjects(
            ICollection<CsvObject> objects,
            TempoMap tempoMap)
        {
            return objects
                .GroupBy(obj => obj.ChunkIndex)
                .Select(g => g
                    .Select(obj =>
                    {
                        var csvEvent = obj as CsvEvent;
                        if (csvEvent != null)
                            return new TimedEvent(csvEvent.Event).SetTime(csvEvent.Time, tempoMap);

                        var csvChord = obj as CsvChord;
                        if (csvChord != null)
                        {
                            var notes = csvChord
                                .Notes
                                .Select(n => new Note(n.NoteNumber) { Channel = n.Channel, Velocity = n.Velocity, OffVelocity = n.OffVlocity }.SetTime(n.Time, tempoMap).SetLength(n.Length, tempoMap))
                                .ToArray();

                            if (notes.Length == 1)
                                return notes.First();

                            return new Chord(notes);
                        }

                        return (ITimedObject)null;
                    })
                    .Where(obj => obj != null)
                    .ToArray())
                .ToArray();
        }

        private static Record ReadRecord(
            CsvReader csvReader,
            bool readChunkId)
        {
            var record = csvReader.ReadRecord();
            if (record == null)
                return null;

            var requiredPartsCount = readChunkId ? 4 : 2;

            var values = record.Values;
            if (values.Length < requiredPartsCount)
                CsvError.ThrowBadFormat(record.LineNumber, "Missing required parameters.");

            int? chunkIndex = null;
            string chunkId = null;

            if (readChunkId)
            {
                int parsedChunkIndex;
                chunkIndex = int.TryParse(values[0], out parsedChunkIndex)
                    ? (int?)parsedChunkIndex
                    : null;

                chunkId = values[1];
                if (string.IsNullOrEmpty(chunkId))
                    CsvError.ThrowBadFormat(record.LineNumber, "Chunk ID isn't specified.");
            }

            int parsedObjectIndex;
            var objectIndex = int.TryParse(values[readChunkId ? 2 : 0], out parsedObjectIndex)
                ? (int?)parsedObjectIndex
                : null;

            var recordType = values[readChunkId ? 3 : 1];
            if (string.IsNullOrEmpty(recordType))
                CsvError.ThrowBadFormat(record.LineNumber, "Record type isn't specified.");

            var parameters = values.Skip(requiredPartsCount).ToArray();

            return new Record(record, chunkIndex, chunkId, objectIndex, recordType, parameters);
        }

        private static RecordType? GetRecordType(string recordType)
        {
            RecordType result;
            if (RecordLabelsToRecordTypes.TryGetValue(recordType, out result))
                return result;

            if (EventsNames.Contains(recordType, StringComparer.OrdinalIgnoreCase))
                return RecordType.Event;

            return null;
        }

        private static HeaderChunk ParseHeader(Record record)
        {
            var parameters = record.Parameters;

            if (parameters.Length < 1)
                CsvError.ThrowBadFormat(record.CsvRecord.LineNumber, "Parameters count is invalid.");

            var timeDivision = default(short);
            if (!short.TryParse(parameters[0], out timeDivision))
                CsvError.ThrowBadFormat(record.CsvRecord.LineNumber, "Invalid time division.");

            return new HeaderChunk
            {
                TimeDivision = TimeDivisionFactory.GetTimeDivision(timeDivision)
            };
        }

        private static CsvEvent ParseEvent(
            Record record,
            CsvSerializationSettings settings,
            bool parseChunkId)
        {
            // TODO
            //if (record.TrackNumber == null)
            //    CsvError.ThrowBadFormat(record.CsvRecord.LineNumber, "Invalid track number.");
            //
            //if (record.Time == null)
            //    CsvError.ThrowBadFormat(record.CsvRecord.LineNumber, "Invalid time.");

            ITimeSpan time;
            TimeSpanUtilities.TryParse(record.Parameters.First(), settings.TimeType, out time);

            if (time == null)
                CsvError.ThrowBadFormat(record.CsvRecord.LineNumber, "Invalid time.");

            try
            {
                var midiEvent = EventParser.ParseEvent(
                    (MidiEventType)Enum.Parse(typeof(MidiEventType), record.RecordType),
                    record.Parameters.Skip(1).ToArray(),
                    settings);
                return new CsvEvent(midiEvent, record.ChunkIndex, record.ChunkId, record.ObjectIndex, time);
            }
            catch (FormatException ex)
            {
                CsvError.ThrowBadFormat(record.CsvRecord.LineNumber, "Invalid format of event record.", ex);
                return null;
            }
        }

        private static CsvNote ParseNote(
            Record record,
            CsvSerializationSettings settings,
            bool parseChunkId)
        {
            // TODO
            //if (record.TrackNumber == null)
            //    CsvError.ThrowBadFormat(record.CsvRecord.LineNumber, "Invalid track number.");
            //
            //if (record.Time == null)
            //    CsvError.ThrowBadFormat(record.CsvRecord.LineNumber, "Invalid time.");

            var parameters = record.Parameters;
            if (parameters.Length < 6)
                CsvError.ThrowBadFormat(record.CsvRecord.LineNumber, "Invalid number of parameters provided.");

            ITimeSpan time;
            TimeSpanUtilities.TryParse(record.Parameters.First(), settings.TimeType, out time);

            if (time == null)
                CsvError.ThrowBadFormat(record.CsvRecord.LineNumber, "Invalid time.");

            ITimeSpan length;
            TimeSpanUtilities.TryParse(parameters[1], settings.LengthType, out length);

            if (length == null)
                CsvError.ThrowBadFormat(record.CsvRecord.LineNumber, "Invalid length.");

            var channel = (FourBitNumber)TypeParser.FourBitNumber(parameters[2], settings);

            var noteNumber = (SevenBitNumber)TypeParser.NoteNumber(parameters[3], settings);

            var velocity = (SevenBitNumber)TypeParser.SevenBitNumber(parameters[4], settings);
            var offVelocity = (SevenBitNumber)TypeParser.SevenBitNumber(parameters[5], settings);

            return new CsvNote(noteNumber, velocity, offVelocity, channel, length, record.ChunkIndex, record.ChunkId, record.ObjectIndex, time);
        }

        private static TempoMap GetTempoMap(ICollection<CsvObject> objects, TimeDivision timeDivision)
        {
            using (var tempoMapManager = new TempoMapManager(timeDivision))
            {
                var setTempoEvents = objects
                    .OfType<CsvEvent>()
                    .Where(e => e.Event is SetTempoEvent)
                    .OrderBy(e => e.Time, new TimeSpanComparer());

                foreach (var csvEvent in setTempoEvents)
                {
                    var setTempoEvent = (SetTempoEvent)csvEvent.Event;
                    tempoMapManager.SetTempo(
                        csvEvent.Time,
                        new Tempo(setTempoEvent.MicrosecondsPerQuarterNote));
                }

                var timeSignatureEvents = objects
                    .OfType<CsvEvent>()
                    .Where(e => e.Event is TimeSignatureEvent)
                    .OrderBy(e => e.Time, new TimeSpanComparer());

                foreach (var csvEvent in timeSignatureEvents)
                {
                    var timeSignatureEvent = (TimeSignatureEvent)csvEvent.Event;
                    tempoMapManager.SetTimeSignature(
                        csvEvent.Time,
                        new TimeSignature(timeSignatureEvent.Numerator, timeSignatureEvent.Denominator));
                }

                return tempoMapManager.TempoMap;
            }
        }

        #endregion
    }
}
