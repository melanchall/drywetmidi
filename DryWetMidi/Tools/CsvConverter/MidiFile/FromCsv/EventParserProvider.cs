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

        private static readonly Dictionary<string, EventParser> EventsParsers =
            new Dictionary<string, EventParser>(StringComparer.OrdinalIgnoreCase)
            {
                [RecordLabels.Events.SequenceTrackName] = GetTextEventParser<SequenceTrackNameEvent>(),
                [RecordLabels.Events.CopyrightNotice] = GetTextEventParser<CopyrightNoticeEvent>(),
                [RecordLabels.Events.InstrumentName] = GetTextEventParser<InstrumentNameEvent>(),
                [RecordLabels.Events.Marker] = GetTextEventParser<MarkerEvent>(),
                [RecordLabels.Events.CuePoint] = GetTextEventParser<CuePointEvent>(),
                [RecordLabels.Events.Lyric] = GetTextEventParser<LyricEvent>(),
                [RecordLabels.Events.Text] = GetTextEventParser<TextEvent>(),
                [RecordLabels.Events.SequenceNumber] = GetEventParser(
                    x => new SequenceNumberEvent((ushort)x[0]),
                    TypeParser.UShort),
                [RecordLabels.Events.PortPrefix] = GetEventParser(
                    x => new PortPrefixEvent((byte)x[0]),
                    TypeParser.Byte),
                [RecordLabels.Events.ChannelPrefix] = GetEventParser(
                    x => new ChannelPrefixEvent((byte)x[0]),
                    TypeParser.Byte),
                [RecordLabels.Events.TimeSignature] = GetEventParser(
                    x => new TimeSignatureEvent((byte)x[0], (byte)x[1], (byte)x[2], (byte)x[3]),
                    TypeParser.Byte,
                    TypeParser.Byte,
                    TypeParser.Byte,
                    TypeParser.Byte),
                [RecordLabels.Events.KeySignature] = GetEventParser(
                    x => new KeySignatureEvent((sbyte)x[0], (byte)x[1]),
                    TypeParser.SByte,
                    TypeParser.Byte),
                [RecordLabels.Events.SetTempo] = GetEventParser(
                    x => new SetTempoEvent((long)x[0]),
                    TypeParser.Long),
                [RecordLabels.Events.SmpteOffset] = GetEventParser(
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
                [RecordLabels.Events.SequencerSpecific] = GetBytesBasedEventParser(
                    x => new SequencerSpecificEvent((byte[])x[1])),
                [RecordLabels.Events.UnknownMeta] = GetBytesBasedEventParser(
                    x => new UnknownMetaEvent((byte)x[0], (byte[])x[2]),
                    TypeParser.Byte),
                [RecordLabels.Events.NoteOn] = GetNoteEventParser<NoteOnEvent>(2),
                [RecordLabels.Events.NoteOff] = GetNoteEventParser<NoteOffEvent>(2),
                [RecordLabels.Events.PitchBend] = GetEventParser(
                    x => new PitchBendEvent((ushort)x[1]) { Channel = (FourBitNumber)x[0] },
                    TypeParser.FourBitNumber,
                    TypeParser.UShort),
                [RecordLabels.Events.ControlChange] = GetChannelEventParser<ControlChangeEvent>(2),
                [RecordLabels.Events.ProgramChange] = GetChannelEventParser<ProgramChangeEvent>(1),
                [RecordLabels.Events.ChannelAftertouch] = GetChannelEventParser<ChannelAftertouchEvent>(1),
                [RecordLabels.Events.NoteAftertouch] = GetNoteEventParser<ChannelAftertouchEvent>(2),
                [RecordLabels.Events.SysExCompleted] = GetBytesBasedEventParser(
                    x => new NormalSysExEvent((byte[])x[1])),
                [RecordLabels.Events.SysExIncompleted] = GetBytesBasedEventParser(
                    x => new NormalSysExEvent((byte[])x[1])),
            };

        #endregion

        #region Methods

        public static EventParser Get(string eventName)
        {
            return EventsParsers[eventName];
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
