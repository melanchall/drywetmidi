using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    internal static class MidiFileToCsvConverter
    {
        #region Constants

        private static readonly Dictionary<Type, Func<MidiEvent, MidiFileCsvConversionSettings, object[]>> EventsParameters =
            new Dictionary<Type, Func<MidiEvent, MidiFileCsvConversionSettings, object[]>>
            {
                [typeof(SequenceTrackNameEvent)] = GetParameters<SequenceTrackNameEvent>((e, s) => e.Text),
                [typeof(CopyrightNoticeEvent)] = GetParameters<CopyrightNoticeEvent>((e, s) => e.Text),
                [typeof(InstrumentNameEvent)] = GetParameters<InstrumentNameEvent>((e, s) => e.Text),
                [typeof(MarkerEvent)] = GetParameters<MarkerEvent>((e, s) => e.Text),
                [typeof(CuePointEvent)] = GetParameters<CuePointEvent>((e, s) => e.Text),
                [typeof(LyricEvent)] = GetParameters<LyricEvent>((e, s) => e.Text),
                [typeof(TextEvent)] = GetParameters<TextEvent>((e, s) => e.Text),
                [typeof(SequenceNumberEvent)] = GetParameters<SequenceNumberEvent>((e, s) => e.Number),
                [typeof(PortPrefixEvent)] = GetParameters<PortPrefixEvent>((e, s) => e.Port),
                [typeof(ChannelPrefixEvent)] = GetParameters<ChannelPrefixEvent>((e, s) => e.Channel),
                [typeof(TimeSignatureEvent)] = GetParameters<TimeSignatureEvent>((e, s) => e.Numerator,
                                                                                 (e, s) =>
                                                                                 {
                                                                                     switch (s.CsvLayout)
                                                                                     {
                                                                                         case MidiFileCsvLayout.DryWetMidi:
                                                                                             return e.Denominator;
                                                                                         case MidiFileCsvLayout.UbuntuMidiCsv:
                                                                                             return (byte)Math.Log(e.Denominator, 2);
                                                                                     }

                                                                                     return null;
                                                                                 },
                                                                                 (e, s) => e.ClocksPerClick,
                                                                                 (e, s) => e.ThirtySecondNotesPerBeat),
                [typeof(KeySignatureEvent)] = GetParameters<KeySignatureEvent>((e, s) => e.Key,
                                                                               (e, s) => e.Scale),
                [typeof(SetTempoEvent)] = GetParameters<SetTempoEvent>((e, s) => e.MicrosecondsPerQuarterNote),
                [typeof(SmpteOffsetEvent)] = GetParameters<SmpteOffsetEvent>((e, s) => e.Hours,
                                                                             (e, s) => e.Minutes,
                                                                             (e, s) => e.Seconds,
                                                                             (e, s) => e.Frames,
                                                                             (e, s) => e.SubFrames),
                [typeof(SequencerSpecificEvent)] = GetParameters<SequencerSpecificEvent>((e, s) => e.Data.Length,
                                                                                         (e, s) => e.Data),
                [typeof(UnknownMetaEvent)] = GetParameters<UnknownMetaEvent>((e, s) => e.StatusByte,
                                                                             (e, s) => e.Data.Length,
                                                                             (e, s) => e.Data),
                [typeof(NoteOnEvent)] = GetParameters<NoteOnEvent>((e, s) => e.Channel,
                                                                   (e, s) => e.NoteNumber,
                                                                   (e, s) => e.Velocity),
                [typeof(NoteOffEvent)] = GetParameters<NoteOffEvent>((e, s) => e.Channel,
                                                                     (e, s) => e.NoteNumber,
                                                                     (e, s) => e.Velocity),
                [typeof(PitchBendEvent)] = GetParameters<PitchBendEvent>((e, s) => e.Channel,
                                                                         (e, s) => e.PitchValue),
                [typeof(ControlChangeEvent)] = GetParameters<ControlChangeEvent>((e, s) => e.Channel,
                                                                                 (e, s) => e.ControlNumber,
                                                                                 (e, s) => e.ControlValue),
                [typeof(ProgramChangeEvent)] = GetParameters<ProgramChangeEvent>((e, s) => e.Channel,
                                                                                 (e, s) => e.ProgramNumber),
                [typeof(ChannelAftertouchEvent)] = GetParameters<ChannelAftertouchEvent>((e, s) => e.Channel,
                                                                                         (e, s) => e.AftertouchValue),
                [typeof(NoteAftertouchEvent)] = GetParameters<NoteAftertouchEvent>((e, s) => e.Channel,
                                                                                   (e, s) => e.NoteNumber,
                                                                                   (e, s) => e.AftertouchValue),
                [typeof(NormalSysExEvent)] = GetParameters<NormalSysExEvent>((e, s) => e.Data.Length,
                                                                             (e, s) => e.Data),
                [typeof(EscapeSysExEvent)] = GetParameters<EscapeSysExEvent>((e, s) => e.Data.Length,
                                                                             (e, s) => e.Data)
            };

        private static readonly Dictionary<Type, Func<MidiEvent, string>> EventsTypes_UbuntuMidiCsv =
            new Dictionary<Type, Func<MidiEvent, string>>
            {
                [typeof(SequenceTrackNameEvent)] = GetType("Title_t"),
                [typeof(CopyrightNoticeEvent)] = GetType("Copyright_t"),
                [typeof(InstrumentNameEvent)] = GetType("Instrument_name_t"),
                [typeof(MarkerEvent)] = GetType("Marker_t"),
                [typeof(CuePointEvent)] = GetType("Cue_point_t"),
                [typeof(LyricEvent)] = GetType("Lyric_t"),
                [typeof(TextEvent)] = GetType("Text_t"),
                [typeof(SequenceNumberEvent)] = GetType("Sequence_number"),
                [typeof(PortPrefixEvent)] = GetType("MIDI_port"),
                [typeof(ChannelPrefixEvent)] = GetType("Channel_prefix"),
                [typeof(TimeSignatureEvent)] = GetType("Time_signature"),
                [typeof(KeySignatureEvent)] = GetType("Key_signature"),
                [typeof(SetTempoEvent)] = GetType("Tempo"),
                [typeof(SmpteOffsetEvent)] = GetType("SMPTE_offset"),
                [typeof(SequencerSpecificEvent)] = GetType("Sequencer_specific"),
                [typeof(UnknownMetaEvent)] = GetType("Unknown_meta_event"),
                [typeof(NoteOnEvent)] = GetType("Note_on_c"),
                [typeof(NoteOffEvent)] = GetType("Note_off_c"),
                [typeof(PitchBendEvent)] = GetType("Pitch_bend_c"),
                [typeof(ControlChangeEvent)] = GetType("Control_c"),
                [typeof(ProgramChangeEvent)] = GetType("Program_c"),
                [typeof(ChannelAftertouchEvent)] = GetType("Channel_aftertouch_c"),
                [typeof(NoteAftertouchEvent)] = GetType("Poly_aftertouch_c"),
                [typeof(NormalSysExEvent)] = GetSysExType("System_exclusive", "System_exclusive_packet"),
                [typeof(EscapeSysExEvent)] = GetSysExType("System_exclusive", "System_exclusive_packet")
            };

        private static readonly Dictionary<Type, Func<MidiEvent, string>> EventsTypes_DryWetMIdi =
            new Dictionary<Type, Func<MidiEvent, string>>
            {
                [typeof(SequenceTrackNameEvent)] = GetType("Sequence/Track Name"),
                [typeof(CopyrightNoticeEvent)] = GetType("Copyright Notice"),
                [typeof(InstrumentNameEvent)] = GetType("Instrument Name"),
                [typeof(MarkerEvent)] = GetType("Marker"),
                [typeof(CuePointEvent)] = GetType("Cue Point"),
                [typeof(LyricEvent)] = GetType("Lyric"),
                [typeof(TextEvent)] = GetType("Text"),
                [typeof(SequenceNumberEvent)] = GetType("Sequence Number"),
                [typeof(PortPrefixEvent)] = GetType("Port Prefix"),
                [typeof(ChannelPrefixEvent)] = GetType("Channel Prefix"),
                [typeof(TimeSignatureEvent)] = GetType("Time Signature"),
                [typeof(KeySignatureEvent)] = GetType("Key Signature"),
                [typeof(SetTempoEvent)] = GetType("Set Tempo"),
                [typeof(SmpteOffsetEvent)] = GetType("SMPTE Offset"),
                [typeof(SequencerSpecificEvent)] = GetType("Sequencer Specific"),
                [typeof(UnknownMetaEvent)] = GetType("Unknown Meta"),
                [typeof(NoteOnEvent)] = GetType("Note On"),
                [typeof(NoteOffEvent)] = GetType("Note Off"),
                [typeof(PitchBendEvent)] = GetType("Pitch Bend"),
                [typeof(ControlChangeEvent)] = GetType("Control Change"),
                [typeof(ProgramChangeEvent)] = GetType("Program Change"),
                [typeof(ChannelAftertouchEvent)] = GetType("Channel Aftertouch"),
                [typeof(NoteAftertouchEvent)] = GetType("Note Aftertouch"),
                [typeof(NormalSysExEvent)] = GetSysExType("System Exclusive", "System Exclusive Packet"),
                [typeof(EscapeSysExEvent)] = GetSysExType("System Exclusive", "System Exclusive Packet")
            };

        #endregion

        #region Methods

        public static void ConvertToCsv(MidiFile midiFile, Stream fileStream, MidiFileCsvConversionSettings settings)
        {
            using (var streamWriter = new StreamWriter(fileStream))
            {
                var trackNumber = 0;
                var tempoMap = midiFile.GetTempoMap();

                OnHeader(streamWriter, midiFile, settings, tempoMap);

                foreach (var trackChunk in midiFile.GetTrackChunks())
                {
                    OnTrackChunkStart(streamWriter, trackNumber, settings, tempoMap);

                    var time = 0L;

                    foreach (var timedEvent in trackChunk.GetTimedEvents())
                    {
                        time = timedEvent.Time;
                        var midiEvent = timedEvent.Event;
                        var eventType = midiEvent.GetType();

                        var recordType = string.Empty;
                        switch (settings.CsvLayout)
                        {
                            case MidiFileCsvLayout.DryWetMidi:
                                recordType = EventsTypes_DryWetMIdi[eventType](midiEvent);
                                break;
                            case MidiFileCsvLayout.UbuntuMidiCsv:
                                recordType = EventsTypes_UbuntuMidiCsv[eventType](midiEvent);
                                break;
                        }

                        var recordParameters = EventsParameters[eventType](midiEvent, settings);

                        WriteRecord(streamWriter,
                                    trackNumber,
                                    time,
                                    recordType,
                                    settings,
                                    tempoMap,
                                    recordParameters);
                    }

                    OnTrackChunkEnd(streamWriter, trackNumber, time, settings, tempoMap);

                    trackNumber++;
                }

                OnFileEnd(streamWriter, settings, tempoMap);
            }
        }

        private static void OnHeader(
            StreamWriter streamWriter,
            MidiFile midiFile,
            MidiFileCsvConversionSettings settings,
            TempoMap tempoMap)
        {
            switch (settings.CsvLayout)
            {
                case MidiFileCsvLayout.DryWetMidi:
                    // TODO: time division format
                    WriteRecord(streamWriter, null, null, "Header", settings, tempoMap, midiFile.OriginalFormat, midiFile.TimeDivision);
                    break;
                case MidiFileCsvLayout.UbuntuMidiCsv:
                    WriteRecord(streamWriter, 0, 0, "Header", settings, tempoMap, (int)midiFile.OriginalFormat, midiFile.GetTrackChunks().Count(), midiFile.TimeDivision.ToInt16());
                    break;
            }
        }

        private static void OnTrackChunkStart(
            StreamWriter streamWriter,
            int trackNumber,
            MidiFileCsvConversionSettings settings,
            TempoMap tempoMap)
        {
            switch (settings.CsvLayout)
            {
                case MidiFileCsvLayout.DryWetMidi:
                    streamWriter.WriteLine();
                    break;
                case MidiFileCsvLayout.UbuntuMidiCsv:
                    WriteRecord(streamWriter, trackNumber, 0, "Start_track", settings, tempoMap);
                    break;
            }
        }

        private static void OnTrackChunkEnd(
            StreamWriter streamWriter,
            int trackNumber,
            long time,
            MidiFileCsvConversionSettings settings,
            TempoMap tempoMap)
        {
            switch (settings.CsvLayout)
            {
                case MidiFileCsvLayout.DryWetMidi:
                    return;
                case MidiFileCsvLayout.UbuntuMidiCsv:
                    WriteRecord(streamWriter, trackNumber, time, "End_track", settings, tempoMap);
                    break;
            }
        }

        private static void OnFileEnd(
            StreamWriter streamWriter,
            MidiFileCsvConversionSettings settings,
            TempoMap tempoMap)
        {
            switch (settings.CsvLayout)
            {
                case MidiFileCsvLayout.DryWetMidi:
                    return;
                case MidiFileCsvLayout.UbuntuMidiCsv:
                    WriteRecord(streamWriter, 0, 0, "End_of_file", settings, tempoMap);
                    break;
            }
        }

        private static void WriteRecord(
            StreamWriter streamWriter,
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

            var processedParameters = parameters.Select(p =>
            {
                var bytes = p as byte[];
                if (bytes != null)
                    return SmfUtilities.DefaultEncoding.GetString(bytes);

                if (p is string)
                    return $"\"{p}\"";

                return p;
            });

            streamWriter.WriteLine(string.Join(", ", new object[] { trackNumber, convertedTime, type }.Concat(processedParameters)));
        }

        private static Func<MidiEvent, MidiFileCsvConversionSettings, object[]> GetParameters<T>(params Func<T, MidiFileCsvConversionSettings, object>[] parametersGetters)
            where T : MidiEvent
        {
            return (e, s) => parametersGetters.Select(g => g((T)e, s)).ToArray();
        }

        private static Func<MidiEvent, string> GetType(string type)
        {
            return e => type;
        }

        private static Func<MidiEvent, string> GetSysExType(string completedType, string incompletedType)
        {
            return e =>
            {
                var sysExEvent = (SysExEvent)e;
                return sysExEvent.Completed ? completedType : incompletedType;
            };
        }

        #endregion
    }
}
