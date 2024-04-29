using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tools
{
    internal static class EventParametersProvider
    {
        #region Delegates

        public delegate object[] EventParametersGetter(MidiEvent midiEvent, CsvSerializationSettings settings);

        #endregion

        #region Constants

        private static readonly Dictionary<MidiEventType, EventParametersGetter> EventsParametersGetters =
            new Dictionary<MidiEventType, EventParametersGetter>
            {
                [MidiEventType.SequenceTrackName] = GetParameters<SequenceTrackNameEvent>(
                    (e, s) => e.Text),
                [MidiEventType.CopyrightNotice] = GetParameters<CopyrightNoticeEvent>(
                    (e, s) => e.Text),
                [MidiEventType.InstrumentName] = GetParameters<InstrumentNameEvent>(
                    (e, s) => e.Text),
                [MidiEventType.Marker] = GetParameters<MarkerEvent>(
                    (e, s) => e.Text),
                [MidiEventType.CuePoint] = GetParameters<CuePointEvent>(
                    (e, s) => e.Text),
                [MidiEventType.Lyric] = GetParameters<LyricEvent>(
                    (e, s) => e.Text),
                [MidiEventType.Text] = GetParameters<TextEvent>(
                    (e, s) => e.Text),
                [MidiEventType.ProgramName] = GetParameters<ProgramNameEvent>(
                    (e, s) => e.Text),
                [MidiEventType.DeviceName] = GetParameters<DeviceNameEvent>(
                    (e, s) => e.Text),
                [MidiEventType.SequenceNumber] = GetParameters<SequenceNumberEvent>(
                    (e, s) => e.Number),
                [MidiEventType.PortPrefix] = GetParameters<PortPrefixEvent>(
                    (e, s) => e.Port),
                [MidiEventType.ChannelPrefix] = GetParameters<ChannelPrefixEvent>(
                    (e, s) => e.Channel),
                [MidiEventType.TimeSignature] = GetParameters<TimeSignatureEvent>(
                    (e, s) => e.Numerator,
                    (e, s) => e.Denominator,
                    (e, s) => e.ClocksPerClick,
                    (e, s) => e.ThirtySecondNotesPerBeat),
                [MidiEventType.KeySignature] = GetParameters<KeySignatureEvent>(
                    (e, s) => e.Key,
                    (e, s) => e.Scale),
                [MidiEventType.SetTempo] = GetParameters<SetTempoEvent>(
                    (e, s) => e.MicrosecondsPerQuarterNote),
                [MidiEventType.SmpteOffset] = GetParameters<SmpteOffsetEvent>(
                    (e, s) => e.Format.ToString(),
                    (e, s) => e.Hours,
                    (e, s) => e.Minutes,
                    (e, s) => e.Seconds,
                    (e, s) => e.Frames,
                    (e, s) => e.SubFrames),
                [MidiEventType.SequencerSpecific] = GetParameters<SequencerSpecificEvent>(
                    (e, s) => e.Data),
                [MidiEventType.UnknownMeta] = GetParameters<UnknownMetaEvent>(
                    (e, s) => e.StatusByte,
                    (e, s) => e.Data),
                [MidiEventType.NoteOn] = GetParameters<NoteOnEvent>(
                    (e, s) => e.Channel,
                    (e, s) => CsvFormattingUtilities.FormatNoteNumber(e.NoteNumber, s.NoteFormat),
                    (e, s) => e.Velocity),
                [MidiEventType.NoteOff] = GetParameters<NoteOffEvent>(
                    (e, s) => e.Channel,
                    (e, s) => CsvFormattingUtilities.FormatNoteNumber(e.NoteNumber, s.NoteFormat),
                    (e, s) => e.Velocity),
                [MidiEventType.PitchBend] = GetParameters<PitchBendEvent>(
                    (e, s) => e.Channel,
                    (e, s) => e.PitchValue),
                [MidiEventType.ControlChange] = GetParameters<ControlChangeEvent>(
                    (e, s) => e.Channel,
                    (e, s) => e.ControlNumber,
                    (e, s) => e.ControlValue),
                [MidiEventType.ProgramChange] = GetParameters<ProgramChangeEvent>(
                    (e, s) => e.Channel,
                    (e, s) => e.ProgramNumber),
                [MidiEventType.ChannelAftertouch] = GetParameters<ChannelAftertouchEvent>(
                    (e, s) => e.Channel,
                    (e, s) => e.AftertouchValue),
                [MidiEventType.NoteAftertouch] = GetParameters<NoteAftertouchEvent>(
                    (e, s) => e.Channel,
                    (e, s) => CsvFormattingUtilities.FormatNoteNumber(e.NoteNumber, s.NoteFormat),
                    (e, s) => e.AftertouchValue),
                [MidiEventType.NormalSysEx] = GetParameters<NormalSysExEvent>(
                    (e, s) => e.Data),
                [MidiEventType.EscapeSysEx] = GetParameters<EscapeSysExEvent>(
                    (e, s) => e.Data),
                [MidiEventType.ActiveSensing] = GetParameters<ActiveSensingEvent>(),
                [MidiEventType.Start] = GetParameters<StartEvent>(),
                [MidiEventType.Stop] = GetParameters<StopEvent>(),
                [MidiEventType.Reset] = GetParameters<ResetEvent>(),
                [MidiEventType.Continue] = GetParameters<ContinueEvent>(),
                [MidiEventType.TimingClock] = GetParameters<TimingClockEvent>(),
                [MidiEventType.TuneRequest] = GetParameters<TuneRequestEvent>(),
                [MidiEventType.MidiTimeCode] = GetParameters<MidiTimeCodeEvent>(
                    (e, s) => e.Component.ToString(),
                    (e, s) => e.ComponentValue),
                [MidiEventType.SongSelect] = GetParameters<SongSelectEvent>(
                    (e, s) => e.Number),
                [MidiEventType.SongPositionPointer] = GetParameters<SongPositionPointerEvent>(
                    (e, s) => e.PointerValue),
            };

        #endregion

        #region Methods

        public static object[] GetEventParameters(MidiEvent midiEvent, CsvSerializationSettings settings)
        {
            return EventsParametersGetters[midiEvent.EventType](midiEvent, settings);
        }

        private static EventParametersGetter GetParameters<TEvent>(params Func<TEvent, CsvSerializationSettings, object>[] parametersGetters)
            where TEvent : MidiEvent
        {
            return (e, s) => parametersGetters.Select(g => g((TEvent)e, s)).ToArray();
        }

        #endregion
    }
}
