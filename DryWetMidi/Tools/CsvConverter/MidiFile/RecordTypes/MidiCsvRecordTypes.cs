namespace Melanchall.DryWetMidi.Tools
{
    internal static class MidiCsvRecordTypes
    {
        public static class File
        {
            public const string Header = "Header";
            public const string TrackChunkStart = "Start_track";
            public const string TrackChunkEnd = "End_track";
            public const string FileEnd = "End_of_file";
        }

        public static class Events
        {
            public const string SequenceTrackName = "Title_t";
            public const string CopyrightNotice = "Copyright_t";
            public const string InstrumentName = "Instrument_name_t";
            public const string Marker = "Marker_t";
            public const string CuePoint = "Cue_point_t";
            public const string Lyric = "Lyric_t";
            public const string Text = "Text_t";
            public const string SequenceNumber = "Sequence_number";
            public const string PortPrefix = "MIDI_port";
            public const string ChannelPrefix = "Channel_prefix";
            public const string TimeSignature = "Time_signature";
            public const string KeySignature = "Key_signature";
            public const string SetTempo = "Tempo";
            public const string SmpteOffset = "SMPTE_offset";
            public const string SequencerSpecific = "Sequencer_specific";
            public const string UnknownMeta = "Unknown_meta_event";
            public const string NoteOn = "Note_on_c";
            public const string NoteOff = "Note_off_c";
            public const string PitchBend = "Pitch_bend_c";
            public const string ControlChange = "Control_c";
            public const string ProgramChange = "Program_c";
            public const string ChannelAftertouch = "Channel_aftertouch_c";
            public const string NoteAftertouch = "Poly_aftertouch_c";
            public const string SysExCompleted = "System_exclusive";
            public const string SysExIncompleted = "System_exclusive_packet";
        }
    }
}
