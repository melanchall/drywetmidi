using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Tools
{
    internal static class EventParametersGetterProvider
    {
        #region Constants

        private static readonly Dictionary<Type, EventParametersGetter> EventsParametersGetters =
            new Dictionary<Type, EventParametersGetter>
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
                                                                                         case MidiFileCsvLayout.MidiCsv:
                                                                                             return (byte)Math.Log(e.Denominator, 2);
                                                                                     }

                                                                                     return null;
                                                                                 },
                                                                                 (e, s) => e.ClocksPerClick,
                                                                                 (e, s) => e.ThirtySecondNotesPerBeat),
                [typeof(KeySignatureEvent)] = GetParameters<KeySignatureEvent>((e, s) => e.Key,
                                                                               (e, s) => e.Scale),
                [typeof(SetTempoEvent)] = GetParameters<SetTempoEvent>((e, s) => e.MicrosecondsPerQuarterNote),
                [typeof(SmpteOffsetEvent)] = GetParameters<SmpteOffsetEvent>((e, s) => SmpteData.GetFormatAndHours(e.Format, e.Hours),
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
                                                                   (e, s) => FormatNoteNumber(e.NoteNumber, s),
                                                                   (e, s) => e.Velocity),
                [typeof(NoteOffEvent)] = GetParameters<NoteOffEvent>((e, s) => e.Channel,
                                                                     (e, s) => FormatNoteNumber(e.NoteNumber, s),
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
                                                                                   (e, s) => FormatNoteNumber(e.NoteNumber, s),
                                                                                   (e, s) => e.AftertouchValue),
                [typeof(NormalSysExEvent)] = GetParameters<NormalSysExEvent>((e, s) => e.Data.Length,
                                                                             (e, s) => e.Data),
                [typeof(EscapeSysExEvent)] = GetParameters<EscapeSysExEvent>((e, s) => e.Data.Length,
                                                                             (e, s) => e.Data)
            };

        #endregion

        #region Methods

        public static EventParametersGetter Get(Type eventType)
        {
            return EventsParametersGetters[eventType];
        }

        private static EventParametersGetter GetParameters<T>(params Func<T, MidiFileCsvConversionSettings, object>[] parametersGetters)
            where T : MidiEvent
        {
            return (e, s) => parametersGetters.Select(g => g((T)e, s)).ToArray();
        }

        private static object FormatNoteNumber(SevenBitNumber noteNumber, MidiFileCsvConversionSettings settings)
        {
            if (settings.CsvLayout == MidiFileCsvLayout.MidiCsv)
                return noteNumber;

            return NoteCsvConversionUtilities.FormatNoteNumber(noteNumber, settings.NoteNumberFormat);
        }

        #endregion
    }
}
