namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// The type of a MIDI event.
    /// </summary>
    public enum MidiEventType : byte
    {
        /// <summary>
        /// Normal system exclusive event.
        /// </summary>
        NormalSysEx,

        /// <summary>
        /// "Escape" system exclusive event which defines an escape sequence.
        /// </summary>
        EscapeSysEx,

        /// <summary>
        /// Sequence Number meta event.
        /// </summary>
        SequenceNumber,

        /// <summary>
        /// Text meta event.
        /// </summary>
        Text,

        /// <summary>
        /// Copyright Notice meta event.
        /// </summary>
        CopyrightNotice,

        /// <summary>
        /// Sequence/Track Name meta event.
        /// </summary>
        SequenceTrackName,

        /// <summary>
        /// Instrument Name meta event.
        /// </summary>
        InstrumentName,

        /// <summary>
        /// Lyric meta event.
        /// </summary>
        Lyric,

        /// <summary>
        /// Marker meta event.
        /// </summary>
        Marker,

        /// <summary>
        /// Cue Point meta event.
        /// </summary>
        CuePoint,

        /// <summary>
        /// Program Name meta event.
        /// </summary>
        ProgramName,

        /// <summary>
        /// Device Name meta event.
        /// </summary>
        DeviceName,

        /// <summary>
        /// MIDI Channel Prefix meta event.
        /// </summary>
        ChannelPrefix,

        /// <summary>
        /// MIDI Port meta event.
        /// </summary>
        PortPrefix,

        /// <summary>
        /// End of Track meta event.
        /// </summary>
        EndOfTrack,

        /// <summary>
        /// Set Tempo meta event.
        /// </summary>
        SetTempo,

        /// <summary>
        /// SMPTE Offset meta event.
        /// </summary>
        SmpteOffset,

        /// <summary>
        /// Time Signature meta event.
        /// </summary>
        TimeSignature,

        /// <summary>
        /// Key Signature meta event.
        /// </summary>
        KeySignature,

        /// <summary>
        /// Sequencer Specific meta event.
        /// </summary>
        SequencerSpecific,

        /// <summary>
        /// Unknown meta event.
        /// </summary>
        UnknownMeta,

        /// <summary>
        /// Custom meta event.
        /// </summary>
        CustomMeta,

        /// <summary>
        /// Note Off event.
        /// </summary>
        NoteOff,

        /// <summary>
        /// Note On event.
        /// </summary>
        NoteOn,

        /// <summary>
        /// Polyphonic Key Pressure (Aftertouch) event.
        /// </summary>
        NoteAftertouch,

        /// <summary>
        /// Control Change event.
        /// </summary>
        ControlChange,

        /// <summary>
        /// Program Change event.
        /// </summary>
        ProgramChange,

        /// <summary>
        /// Channel Pressure (Aftertouch) event.
        /// </summary>
        ChannelAftertouch,

        /// <summary>
        /// Pitch Bend Change event.
        /// </summary>
        PitchBend,

        /// <summary>
        /// Timing Clock event.
        /// </summary>
        TimingClock,

        /// <summary>
        /// Start event.
        /// </summary>
        Start,

        /// <summary>
        /// Continue event.
        /// </summary>
        Continue,

        /// <summary>
        /// Stop event.
        /// </summary>
        Stop,

        /// <summary>
        /// Active Sensing event.
        /// </summary>
        ActiveSensing,

        /// <summary>
        /// Reset event.
        /// </summary>
        Reset,

        /// <summary>
        /// MIDI Time Code (MIDI Quarter Frame) event.
        /// </summary>
        MidiTimeCode,

        /// <summary>
        /// Song Position Pointer event.
        /// </summary>
        SongPositionPointer,

        /// <summary>
        /// Song Select event.
        /// </summary>
        SongSelect,

        /// <summary>
        /// Tune Request event.
        /// </summary>
        TuneRequest
    }
}
