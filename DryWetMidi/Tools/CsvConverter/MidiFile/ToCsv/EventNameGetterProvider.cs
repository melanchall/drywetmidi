using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Tools
{
    internal static class EventNameGetterProvider
    {
        #region Constants

        private static readonly Dictionary<Type, EventNameGetter> EventsTypes =
            new Dictionary<Type, EventNameGetter>
            {
                [typeof(SequenceTrackNameEvent)] = GetType(RecordLabels.Events.SequenceTrackName),
                [typeof(CopyrightNoticeEvent)]   = GetType(RecordLabels.Events.CopyrightNotice),
                [typeof(InstrumentNameEvent)]    = GetType(RecordLabels.Events.InstrumentName),
                [typeof(MarkerEvent)]            = GetType(RecordLabels.Events.Marker),
                [typeof(CuePointEvent)]          = GetType(RecordLabels.Events.CuePoint),
                [typeof(LyricEvent)]             = GetType(RecordLabels.Events.Lyric),
                [typeof(TextEvent)]              = GetType(RecordLabels.Events.Text),
                [typeof(SequenceNumberEvent)]    = GetType(RecordLabels.Events.SequenceNumber),
                [typeof(PortPrefixEvent)]        = GetType(RecordLabels.Events.PortPrefix),
                [typeof(ChannelPrefixEvent)]     = GetType(RecordLabels.Events.ChannelPrefix),
                [typeof(TimeSignatureEvent)]     = GetType(RecordLabels.Events.TimeSignature),
                [typeof(KeySignatureEvent)]      = GetType(RecordLabels.Events.KeySignature),
                [typeof(SetTempoEvent)]          = GetType(RecordLabels.Events.SetTempo),
                [typeof(SmpteOffsetEvent)]       = GetType(RecordLabels.Events.SmpteOffset),
                [typeof(SequencerSpecificEvent)] = GetType(RecordLabels.Events.SequencerSpecific),
                [typeof(UnknownMetaEvent)]       = GetType(RecordLabels.Events.UnknownMeta),
                [typeof(NoteOnEvent)]            = GetType(RecordLabels.Events.NoteOn),
                [typeof(NoteOffEvent)]           = GetType(RecordLabels.Events.NoteOff),
                [typeof(PitchBendEvent)]         = GetType(RecordLabels.Events.PitchBend),
                [typeof(ControlChangeEvent)]     = GetType(RecordLabels.Events.ControlChange),
                [typeof(ProgramChangeEvent)]     = GetType(RecordLabels.Events.ProgramChange),
                [typeof(ChannelAftertouchEvent)] = GetType(RecordLabels.Events.ChannelAftertouch),
                [typeof(NoteAftertouchEvent)]    = GetType(RecordLabels.Events.NoteAftertouch),
                [typeof(NormalSysExEvent)]       = GetSysExType(RecordLabels.Events.SysExCompleted,
                                                                RecordLabels.Events.SysExIncompleted),
                [typeof(EscapeSysExEvent)]       = GetSysExType(RecordLabels.Events.SysExCompleted,
                                                                RecordLabels.Events.SysExIncompleted)
            };

        #endregion

        #region Methods

        public static EventNameGetter Get(Type eventType)
        {
            return EventsTypes[eventType];
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
