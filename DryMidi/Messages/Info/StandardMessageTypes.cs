namespace Melanchall.DryMidi
{
    internal static class StandardMessageTypes
    {
        #region Constants

        internal static readonly MessageTypesCollection Channel = new MessageTypesCollection
        {
            { typeof(ChannelAftertouchMessage), MessageStatusBytes.Channel.ChannelAftertouch },
            { typeof(ControlChangeMessage), MessageStatusBytes.Channel.ControlChange },
            { typeof(NoteAftertouchMessage), MessageStatusBytes.Channel.NoteAftertouch },
            { typeof(NoteOffMessage), MessageStatusBytes.Channel.NoteOff },
            { typeof(NoteOnMessage), MessageStatusBytes.Channel.NoteOn },
            { typeof(PitchBendMessage), MessageStatusBytes.Channel.PitchBend },
            { typeof(ProgramChangeMessage), MessageStatusBytes.Channel.ProgramChange }
        };

        internal static readonly MessageTypesCollection Meta = new MessageTypesCollection
        {
            { typeof(SequenceNumberMessage), MessageStatusBytes.Meta.SequenceNumber },
            { typeof(TextMessage), MessageStatusBytes.Meta.Text },
            { typeof(CopyrightNoticeMessage), MessageStatusBytes.Meta.CopyrightNotice },
            { typeof(SequenceTrackNameMessage), MessageStatusBytes.Meta.SequenceTrackName },
            { typeof(InstrumentNameMessage), MessageStatusBytes.Meta.InstrumentName },
            { typeof(LyricMessage), MessageStatusBytes.Meta.Lyric },
            { typeof(MarkerMessage), MessageStatusBytes.Meta.Marker },
            { typeof(CuePointMessage), MessageStatusBytes.Meta.CuePoint },
            { typeof(ProgramNameMessage), MessageStatusBytes.Meta.ProgramName },
            { typeof(DeviceNameMessage), MessageStatusBytes.Meta.DeviceName },
            { typeof(ChannelPrefixMessage), MessageStatusBytes.Meta.ChannelPrefix },
            { typeof(PortPrefixMessage), MessageStatusBytes.Meta.PortPrefix },
            { typeof(EndOfTrackMessage), MessageStatusBytes.Meta.EndOfTrack },
            { typeof(SetTempoMessage), MessageStatusBytes.Meta.SetTempo },
            { typeof(SmpteOffsetMessage), MessageStatusBytes.Meta.SmpteOffset },
            { typeof(TimeSignatureMessage), MessageStatusBytes.Meta.TimeSignature },
            { typeof(KeySignatureMessage), MessageStatusBytes.Meta.KeySignature },
            { typeof(SequencerSpecificMessage), MessageStatusBytes.Meta.SequencerSpecific }
        };

        internal static readonly MessageTypesCollection SysEx = new MessageTypesCollection
        {
            { typeof(EscapeSysExMessage), MessageStatusBytes.Global.EscapeSysEx },
            { typeof(NormalSysExMessage), MessageStatusBytes.Global.NormalSysEx }
        };

        #endregion
    }
}
