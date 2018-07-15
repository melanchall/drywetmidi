using System.IO;
using System.Linq;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    internal static class MidiFileToCsvConverter
    {
        #region Methods

        public static void ConvertToCsv(MidiFile midiFile, Stream stream, MidiFileCsvConversionSettings settings)
        {
            using (var streamWriter = new StreamWriter(stream))
            {
                var trackNumber = 0;
                var tempoMap = midiFile.GetTempoMap();

                WriteHeader(streamWriter, midiFile, settings, tempoMap);

                foreach (var trackChunk in midiFile.GetTrackChunks())
                {
                    WriteTrackChunkStart(streamWriter, trackNumber, settings, tempoMap);

                    var time = 0L;
                    var timedEvents = trackChunk.GetTimedEvents();
                    var timedObjects = settings.CsvLayout == MidiFileCsvLayout.MidiCsv || settings.NoteSettings.NoteFormat == NoteFormat.Events
                        ? timedEvents
                        : timedEvents.GetTimedEventsAndNotes();

                    foreach (var timedObject in timedObjects)
                    {
                        time = timedObject.Time;

                        var timedEvent = timedObject as TimedEvent;
                        if (timedEvent != null)
                            WriteTimedEvent(timedEvent, streamWriter, trackNumber, time, settings, tempoMap);
                        else
                        {
                            var note = timedObject as Note;
                            if (note != null)
                                WriteNote(note, streamWriter, trackNumber, time, settings, tempoMap);
                        }
                    }

                    WriteTrackChunkEnd(streamWriter, trackNumber, time, settings, tempoMap);

                    trackNumber++;
                }

                WriteFileEnd(streamWriter, settings, tempoMap);
            }
        }

        private static void WriteNote(Note note,
                                      StreamWriter streamWriter,
                                      int trackNumber,
                                      long time,
                                      MidiFileCsvConversionSettings settings,
                                      TempoMap tempoMap)
        {
            var formattedNote = settings.NoteSettings.NoteNumberFormat == NoteNumberFormat.NoteNumber
                ? (object)note.NoteNumber
                : note;

            var formattedLength = TimeConverter.ConvertTo(note.Length, settings.NoteSettings.LengthType, tempoMap);

            WriteRecord(streamWriter,
                        trackNumber,
                        time,
                        DryWetMidiRecordTypes.Note,
                        settings,
                        tempoMap,
                        note.Channel,
                        formattedNote,
                        formattedLength,
                        note.Velocity,
                        note.OffVelocity);
        }

        private static void WriteTimedEvent(TimedEvent timedEvent,
                                            StreamWriter streamWriter,
                                            int trackNumber,
                                            long time,
                                            MidiFileCsvConversionSettings settings,
                                            TempoMap tempoMap)
        {
            var midiEvent = timedEvent.Event;
            var eventType = midiEvent.GetType();

            var eventNameGetter = EventNameGetterProvider.Get(eventType, settings.CsvLayout);
            var recordType = eventNameGetter(midiEvent);

            var eventParametersGetter = EventParametersGetterProvider.Get(eventType);
            var recordParameters = eventParametersGetter(midiEvent, settings);

            WriteRecord(streamWriter,
                        trackNumber,
                        time,
                        recordType,
                        settings,
                        tempoMap,
                        recordParameters);
        }

        private static void WriteHeader(StreamWriter streamWriter,
                                        MidiFile midiFile,
                                        MidiFileCsvConversionSettings settings,
                                        TempoMap tempoMap)
        {
            MidiFileFormat? format = null;
            try
            {
                format = midiFile.OriginalFormat;
            }
            catch { }

            var trackChunksCount = midiFile.GetTrackChunks().Count();

            switch (settings.CsvLayout)
            {
                case MidiFileCsvLayout.DryWetMidi:
                    WriteRecord(streamWriter,
                                null,
                                null,
                                DryWetMidiRecordTypes.File.Header,
                                settings,
                                tempoMap,
                                format,
                                midiFile.TimeDivision.ToInt16());
                    break;
                case MidiFileCsvLayout.MidiCsv:
                    WriteRecord(streamWriter,
                                0,
                                0,
                                MidiCsvRecordTypes.File.Header,
                                settings,
                                tempoMap,
                                format != null ? (ushort)format.Value : (trackChunksCount > 1 ? 1 : 0),
                                trackChunksCount,
                                midiFile.TimeDivision.ToInt16());
                    break;
            }
        }

        private static void WriteTrackChunkStart(StreamWriter streamWriter,
                                                 int trackNumber,
                                                 MidiFileCsvConversionSettings settings,
                                                 TempoMap tempoMap)
        {
            switch (settings.CsvLayout)
            {
                case MidiFileCsvLayout.DryWetMidi:
                    break;
                case MidiFileCsvLayout.MidiCsv:
                    WriteRecord(streamWriter, trackNumber, 0, MidiCsvRecordTypes.File.TrackChunkStart, settings, tempoMap);
                    break;
            }
        }

        private static void WriteTrackChunkEnd(StreamWriter streamWriter,
                                               int trackNumber,
                                               long time,
                                               MidiFileCsvConversionSettings settings,
                                               TempoMap tempoMap)
        {
            switch (settings.CsvLayout)
            {
                case MidiFileCsvLayout.DryWetMidi:
                    return;
                case MidiFileCsvLayout.MidiCsv:
                    WriteRecord(streamWriter, trackNumber, time, MidiCsvRecordTypes.File.TrackChunkEnd, settings, tempoMap);
                    break;
            }
        }

        private static void WriteFileEnd(StreamWriter streamWriter,
                                         MidiFileCsvConversionSettings settings,
                                         TempoMap tempoMap)
        {
            switch (settings.CsvLayout)
            {
                case MidiFileCsvLayout.DryWetMidi:
                    return;
                case MidiFileCsvLayout.MidiCsv:
                    WriteRecord(streamWriter, 0, 0, MidiCsvRecordTypes.File.FileEnd, settings, tempoMap);
                    break;
            }
        }

        private static void WriteRecord(StreamWriter streamWriter,
                                        int? trackNumber,
                                        long? time,
                                        string type,
                                        MidiFileCsvConversionSettings settings,
                                        TempoMap tempoMap,
                                        params object[] parameters)
        {
            var convertedTime = time == null
                ? null
                : TimeConverter.ConvertTo(time.Value, settings.TimeType, tempoMap);

            var processedParameters = parameters.SelectMany(ProcessParameter);

            streamWriter.WriteLine(CsvUtilities.MergeCsvValues(
                settings.CsvDelimiter,
                new object[] { trackNumber, convertedTime, type }.Concat(processedParameters)));
        }

        private static object[] ProcessParameter(object parameter)
        {
            if (parameter == null)
                return new[] { string.Empty };

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
