using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Tools
{
    internal static class EventParserProvider
    {
        #region Constants

        private static readonly Dictionary<string, EventParser> EventsParsers_MidiCsv =
            new Dictionary<string, EventParser>(StringComparer.OrdinalIgnoreCase)
            {
                [MidiCsvRecordTypes.Events.SequenceTrackName] = GetTextEventParser<SequenceTrackNameEvent>(),
                [MidiCsvRecordTypes.Events.CopyrightNotice] = GetTextEventParser<CopyrightNoticeEvent>(),
                [MidiCsvRecordTypes.Events.InstrumentName] = GetTextEventParser<InstrumentNameEvent>(),
                [MidiCsvRecordTypes.Events.Marker] = GetTextEventParser<MarkerEvent>(),
                [MidiCsvRecordTypes.Events.CuePoint] = GetTextEventParser<CuePointEvent>(),
                [MidiCsvRecordTypes.Events.Lyric] = GetTextEventParser<LyricEvent>(),
                [MidiCsvRecordTypes.Events.Text] = GetTextEventParser<TextEvent>(),
                [MidiCsvRecordTypes.Events.SequenceNumber] = GetEventParser(
                    x => new SequenceNumberEvent((ushort)x[0]),
                    TypeParser.UShort),
                [MidiCsvRecordTypes.Events.PortPrefix] = GetEventParser(
                    x => new PortPrefixEvent((byte)x[0]),
                    TypeParser.Byte),
                [MidiCsvRecordTypes.Events.ChannelPrefix] = GetEventParser(
                    x => new ChannelPrefixEvent((byte)x[0]),
                    TypeParser.Byte),
                [MidiCsvRecordTypes.Events.TimeSignature] = GetEventParser(
                    x => new TimeSignatureEvent((byte)x[0], (byte)Math.Pow(2, (byte)x[1]), (byte)x[2], (byte)x[3]),
                    TypeParser.Byte,
                    TypeParser.Byte,
                    TypeParser.Byte,
                    TypeParser.Byte),
                [MidiCsvRecordTypes.Events.KeySignature] = GetEventParser(
                    x => new KeySignatureEvent((sbyte)x[0], (byte)x[1]),
                    TypeParser.SByte,
                    TypeParser.Byte),
                [MidiCsvRecordTypes.Events.SetTempo] = GetEventParser(
                    x => new SetTempoEvent((long)x[0]),
                    TypeParser.Long),
                [MidiCsvRecordTypes.Events.SmpteOffset] = GetEventParser(
                    x => new SmpteOffsetEvent(SmpteData.GetFormat((byte)x[0]),
                                              SmpteData.GetHours((byte)x[0]),
                                              (byte)x[1],
                                              (byte)x[2],
                                              (byte)x[3],
                                              (byte)x[4]),
                    TypeParser.Byte,
                    TypeParser.Byte,
                    TypeParser.Byte,
                    TypeParser.Byte,
                    TypeParser.Byte),
                [MidiCsvRecordTypes.Events.SequencerSpecific] = GetBytesBasedEventParser(
                    x => new SequencerSpecificEvent((byte[])x[1])),
                [MidiCsvRecordTypes.Events.UnknownMeta] = GetBytesBasedEventParser(
                    x => new UnknownMetaEvent((byte)x[0], (byte[])x[2]),
                    TypeParser.Byte),
                [MidiCsvRecordTypes.Events.NoteOn] = GetNoteEventParser<NoteOnEvent>(2),
                [MidiCsvRecordTypes.Events.NoteOff] = GetNoteEventParser<NoteOffEvent>(2),
                [MidiCsvRecordTypes.Events.PitchBend] = GetEventParser(
                    x => new PitchBendEvent((ushort)x[1]) { Channel = (FourBitNumber)x[0] },
                    TypeParser.FourBitNumber,
                    TypeParser.UShort),
                [MidiCsvRecordTypes.Events.ControlChange] = GetChannelEventParser<ControlChangeEvent>(2),
                [MidiCsvRecordTypes.Events.ProgramChange] = GetChannelEventParser<ProgramChangeEvent>(1),
                [MidiCsvRecordTypes.Events.ChannelAftertouch] = GetChannelEventParser<ChannelAftertouchEvent>(1),
                [MidiCsvRecordTypes.Events.NoteAftertouch] = GetNoteEventParser<ChannelAftertouchEvent>(2),
                [MidiCsvRecordTypes.Events.SysExCompleted] = GetBytesBasedEventParser(
                    x => new NormalSysExEvent((byte[])x[1])),
                [MidiCsvRecordTypes.Events.SysExIncompleted] = GetBytesBasedEventParser(
                    x => new NormalSysExEvent((byte[])x[1])),
            };

        private static readonly Dictionary<string, EventParser> EventsParsers_DryWetMidi =
            new Dictionary<string, EventParser>(StringComparer.OrdinalIgnoreCase)
            {
                [DryWetMidiRecordTypes.Events.SequenceTrackName] = GetTextEventParser<SequenceTrackNameEvent>(),
                [DryWetMidiRecordTypes.Events.CopyrightNotice] = GetTextEventParser<CopyrightNoticeEvent>(),
                [DryWetMidiRecordTypes.Events.InstrumentName] = GetTextEventParser<InstrumentNameEvent>(),
                [DryWetMidiRecordTypes.Events.Marker] = GetTextEventParser<MarkerEvent>(),
                [DryWetMidiRecordTypes.Events.CuePoint] = GetTextEventParser<CuePointEvent>(),
                [DryWetMidiRecordTypes.Events.Lyric] = GetTextEventParser<LyricEvent>(),
                [DryWetMidiRecordTypes.Events.Text] = GetTextEventParser<TextEvent>(),
                [DryWetMidiRecordTypes.Events.SequenceNumber] = GetEventParser(
                    x => new SequenceNumberEvent((ushort)x[0]),
                    TypeParser.UShort),
                [DryWetMidiRecordTypes.Events.PortPrefix] = GetEventParser(
                    x => new PortPrefixEvent((byte)x[0]),
                    TypeParser.Byte),
                [DryWetMidiRecordTypes.Events.ChannelPrefix] = GetEventParser(
                    x => new ChannelPrefixEvent((byte)x[0]),
                    TypeParser.Byte),
                [DryWetMidiRecordTypes.Events.TimeSignature] = GetEventParser(
                    x => new TimeSignatureEvent((byte)x[0], (byte)x[1], (byte)x[2], (byte)x[3]),
                    TypeParser.Byte,
                    TypeParser.Byte,
                    TypeParser.Byte,
                    TypeParser.Byte),
                [DryWetMidiRecordTypes.Events.KeySignature] = GetEventParser(
                    x => new KeySignatureEvent((sbyte)x[0], (byte)x[1]),
                    TypeParser.SByte,
                    TypeParser.Byte),
                [DryWetMidiRecordTypes.Events.SetTempo] = GetEventParser(
                    x => new SetTempoEvent((long)x[0]),
                    TypeParser.Long),
                [DryWetMidiRecordTypes.Events.SmpteOffset] = GetEventParser(
                    x => new SmpteOffsetEvent(SmpteData.GetFormat((byte)x[0]),
                                              SmpteData.GetHours((byte)x[0]),
                                              (byte)x[1],
                                              (byte)x[2],
                                              (byte)x[3],
                                              (byte)x[4]),
                    TypeParser.Byte,
                    TypeParser.Byte,
                    TypeParser.Byte,
                    TypeParser.Byte,
                    TypeParser.Byte),
                [DryWetMidiRecordTypes.Events.SequencerSpecific] = GetBytesBasedEventParser(
                    x => new SequencerSpecificEvent((byte[])x[1])),
                [DryWetMidiRecordTypes.Events.UnknownMeta] = GetBytesBasedEventParser(
                    x => new UnknownMetaEvent((byte)x[0], (byte[])x[2]),
                    TypeParser.Byte),
                [DryWetMidiRecordTypes.Events.NoteOn] = GetNoteEventParser<NoteOnEvent>(2),
                [DryWetMidiRecordTypes.Events.NoteOff] = GetNoteEventParser<NoteOffEvent>(2),
                [DryWetMidiRecordTypes.Events.PitchBend] = GetEventParser(
                    x => new PitchBendEvent((ushort)x[1]) { Channel = (FourBitNumber)x[0] },
                    TypeParser.FourBitNumber,
                    TypeParser.UShort),
                [DryWetMidiRecordTypes.Events.ControlChange] = GetChannelEventParser<ControlChangeEvent>(2),
                [DryWetMidiRecordTypes.Events.ProgramChange] = GetChannelEventParser<ProgramChangeEvent>(1),
                [DryWetMidiRecordTypes.Events.ChannelAftertouch] = GetChannelEventParser<ChannelAftertouchEvent>(1),
                [DryWetMidiRecordTypes.Events.NoteAftertouch] = GetNoteEventParser<ChannelAftertouchEvent>(2),
                [DryWetMidiRecordTypes.Events.SysExCompleted] = GetBytesBasedEventParser(
                    x => new NormalSysExEvent((byte[])x[1])),
                [DryWetMidiRecordTypes.Events.SysExIncompleted] = GetBytesBasedEventParser(
                    x => new NormalSysExEvent((byte[])x[1])),
            };

        #endregion

        #region Methods

        public static EventParser Get(string eventName, MidiFileCsvLayout layout)
        {
            switch (layout)
            {
                case MidiFileCsvLayout.DryWetMidi:
                    return EventsParsers_DryWetMidi[eventName];
                case MidiFileCsvLayout.MidiCsv:
                    return EventsParsers_MidiCsv[eventName];
            }

            return null;
        }

        private static EventParser GetBytesBasedEventParser(Func<object[], MidiEvent> eventCreator, params ParameterParser[] parametersParsers)
        {
            return (p, s) =>
            {
                if (p.Length < parametersParsers.Length)
                    CsvError.ThrowBadFormat("Invalid number of parameters provided.");

                var parameters = new List<object>(parametersParsers.Length);
                var i = 0;

                for (i = 0; i < parametersParsers.Length; i++)
                {
                    var parameterParser = parametersParsers[i];

                    try
                    {
                        var parameter = parameterParser(p[i], s);
                        parameters.Add(parameter);
                    }
                    catch
                    {
                        CsvError.ThrowBadFormat($"Parameter ({i}) is invalid.");
                    }
                }

                if (p.Length < i)
                    CsvError.ThrowBadFormat("Invalid number of parameters provided.");

                int bytesNumber = 0;

                try
                {
                    bytesNumber = int.Parse(p[i]);
                    parameters.Add(bytesNumber);
                }
                catch
                {
                    CsvError.ThrowBadFormat($"Parameter ({i}) is invalid.");
                }

                i++;
                if (p.Length < i + bytesNumber)
                    CsvError.ThrowBadFormat("Invalid number of parameters provided.");

                try
                {
                    var bytes = p.Skip(i)
                                 .Select(x =>
                                 {
                                     var b = (byte)TypeParser.Byte(x, s);
                                     i++;
                                     return b;
                                 })
                                 .ToArray();
                    parameters.Add(bytes);
                }
                catch
                {
                    CsvError.ThrowBadFormat($"Parameter ({i}) is invalid.");
                }

                return eventCreator(parameters.ToArray());
            };
        }

        private static EventParser GetTextEventParser<TEvent>()
            where TEvent : BaseTextEvent
        {
            return GetEventParser(
                x =>
                {
                    var textEvent = (BaseTextEvent)Activator.CreateInstance<TEvent>();
                    textEvent.Text = (string)x[0];
                    return textEvent;
                },
                TypeParser.String);
        }

        private static EventParser GetNoteEventParser<TEvent>(int parametersNumber)
            where TEvent : ChannelEvent
        {
            return GetChannelEventParser<TEvent>(
                new[] { TypeParser.NoteNumber }
                .Concat(Enumerable.Range(0, parametersNumber - 1).Select(i => TypeParser.SevenBitNumber))
                .ToArray());
        }

        private static EventParser GetChannelEventParser<TEvent>(int parametersNumber)
            where TEvent : ChannelEvent
        {
            return GetChannelEventParser<TEvent>(Enumerable.Range(0, parametersNumber)
                                                           .Select(i => TypeParser.SevenBitNumber)
                                                           .ToArray());
        }

        private static EventParser GetChannelEventParser<TEvent>(ParameterParser[] parametersParsers)
            where TEvent : ChannelEvent
        {
            return GetEventParser(
                x =>
                {
                    var channelEvent = Activator.CreateInstance<TEvent>();
                    channelEvent.Channel = (FourBitNumber)x[0];

                    channelEvent._dataByte1 = Convert.ToByte(x[1]);
                    if (x.Length > 2)
                        channelEvent._dataByte2 = Convert.ToByte(x[2]);

                    return channelEvent;
                },
                new[] { TypeParser.FourBitNumber }
                    .Concat(parametersParsers)
                    .ToArray());
        }

        private static EventParser GetEventParser(Func<object[], MidiEvent> eventCreator, params ParameterParser[] parametersParsers)
        {
            return (p, s) =>
            {
                if (p.Length < parametersParsers.Length)
                    CsvError.ThrowBadFormat("Invalid number of parameters provided.");

                var parameters = new List<object>(parametersParsers.Length);

                for (int i = 0; i < parametersParsers.Length; i++)
                {
                    var parameterParser = parametersParsers[i];

                    try
                    {
                        var parameter = parameterParser(p[i], s);
                        parameters.Add(parameter);
                    }
                    catch
                    {
                        CsvError.ThrowBadFormat($"Parameter ({i}) is invalid.");
                    }
                }

                return eventCreator(parameters.ToArray());
            };
        }

        #endregion
    }
}
