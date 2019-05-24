namespace Melanchall.DryWetMidi.Smf
{
    public enum MidiEventType
    {
        // Sysex

        NormalSysEx,
        EscapeSysEx,

        // Meta

        SequenceNumber,
        Text,
        CopyrightNotice,
        SequenceTrackName,
        InstrumentName,
        Lyric,
        Marker,
        CuePoint,
        ProgramName,
        DeviceName,
        ChannelPrefix,
        PortPrefix,
        EndOfTrack,
        SetTempo,
        SmpteOffset,
        TimeSignature,
        KeySignature,
        SequencerSpecific,
        UnknownMeta,

        // Channel

        NoteOff,
        NoteOn,
        NoteAftertouch,
        ControlChange,
        ProgramChange,
        ChannelAftertouch,
        PitchBend,

        // System real-time

        TimingClock,
        Start,
        Continue,
        Stop,
        ActiveSensing,
        Reset,

        // System common

        MidiTimeCode,
        SongPositionPointer,
        SongSelect,
        TuneRequest
    }
}
