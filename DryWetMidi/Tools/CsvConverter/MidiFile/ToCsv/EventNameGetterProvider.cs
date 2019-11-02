using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Tools
{
    internal static class EventNameGetterProvider
    {
        #region Constants

        private static readonly Dictionary<Type, EventNameGetter> EventsTypes_MidiCsv =
            new Dictionary<Type, EventNameGetter>
            {
                [typeof(SequenceTrackNameEvent)] = GetType(MidiCsvRecordTypes.Events.SequenceTrackName),
                [typeof(CopyrightNoticeEvent)]   = GetType(MidiCsvRecordTypes.Events.CopyrightNotice),
                [typeof(InstrumentNameEvent)]    = GetType(MidiCsvRecordTypes.Events.InstrumentName),
                [typeof(MarkerEvent)]            = GetType(MidiCsvRecordTypes.Events.Marker),
                [typeof(CuePointEvent)]          = GetType(MidiCsvRecordTypes.Events.CuePoint),
                [typeof(LyricEvent)]             = GetType(MidiCsvRecordTypes.Events.Lyric),
                [typeof(TextEvent)]              = GetType(MidiCsvRecordTypes.Events.Text),
                [typeof(SequenceNumberEvent)]    = GetType(MidiCsvRecordTypes.Events.SequenceNumber),
                [typeof(PortPrefixEvent)]        = GetType(MidiCsvRecordTypes.Events.PortPrefix),
                [typeof(ChannelPrefixEvent)]     = GetType(MidiCsvRecordTypes.Events.ChannelPrefix),
                [typeof(TimeSignatureEvent)]     = GetType(MidiCsvRecordTypes.Events.TimeSignature),
                [typeof(KeySignatureEvent)]      = GetType(MidiCsvRecordTypes.Events.KeySignature),
                [typeof(SetTempoEvent)]          = GetType(MidiCsvRecordTypes.Events.SetTempo),
                [typeof(SmpteOffsetEvent)]       = GetType(MidiCsvRecordTypes.Events.SmpteOffset),
                [typeof(SequencerSpecificEvent)] = GetType(MidiCsvRecordTypes.Events.SequencerSpecific),
                [typeof(UnknownMetaEvent)]       = GetType(MidiCsvRecordTypes.Events.UnknownMeta),
                [typeof(NoteOnEvent)]            = GetType(MidiCsvRecordTypes.Events.NoteOn),
                [typeof(NoteOffEvent)]           = GetType(MidiCsvRecordTypes.Events.NoteOff),
                [typeof(PitchBendEvent)]         = GetType(MidiCsvRecordTypes.Events.PitchBend),
                [typeof(ControlChangeEvent)]     = GetType(MidiCsvRecordTypes.Events.ControlChange),
                [typeof(ProgramChangeEvent)]     = GetType(MidiCsvRecordTypes.Events.ProgramChange),
                [typeof(ChannelAftertouchEvent)] = GetType(MidiCsvRecordTypes.Events.ChannelAftertouch),
                [typeof(NoteAftertouchEvent)]    = GetType(MidiCsvRecordTypes.Events.NoteAftertouch),
                [typeof(NormalSysExEvent)]       = GetSysExType(MidiCsvRecordTypes.Events.SysExCompleted,
                                                                MidiCsvRecordTypes.Events.SysExIncompleted),
                [typeof(EscapeSysExEvent)]       = GetSysExType(MidiCsvRecordTypes.Events.SysExCompleted,
                                                                MidiCsvRecordTypes.Events.SysExIncompleted)
            };

        private static readonly Dictionary<Type, EventNameGetter> EventsTypes_DryWetMidi =
            new Dictionary<Type, EventNameGetter>
            {
                [typeof(SequenceTrackNameEvent)] = GetType(DryWetMidiRecordTypes.Events.SequenceTrackName),
                [typeof(CopyrightNoticeEvent)]   = GetType(DryWetMidiRecordTypes.Events.CopyrightNotice),
                [typeof(InstrumentNameEvent)]    = GetType(DryWetMidiRecordTypes.Events.InstrumentName),
                [typeof(MarkerEvent)]            = GetType(DryWetMidiRecordTypes.Events.Marker),
                [typeof(CuePointEvent)]          = GetType(DryWetMidiRecordTypes.Events.CuePoint),
                [typeof(LyricEvent)]             = GetType(DryWetMidiRecordTypes.Events.Lyric),
                [typeof(TextEvent)]              = GetType(DryWetMidiRecordTypes.Events.Text),
                [typeof(SequenceNumberEvent)]    = GetType(DryWetMidiRecordTypes.Events.SequenceNumber),
                [typeof(PortPrefixEvent)]        = GetType(DryWetMidiRecordTypes.Events.PortPrefix),
                [typeof(ChannelPrefixEvent)]     = GetType(DryWetMidiRecordTypes.Events.ChannelPrefix),
                [typeof(TimeSignatureEvent)]     = GetType(DryWetMidiRecordTypes.Events.TimeSignature),
                [typeof(KeySignatureEvent)]      = GetType(DryWetMidiRecordTypes.Events.KeySignature),
                [typeof(SetTempoEvent)]          = GetType(DryWetMidiRecordTypes.Events.SetTempo),
                [typeof(SmpteOffsetEvent)]       = GetType(DryWetMidiRecordTypes.Events.SmpteOffset),
                [typeof(SequencerSpecificEvent)] = GetType(DryWetMidiRecordTypes.Events.SequencerSpecific),
                [typeof(UnknownMetaEvent)]       = GetType(DryWetMidiRecordTypes.Events.UnknownMeta),
                [typeof(NoteOnEvent)]            = GetType(DryWetMidiRecordTypes.Events.NoteOn),
                [typeof(NoteOffEvent)]           = GetType(DryWetMidiRecordTypes.Events.NoteOff),
                [typeof(PitchBendEvent)]         = GetType(DryWetMidiRecordTypes.Events.PitchBend),
                [typeof(ControlChangeEvent)]     = GetType(DryWetMidiRecordTypes.Events.ControlChange),
                [typeof(ProgramChangeEvent)]     = GetType(DryWetMidiRecordTypes.Events.ProgramChange),
                [typeof(ChannelAftertouchEvent)] = GetType(DryWetMidiRecordTypes.Events.ChannelAftertouch),
                [typeof(NoteAftertouchEvent)]    = GetType(DryWetMidiRecordTypes.Events.NoteAftertouch),
                [typeof(NormalSysExEvent)]       = GetSysExType(DryWetMidiRecordTypes.Events.SysExCompleted,
                                                                DryWetMidiRecordTypes.Events.SysExIncompleted),
                [typeof(EscapeSysExEvent)]       = GetSysExType(DryWetMidiRecordTypes.Events.SysExCompleted,
                                                                DryWetMidiRecordTypes.Events.SysExIncompleted)
            };

        #endregion

        #region Methods

        public static EventNameGetter Get(Type eventType, MidiFileCsvLayout layout)
        {
            switch (layout)
            {
                case MidiFileCsvLayout.DryWetMidi:
                    return EventsTypes_DryWetMidi[eventType];
                case MidiFileCsvLayout.MidiCsv:
                    return EventsTypes_MidiCsv[eventType];
            }

            return null;
        }

        private static EventNameGetter GetType(string type)
        {
            return e => type;
        }

        private static EventNameGetter GetSysExType(string completedType, string incompletedType)
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
