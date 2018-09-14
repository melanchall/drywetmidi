namespace Melanchall.DryWetMidi.Smf
{
    internal static class StandardEventTypes
    {
        #region Constants

        internal static readonly EventTypesCollection Channel = new EventTypesCollection
        {
            { typeof(ChannelAftertouchEvent), EventStatusBytes.Channel.ChannelAftertouch },
            { typeof(ControlChangeEvent), EventStatusBytes.Channel.ControlChange },
            { typeof(NoteAftertouchEvent), EventStatusBytes.Channel.NoteAftertouch },
            { typeof(NoteOffEvent), EventStatusBytes.Channel.NoteOff },
            { typeof(NoteOnEvent), EventStatusBytes.Channel.NoteOn },
            { typeof(PitchBendEvent), EventStatusBytes.Channel.PitchBend },
            { typeof(ProgramChangeEvent), EventStatusBytes.Channel.ProgramChange }
        };

        internal static readonly EventTypesCollection Meta = new EventTypesCollection
        {
            { typeof(SequenceNumberEvent), EventStatusBytes.Meta.SequenceNumber },
            { typeof(TextEvent), EventStatusBytes.Meta.Text },
            { typeof(CopyrightNoticeEvent), EventStatusBytes.Meta.CopyrightNotice },
            { typeof(SequenceTrackNameEvent), EventStatusBytes.Meta.SequenceTrackName },
            { typeof(InstrumentNameEvent), EventStatusBytes.Meta.InstrumentName },
            { typeof(LyricEvent), EventStatusBytes.Meta.Lyric },
            { typeof(MarkerEvent), EventStatusBytes.Meta.Marker },
            { typeof(CuePointEvent), EventStatusBytes.Meta.CuePoint },
            { typeof(ProgramNameEvent), EventStatusBytes.Meta.ProgramName },
            { typeof(DeviceNameEvent), EventStatusBytes.Meta.DeviceName },
            { typeof(ChannelPrefixEvent), EventStatusBytes.Meta.ChannelPrefix },
            { typeof(PortPrefixEvent), EventStatusBytes.Meta.PortPrefix },
            { typeof(EndOfTrackEvent), EventStatusBytes.Meta.EndOfTrack },
            { typeof(SetTempoEvent), EventStatusBytes.Meta.SetTempo },
            { typeof(SmpteOffsetEvent), EventStatusBytes.Meta.SmpteOffset },
            { typeof(TimeSignatureEvent), EventStatusBytes.Meta.TimeSignature },
            { typeof(KeySignatureEvent), EventStatusBytes.Meta.KeySignature },
            { typeof(SequencerSpecificEvent), EventStatusBytes.Meta.SequencerSpecific }
        };

        internal static readonly EventTypesCollection SysEx = new EventTypesCollection
        {
            { typeof(EscapeSysExEvent), EventStatusBytes.Global.EscapeSysEx },
            { typeof(NormalSysExEvent), EventStatusBytes.Global.NormalSysEx }
        };

        internal static readonly EventTypesCollection SystemRealTime = new EventTypesCollection
        {
            { typeof(ActiveSensingEvent), EventStatusBytes.SystemRealTime.ActiveSensing },
            { typeof(ContinueEvent), EventStatusBytes.SystemRealTime.Continue },
            { typeof(ResetEvent), EventStatusBytes.SystemRealTime.Reset },
            { typeof(StartEvent), EventStatusBytes.SystemRealTime.Start },
            { typeof(StopEvent), EventStatusBytes.SystemRealTime.Stop },
            { typeof(TimingClockEvent), EventStatusBytes.SystemRealTime.TimingClock }
        };

        internal static readonly EventTypesCollection SystemCommon = new EventTypesCollection
        {
            { typeof(MidiTimeCodeEvent), EventStatusBytes.SystemCommon.MtcQuarterFrame },
            { typeof(SongSelectEvent), EventStatusBytes.SystemCommon.SongSelect },
            { typeof(SongPositionPointerEvent), EventStatusBytes.SystemCommon.SongPositionPointer },
            { typeof(TuneRequestEvent), EventStatusBytes.SystemCommon.TuneRequest }
        };

        #endregion
    }
}
