namespace Melanchall.DryWetMidi.Tools
{
    internal static class DryWetMidiRecordTypes
    {
        public static class File
        {
            public const string Header = "Header";
        }

        public static class Events
        {
            public const string SequenceTrackName = "Sequence/Track Name";
            public const string CopyrightNotice = "Copyright Notice";
            public const string InstrumentName = "Instrument Name";
            public const string Marker = "Marker";
            public const string CuePoint = "Cue Point";
            public const string Lyric = "Lyric";
            public const string Text = "Text";
            public const string SequenceNumber = "Sequence Number";
            public const string PortPrefix = "Port Prefix";
            public const string ChannelPrefix = "Channel Prefix";
            public const string TimeSignature = "Time Signature";
            public const string KeySignature = "Key Signature";
            public const string SetTempo = "Set Tempo";
            public const string SmpteOffset = "SMPTE Offset";
            public const string SequencerSpecific = "Sequencer Specific";
            public const string UnknownMeta = "Unknown Meta";
            public const string NoteOn = "Note On";
            public const string NoteOff = "Note Off";
            public const string PitchBend = "Pitch Bend";
            public const string ControlChange = "Control Change";
            public const string ProgramChange = "Program Change";
            public const string ChannelAftertouch = "Channel Aftertouch";
            public const string NoteAftertouch = "Note Aftertouch";
            public const string SysExCompleted = "System Exclusive";
            public const string SysExIncompleted = "System Exclusive Packet";
        }

        public const string Note = "Note";
    }
}
