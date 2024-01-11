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
        #region Methods

        public static void SerializeToCsv(
            this MidiFile midiFile,
            Stream stream,
            CsvSerializationSettings settings = null,
            ObjectType objectType = ObjectType.TimedEvent,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(stream), stream);

            if (!stream.CanWrite)
                throw new ArgumentException("Stream doesn't support writing.", nameof(stream));

            settings = settings ?? new CsvSerializationSettings();
            objectDetectionSettings = objectDetectionSettings ?? new ObjectDetectionSettings();

            var chunkIndex = 0;
            var tempoMap = midiFile.GetTempoMap();

            using (var writer = new CsvWriter(stream, settings))
            {
                WriteHeaderChunk(midiFile, writer, settings, chunkIndex++);

                foreach (var chunk in midiFile.Chunks)
                {
                    WriteChunk(chunk, writer, settings, tempoMap, objectType, objectDetectionSettings, chunkIndex++);
                }
            }
        }

        public static void SerializeToCsv(
            this MidiFile midiFile,
            string filePath,
            bool overwriteFile,
            CsvSerializationSettings settings = null,
            ObjectType objectType = ObjectType.TimedEvent,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNullOrEmptyString(nameof(filePath), filePath, "File path");

            using (var fileStream = FileUtilities.OpenFileForWrite(filePath, overwriteFile))
            {
                midiFile.SerializeToCsv(fileStream, settings, objectType, objectDetectionSettings);
            }
        }

        public static void SerializeToCsv(
            this IEnumerable<MidiChunk> midiChunks,
            Stream stream,
            TempoMap tempoMap,
            CsvSerializationSettings settings = null,
            ObjectType objectType = ObjectType.TimedEvent,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiChunks), midiChunks);
            ThrowIfArgument.IsNull(nameof(stream), stream);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            if (!stream.CanWrite)
                throw new ArgumentException("Stream doesn't support writing.", nameof(stream));

            settings = settings ?? new CsvSerializationSettings();
            objectDetectionSettings = objectDetectionSettings ?? new ObjectDetectionSettings();

            var chunkIndex = 0;

            using (var writer = new CsvWriter(stream, settings))
            {
                foreach (var midiChunk in midiChunks)
                {
                    WriteChunk(midiChunk, writer, settings, tempoMap, objectType, objectDetectionSettings, chunkIndex++);
                }
            }
        }

        public static void SerializeToCsv(
            this IEnumerable<MidiChunk> midiChunks,
            string filePath,
            bool overwriteFile,
            TempoMap tempoMap,
            CsvSerializationSettings settings = null,
            ObjectType objectType = ObjectType.TimedEvent,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiChunks), midiChunks);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNullOrEmptyString(nameof(filePath), filePath, "File path");

            using (var fileStream = FileUtilities.OpenFileForWrite(filePath, overwriteFile))
            {
                midiChunks.SerializeToCsv(fileStream, tempoMap, settings, objectType, objectDetectionSettings);
            }
        }

        public static void SerializeToCsv(
            this MidiChunk midiChunk,
            Stream stream,
            TempoMap tempoMap,
            CsvSerializationSettings settings = null,
            ObjectType objectType = ObjectType.TimedEvent,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiChunk), midiChunk);
            ThrowIfArgument.IsNull(nameof(stream), stream);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            if (!stream.CanWrite)
                throw new ArgumentException("Stream doesn't support writing.", nameof(stream));

            settings = settings ?? new CsvSerializationSettings();
            objectDetectionSettings = objectDetectionSettings ?? new ObjectDetectionSettings();

            using (var writer = new CsvWriter(stream, settings))
            {
                WriteChunk(midiChunk, writer, settings, tempoMap, objectType, objectDetectionSettings, 0);
            }
        }

        public static void SerializeToCsv(
            this MidiChunk midiChunk,
            string filePath,
            bool overwriteFile,
            TempoMap tempoMap,
            CsvSerializationSettings settings = null,
            ObjectType objectType = ObjectType.TimedEvent,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiChunk), midiChunk);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNullOrEmptyString(nameof(filePath), filePath, "File path");

            using (var fileStream = FileUtilities.OpenFileForWrite(filePath, overwriteFile))
            {
                midiChunk.SerializeToCsv(fileStream, tempoMap, settings, objectType, objectDetectionSettings);
            }
        }

        public static void SerializeToCsv(
            this IEnumerable<ITimedObject> timedObjects,
            Stream stream,
            TempoMap tempoMap,
            CsvSerializationSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(timedObjects), timedObjects);
            ThrowIfArgument.IsNull(nameof(stream), stream);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            if (!stream.CanWrite)
                throw new ArgumentException("Stream doesn't support writing.", nameof(stream));

            settings = settings ?? new CsvSerializationSettings();

            using (var writer = new CsvWriter(stream, settings))
            {
                WriteObjects(timedObjects, writer, settings, tempoMap, null, null);
            }
        }

        public static void SerializeToCsv(
            this IEnumerable<ITimedObject> timedObjects,
            string filePath,
            bool overwriteFile,
            TempoMap tempoMap,
            CsvSerializationSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(timedObjects), timedObjects);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNullOrEmptyString(nameof(filePath), filePath, "File path");

            using (var fileStream = FileUtilities.OpenFileForWrite(filePath, overwriteFile))
            {
                timedObjects.SerializeToCsv(fileStream, tempoMap, settings);
            }
        }

        private static void WriteChunk(
            MidiChunk midiChunk,
            CsvWriter writer,
            CsvSerializationSettings settings,
            TempoMap tempoMap,
            ObjectType objectType,
            ObjectDetectionSettings objectDetectionSettings,
            int chunkIndex)
        {
            var trackChunk = midiChunk as TrackChunk;
            if (trackChunk != null)
            {
                WriteTrackChunk(trackChunk, writer, settings, tempoMap, objectType, objectDetectionSettings, chunkIndex);
                return;
            }

            var unknownChunk = midiChunk as UnknownChunk;
            if (unknownChunk != null)
            {
                WriteUnknownChunk(unknownChunk, writer, settings, chunkIndex);
                return;
            }

            WriteCustomChunk(midiChunk, writer, settings, chunkIndex);
        }

        private static void WriteHeaderChunk(
            MidiFile midiFile,
            CsvWriter writer,
            CsvSerializationSettings settings,
            int chunkIndex)
        {
            writer.WriteRecord(
                chunkIndex,
                HeaderChunk.Id,
                0,
                Record.HeaderType,
                midiFile.TimeDivision.ToInt16());
        }

        private static void WriteTrackChunk(
            TrackChunk trackChunk,
            CsvWriter writer,
            CsvSerializationSettings settings,
            TempoMap tempoMap,
            ObjectType objectType,
            ObjectDetectionSettings objectDetectionSettings,
            int chunkIndex)
        {
            WriteObjects(
                trackChunk.GetObjects(objectType, objectDetectionSettings),
                writer,
                settings,
                tempoMap,
                chunkIndex,
                TrackChunk.Id);
        }

        private static void WriteUnknownChunk(
            UnknownChunk unknownChunk,
            CsvWriter writer,
            CsvSerializationSettings settings,
            int chunkIndex)
        {
            // TODO: WriteUnknownChunk
        }

        private static void WriteCustomChunk(
            MidiChunk midiChunk,
            CsvWriter writer,
            CsvSerializationSettings settings,
            int chunkIndex)
        {
            // TODO: WriteCustomChunk
        }

        private static void WriteObjects(
            IEnumerable<ITimedObject> timedObjects,
            CsvWriter writer,
            CsvSerializationSettings settings,
            TempoMap tempoMap,
            int? chunkIndex,
            string chunkId)
        {
            var objectIndex = 0;

            foreach (var obj in timedObjects)
            {
                WriteObject(obj, writer, settings, tempoMap, chunkIndex, chunkId, objectIndex++);
            }
        }

        private static void WriteObject(
            ITimedObject timedObject,
            CsvWriter writer,
            CsvSerializationSettings settings,
            TempoMap tempoMap,
            int? chunkIndex,
            string chunkId,
            int objectIndex)
        {
            var timedEvent = timedObject as TimedEvent;
            if (timedEvent != null)
            {
                WriteTimedEvent(timedEvent, writer, settings, tempoMap, chunkIndex, chunkId, objectIndex);
                return;
            }

            var note = timedObject as Note;
            if (note != null)
            {
                WriteNote(note, writer, settings, tempoMap, chunkIndex, chunkId, objectIndex);
                return;
            }

            var chord = timedObject as Chord;
            if (chord != null)
            {
                WriteChord(chord, writer, settings, tempoMap, chunkIndex, chunkId, objectIndex);
                return;
            }

            var registeredParameter = timedObject as RegisteredParameter;
            if (registeredParameter != null)
            {
                WriteRegisteredParameter(registeredParameter, writer, settings, tempoMap, chunkIndex, chunkId, objectIndex);
                return;
            }

            var rest = timedObject as Rest;
            if (rest != null)
                return;

            WriteCustomObject(timedObject, writer, settings, chunkIndex, chunkId, objectIndex);
        }

        private static void WriteRegisteredParameter(
            RegisteredParameter registeredParameter,
            CsvWriter writer,
            CsvSerializationSettings settings,
            TempoMap tempoMap,
            int? chunkIndex,
            string chunkId,
            int objectIndex)
        {
            foreach (var timedEvent in registeredParameter.GetTimedEvents())
            {
                WriteTimedEvent(timedEvent, writer, settings, tempoMap, chunkIndex, chunkId, objectIndex);
            }
        }

        private static void WriteTimedEvent(
            TimedEvent timedEvent,
            CsvWriter writer,
            CsvSerializationSettings settings,
            TempoMap tempoMap,
            int? chunkIndex,
            string chunkId,
            int objectIndex)
        {
            var midiEvent = timedEvent.Event;
            if (midiEvent is EndOfTrackEvent)
                return;

            var eventParameters = EventParametersProvider.GetEventParameters(midiEvent, settings);

            WriteObjectRecord(
                writer,
                settings,
                tempoMap,
                chunkIndex,
                chunkId,
                objectIndex,
                timedEvent,
                eventParameters);
        }

        private static void WriteNote(
            Note note,
            CsvWriter writer,
            CsvSerializationSettings settings,
            TempoMap tempoMap,
            int? chunkIndex,
            string chunkId,
            int objectIndex)
        {
            WriteObjectRecord(
                writer,
                settings,
                tempoMap,
                chunkIndex,
                chunkId,
                objectIndex,
                note,
                CsvFormattingUtilities.FormatLength(note, settings.LengthType, tempoMap),
                note.Channel,
                CsvFormattingUtilities.FormatNoteNumber(note.NoteNumber, settings.NoteNumberFormat),
                note.Velocity,
                note.OffVelocity);
        }

        private static void WriteChord(
            Chord chord,
            CsvWriter writer,
            CsvSerializationSettings settings,
            TempoMap tempoMap,
            int? chunkIndex,
            string chunkId,
            int objectIndex)
        {
            foreach (var note in chord.Notes)
            {
                WriteNote(note, writer, settings, tempoMap, chunkIndex, chunkId, objectIndex);
            }
        }

        private static void WriteCustomObject(
            ITimedObject timedObject,
            CsvWriter writer,
            CsvSerializationSettings settings,
            int? chunkIndex,
            string chunkId,
            int objectIndex)
        {
            // TODO: WriteCustomObject
        }

        private static void WriteObjectRecord(
            CsvWriter writer,
            CsvSerializationSettings settings,
            TempoMap tempoMap,
            int? chunkIndex,
            string chunkId,
            int objectIndex,
            ITimedObject obj,
            params object[] values)
        {
            writer.WriteRecord(
                new object[]
                {
                    chunkIndex,
                    chunkId,
                    objectIndex,
                    obj is TimedEvent ? ((TimedEvent)obj).Event.EventType.ToString() : Record.NoteType,
                    CsvFormattingUtilities.FormatTime(obj, settings.TimeType, tempoMap)
                }
                .Where(v => v != null)
                .Concat(values));
        }

        #endregion
    }
}
