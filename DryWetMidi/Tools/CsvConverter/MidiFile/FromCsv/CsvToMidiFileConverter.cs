using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    internal static class CsvToMidiFileConverter
    {
        #region Constants

        private static readonly Dictionary<string, RecordType> RecordTypes_DryWetMidi =
            new Dictionary<string, RecordType>(StringComparer.OrdinalIgnoreCase)
            {
                [DryWetMidiRecordTypes.File.Header] = RecordType.Header,
                [DryWetMidiRecordTypes.Note] = RecordType.Note
            };

        private static readonly Dictionary<string, RecordType> RecordTypes_MidiCsv =
            new Dictionary<string, RecordType>(StringComparer.OrdinalIgnoreCase)
            {
                [MidiCsvRecordTypes.File.Header] = RecordType.Header,
                [MidiCsvRecordTypes.File.TrackChunkStart] = RecordType.TrackChunkStart,
                [MidiCsvRecordTypes.File.TrackChunkEnd] = RecordType.TrackChunkEnd,
                [MidiCsvRecordTypes.File.FileEnd] = RecordType.FileEnd
            };

        #endregion

        #region Methods

        public static MidiFile ConvertToMidiFile(Stream stream, MidiFileCsvConversionSettings settings)
        {
            var midiFile = new MidiFile();
            var events = new Dictionary<int, List<TimedMidiEvent>>();

            using (var csvReader = new CsvReader(stream, settings.CsvSettings))
            {
                var lineNumber = 0;
                Record record;

                while ((record = ReadRecord(csvReader, settings)) != null)
                {
                    var recordType = GetRecordType(record.RecordType, settings);
                    if (recordType == null)
                        CsvError.ThrowBadFormat(lineNumber, "Unknown record.");

                    switch (recordType)
                    {
                        case RecordType.Header:
                            {
                                var headerChunk = ParseHeader(record, settings);
                                midiFile.TimeDivision = headerChunk.TimeDivision;
                                midiFile.OriginalFormat = (MidiFileFormat)headerChunk.FileFormat;
                            }
                            break;
                        case RecordType.TrackChunkStart:
                        case RecordType.TrackChunkEnd:
                        case RecordType.FileEnd:
                            break;
                        case RecordType.Event:
                            {
                                var midiEvent = ParseEvent(record, settings);
                                var trackChunkNumber = record.TrackNumber.Value;

                                AddTimedEvents(events, trackChunkNumber, new TimedMidiEvent(record.Time, midiEvent));
                            }
                            break;
                        case RecordType.Note:
                            {
                                var noteEvents = ParseNote(record, settings);
                                var trackChunkNumber = record.TrackNumber.Value;

                                AddTimedEvents(events, trackChunkNumber, noteEvents);
                            }
                            break;
                    }

                    lineNumber = record.LineNumber + 1;
                }
            }

            if (!events.Keys.Any())
                return midiFile;

            var tempoMap = GetTempoMap(events.Values.SelectMany(e => e), midiFile.TimeDivision);

            var trackChunks = new TrackChunk[events.Keys.Max() + 1];
            for (int i = 0; i < trackChunks.Length; i++)
            {
                List<TimedMidiEvent> timedMidiEvents;
                trackChunks[i] = events.TryGetValue(i, out timedMidiEvents)
                    ? timedMidiEvents.Select(e => new TimedEvent(e.Event, TimeConverter.ConvertFrom(e.Time, tempoMap))).ToTrackChunk()
                    : new TrackChunk();
            }

            midiFile.Chunks.AddRange(trackChunks);

            return midiFile;
        }

        private static void AddTimedEvents(Dictionary<int, List<TimedMidiEvent>> eventsMap,
                                           int trackChunkNumber,
                                           params TimedMidiEvent[] events)
        {
            List<TimedMidiEvent> timedMidiEvents;
            if (!eventsMap.TryGetValue(trackChunkNumber, out timedMidiEvents))
                eventsMap.Add(trackChunkNumber, timedMidiEvents = new List<TimedMidiEvent>());

            timedMidiEvents.AddRange(events);
        }

        private static TempoMap GetTempoMap(IEnumerable<TimedMidiEvent> timedMidiEvents, TimeDivision timeDivision)
        {
            using (var tempoMapManager = new TempoMapManager(timeDivision))
            {
                var setTempoEvents = timedMidiEvents.Where(e => e.Event is SetTempoEvent)
                                                    .OrderBy(e => e.Time, new TimeSpanComparer());
                foreach (var timedMidiEvent in setTempoEvents)
                {
                    var setTempoEvent = (SetTempoEvent)timedMidiEvent.Event;
                    tempoMapManager.SetTempo(timedMidiEvent.Time,
                                             new Tempo(setTempoEvent.MicrosecondsPerQuarterNote));
                }

                var timeSignatureEvents = timedMidiEvents.Where(e => e.Event is TimeSignatureEvent)
                                                         .OrderBy(e => e.Time, new TimeSpanComparer());
                foreach (var timedMidiEvent in timeSignatureEvents)
                {
                    var timeSignatureEvent = (TimeSignatureEvent)timedMidiEvent.Event;
                    tempoMapManager.SetTimeSignature(timedMidiEvent.Time,
                                                     new TimeSignature(timeSignatureEvent.Numerator, timeSignatureEvent.Denominator));
                }

                return tempoMapManager.TempoMap;
            }
        }

        private static RecordType? GetRecordType(string recordType, MidiFileCsvConversionSettings settings)
        {
            var csvLayout = settings.CsvLayout;

            var recordTypes = csvLayout == MidiFileCsvLayout.DryWetMidi
                ? RecordTypes_DryWetMidi
                : RecordTypes_MidiCsv;
            var eventsNames = EventsNamesProvider.Get(csvLayout);

            RecordType result;
            if (recordTypes.TryGetValue(recordType, out result))
                return result;

            if (eventsNames.Contains(recordType, StringComparer.OrdinalIgnoreCase))
                return RecordType.Event;

            return null;
        }

        private static HeaderChunk ParseHeader(Record record, MidiFileCsvConversionSettings settings)
        {
            var parameters = record.Parameters;

            var format = default(MidiFileFormat?);
            var timeDivision = default(short);

            switch (settings.CsvLayout)
            {
                case MidiFileCsvLayout.DryWetMidi:
                    {
                        if (parameters.Length < 2)
                            CsvError.ThrowBadFormat(record.LineNumber, "Parameters count is invalid.");

                        MidiFileFormat formatValue;
                        if (Enum.TryParse(parameters[0], true, out formatValue))
                            format = formatValue;

                        if (!short.TryParse(parameters[1], out timeDivision))
                            CsvError.ThrowBadFormat(record.LineNumber, "Invalid time division.");
                    }
                    break;
                case MidiFileCsvLayout.MidiCsv:
                    {
                        if (parameters.Length < 3)
                            CsvError.ThrowBadFormat(record.LineNumber, "Parameters count is invalid.");

                        ushort formatValue;
                        if (ushort.TryParse(parameters[0], out formatValue) && Enum.IsDefined(typeof(MidiFileFormat), formatValue))
                            format = (MidiFileFormat)formatValue;

                        if (!short.TryParse(parameters[2], out timeDivision))
                            CsvError.ThrowBadFormat(record.LineNumber, "Invalid time division.");
                    }
                    break;
            }

            return new HeaderChunk
            {
                FileFormat = format != null ? (ushort)format.Value : ushort.MaxValue,
                TimeDivision = TimeDivisionFactory.GetTimeDivision(timeDivision)
            };
        }

        private static MidiEvent ParseEvent(Record record, MidiFileCsvConversionSettings settings)
        {
            if (record.TrackNumber == null)
                CsvError.ThrowBadFormat(record.LineNumber, "Invalid track number.");

            if (record.Time == null)
                CsvError.ThrowBadFormat(record.LineNumber, "Invalid time.");

            var eventParser = EventParserProvider.Get(record.RecordType, settings.CsvLayout);

            try
            {
                return eventParser(record.Parameters, settings);
            }
            catch (FormatException ex)
            {
                CsvError.ThrowBadFormat(record.LineNumber, "Invalid format of event record.", ex);
                return null;
            }
        }

        private static TimedMidiEvent[] ParseNote(Record record, MidiFileCsvConversionSettings settings)
        {
            if (record.TrackNumber == null)
                CsvError.ThrowBadFormat(record.LineNumber, "Invalid track number.");

            if (record.Time == null)
                CsvError.ThrowBadFormat(record.LineNumber, "Invalid time.");

            var parameters = record.Parameters;
            if (parameters.Length < 5)
                CsvError.ThrowBadFormat(record.LineNumber, "Invalid number of parameters provided.");

            var i = -1;

            try
            {
                var channel = (FourBitNumber)TypeParser.FourBitNumber(parameters[++i], settings);
                var noteNumber = (SevenBitNumber)TypeParser.NoteNumber(parameters[++i], settings);

                ITimeSpan length;
                TimeSpanUtilities.TryParse(parameters[++i], settings.NoteLengthType, out length);

                var velocity = (SevenBitNumber)TypeParser.SevenBitNumber(parameters[++i], settings);
                var offVelocity = (SevenBitNumber)TypeParser.SevenBitNumber(parameters[++i], settings);

                return new[]
                {
                    new TimedMidiEvent(record.Time, new NoteOnEvent(noteNumber, velocity) { Channel = channel }),
                    new TimedMidiEvent(record.Time.Add(length, TimeSpanMode.TimeLength), new NoteOffEvent(noteNumber, offVelocity) { Channel = channel }),
                };
            }
            catch
            {
                CsvError.ThrowBadFormat(record.LineNumber, $"Parameter ({i}) is invalid.");
            }

            return null;
        }

        private static Record ReadRecord(CsvReader csvReader, MidiFileCsvConversionSettings settings)
        {
            var record = csvReader.ReadRecord();
            if (record == null)
                return null;

            var values = record.Values;
            if (values.Length < 3)
                CsvError.ThrowBadFormat(record.LineNumber, "Missing required parameters.");

            int parsedTrackNumber;
            var trackNumber = int.TryParse(values[0], out parsedTrackNumber)
                ? (int?)parsedTrackNumber
                : null;

            ITimeSpan time;
            TimeSpanUtilities.TryParse(values[1], settings.TimeType, out time);

            var recordType = values[2];
            if (string.IsNullOrEmpty(recordType))
                CsvError.ThrowBadFormat(record.LineNumber, "Record type isn't specified.");

            var parameters = values.Skip(3).ToArray();

            return new Record(record.LineNumber, trackNumber, time, recordType, parameters);
        }

        #endregion
    }
}
