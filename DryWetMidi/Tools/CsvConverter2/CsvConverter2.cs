using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tools
{
    public static class CsvConverter2
    {
        #region Methods

        public static void ConvertToCsv(this MidiFile midiFile, ObjectType objectType, ObjectDetectionSettings objectDetectionSettings)
        {
            // header chunk

            var chunkIndex = 0;

            var writer = new CsvWriter2(null);

            foreach (var chunk in midiFile.Chunks)
            {
                WriteChunk(chunk, writer, objectType, objectDetectionSettings, chunkIndex++);
            }
        }

        public static void ConvertToCsv(this IEnumerable<TrackChunk> trackChunks, ObjectType objectType, ObjectDetectionSettings objectDetectionSettings)
        {
            var chunkIndex = 0;

            var writer = new CsvWriter2(null);

            foreach (var trackChunk in trackChunks)
            {
                WriteTrackChunk(trackChunk, writer, objectType, objectDetectionSettings, chunkIndex++);
            }
        }

        public static void ConvertToCsv(this TrackChunk trackChunk, ObjectType objectType, ObjectDetectionSettings objectDetectionSettings)
        {
            var writer = new CsvWriter2(null);

            WriteTrackChunk(trackChunk, writer, objectType, objectDetectionSettings, 0);
        }

        public static void ConvertToCsv(this IEnumerable<ITimedObject> timedObjects)
        {
            var writer = new CsvWriter2(null);

            WriteObjects(timedObjects, writer, null, null);
        }

        private static void WriteChunk(MidiChunk midiChunk, CsvWriter2 writer, ObjectType objectType, ObjectDetectionSettings objectDetectionSettings, int chunkIndex)
        {
            var trackChunk = midiChunk as TrackChunk;
            if (trackChunk != null)
            {
                WriteTrackChunk(trackChunk, writer, objectType, objectDetectionSettings, chunkIndex);
                return;
            }

            var unknownChunk = midiChunk as UnknownChunk;
            if (unknownChunk != null)
            {
                WriteUnknownChunk(unknownChunk, writer, chunkIndex);
                return;
            }

            WriteCustomChunk(midiChunk, writer, chunkIndex);
        }

        private static void WriteTrackChunk(TrackChunk trackChunk, CsvWriter2 writer, ObjectType objectType, ObjectDetectionSettings objectDetectionSettings, int chunkIndex)
        {
            WriteObjects(trackChunk.GetObjects(objectType, objectDetectionSettings), writer, chunkIndex, TrackChunk.Id);
        }

        private static void WriteUnknownChunk(UnknownChunk unknownChunk, CsvWriter2 writer, int chunkIndex)
        {
        }

        private static void WriteCustomChunk(MidiChunk midiChunk, CsvWriter2 writer, int chunkIndex)
        {
        }

        private static void WriteObjects(IEnumerable<ITimedObject> timedObjects, CsvWriter2 writer, int? chunkIndex, string chunkId)
        {
            var objectIndex = 0;

            foreach (var obj in timedObjects)
            {
                WriteObject(obj, writer, objectIndex++, chunkIndex, chunkId);
            }
        }

        private static void WriteObject(ITimedObject timedObject, CsvWriter2 writer, int objectIndex, int? chunkIndex, string chunkId)
        {
            var timedEvent = timedObject as TimedEvent;
            if (timedEvent != null)
            {
                WriteTimedEvent(timedEvent, writer, objectIndex, chunkIndex, chunkId);
                return;
            }

            var note = timedObject as Note;
            if (note != null)
            {
                WriteNote(note, writer, objectIndex, chunkIndex, chunkId);
                return;
            }

            var chord = timedObject as Chord;
            if (chord != null)
            {
                WriteChord(chord, writer, objectIndex, chunkIndex, chunkId);
                return;
            }

            var registeredParameter = timedObject as RegisteredParameter;
            if (registeredParameter != null)
            {
                WriteRegisteredParameter(registeredParameter, writer, objectIndex, chunkIndex, chunkId);
                return;
            }

            var rest = timedObject as Rest;
            if (rest != null)
                return;

            WriteCustomObject(timedObject, writer, objectIndex, chunkIndex, chunkId);
        }

        private static void WriteRegisteredParameter(RegisteredParameter registeredParameter, CsvWriter2 writer, int objectIndex, int? chunkIndex, string chunkId)
        {
            foreach (var timedEvent in registeredParameter.GetTimedEvents())
            {
                WriteTimedEvent(timedEvent, writer, objectIndex, chunkIndex, chunkId);
            }
        }

        private static void WriteTimedEvent(TimedEvent timedEvent, CsvWriter2 writer, int objectIndex, int? chunkIndex, string chunkId)
        {
            var midiEvent = timedEvent.Event;
            var eventType = midiEvent.GetType();

            var eventNameGetter = EventNameGetterProvider.Get(eventType);
            var recordType = eventNameGetter(midiEvent);

            var eventParametersGetter = EventParametersGetterProvider.Get(eventType);
            var recordParameters = eventParametersGetter(midiEvent, null); // settings

            var processedParameters = recordParameters.SelectMany(ProcessParameter);

            writer.WriteRecord(
                new object[]
                {
                    chunkIndex,
                    chunkId,
                    objectIndex,
                    "Event",
                    timedEvent.Time, // format
                    timedEvent.Event.EventType
                }
                .Concat(processedParameters));
        }

        private static void WriteNote(Note note, CsvWriter2 writer, int objectIndex, int? chunkIndex, string chunkId)
        {
            writer.WriteRecord(
                chunkIndex,
                chunkId,
                objectIndex,
                "Note",
                note.Time, // format
                note.NoteNumber, // format
                note.Length, // format
                note.Velocity,
                note.OffVelocity);
        }

        private static void WriteChord(Chord chord, CsvWriter2 writer, int objectIndex, int? chunkIndex, string chunkId)
        {
            foreach (var note in chord.Notes)
            {
                WriteNote(note, writer, objectIndex, chunkIndex, chunkId);
            }
        }

        private static void WriteCustomObject(ITimedObject timedObject, CsvWriter2 writer, int objectIndex, int? chunkIndex, string chunkId)
        {
        }

        private static object[] ProcessParameter(object parameter)
        {
            if (parameter == null)
                return new object[] { string.Empty };

            var bytes = parameter as byte[];
            if (bytes != null)
                return bytes.OfType<object>().ToArray();

            var s = parameter as string;
            if (s != null)
                parameter = CsvUtilities.EscapeString(s);

            return new[] { parameter };
        }

        #endregion
    }
}
