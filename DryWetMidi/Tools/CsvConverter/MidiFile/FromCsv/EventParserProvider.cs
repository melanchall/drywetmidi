using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;

namespace Melanchall.DryWetMidi.Tools
{
    internal static class EventParserProvider
    {
        #region Constants

        private static class TypeParsers
        {
            public static readonly Func<string, object> Byte = p => byte.Parse(p);
            public static readonly Func<string, object> SByte = p => sbyte.Parse(p);
            public static readonly Func<string, object> Long = p => long.Parse(p);
            public static readonly Func<string, object> UShort = p => ushort.Parse(p);
            public static readonly Func<string, object> String = p => p.Trim('"');
            public static readonly Func<string, object> Int = p => int.Parse(p);
            public static readonly Func<string, object> FourBitNumber = p => (FourBitNumber)byte.Parse(p);
            public static readonly Func<string, object> SevenBitNumber = p => (SevenBitNumber)byte.Parse(p);
        }

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
                    TypeParsers.UShort),
                [MidiCsvRecordTypes.Events.PortPrefix] = GetEventParser(
                    x => new PortPrefixEvent((byte)x[0]),
                    TypeParsers.Byte),
                [MidiCsvRecordTypes.Events.ChannelPrefix] = GetEventParser(
                    x => new ChannelPrefixEvent((byte)x[0]),
                    TypeParsers.Byte),
                [MidiCsvRecordTypes.Events.TimeSignature] = GetEventParser(
                    x => new TimeSignatureEvent((byte)x[0], (byte)Math.Pow(2, (byte)x[1]), (byte)x[2], (byte)x[3]),
                    TypeParsers.Byte,
                    TypeParsers.Byte,
                    TypeParsers.Byte,
                    TypeParsers.Byte),
                [MidiCsvRecordTypes.Events.KeySignature] = GetEventParser(
                    x => new KeySignatureEvent((sbyte)x[0], (byte)x[1]),
                    TypeParsers.SByte,
                    TypeParsers.Byte),
                [MidiCsvRecordTypes.Events.SetTempo] = GetEventParser(
                    x => new SetTempoEvent((long)x[0]),
                    TypeParsers.Long),
                [MidiCsvRecordTypes.Events.SmpteOffset] = GetEventParser(
                    x => new SmpteOffsetEvent(SmpteOffsetEvent.GetFormat((byte)x[0]),
                                              SmpteOffsetEvent.GetHours((byte)x[0]),
                                              (byte)x[1],
                                              (byte)x[2],
                                              (byte)x[3],
                                              (byte)x[4]),
                    TypeParsers.Byte,
                    TypeParsers.Byte,
                    TypeParsers.Byte,
                    TypeParsers.Byte,
                    TypeParsers.Byte),
                [MidiCsvRecordTypes.Events.SequencerSpecific] = GetBytesBasedEventParser(
                    x => new SequencerSpecificEvent((byte[])x[1])),
                [MidiCsvRecordTypes.Events.UnknownMeta] = GetBytesBasedEventParser(
                    x => new UnknownMetaEvent((byte)x[0], (byte[])x[2]),
                    TypeParsers.Byte),
                [MidiCsvRecordTypes.Events.NoteOn] = GetChannelEventParser<NoteOnEvent>(2),
                [MidiCsvRecordTypes.Events.NoteOff] = GetChannelEventParser<NoteOffEvent>(2),
                [MidiCsvRecordTypes.Events.PitchBend] = GetEventParser(
                    x => new PitchBendEvent((ushort)x[1]) { Channel = (FourBitNumber)x[0] },
                    TypeParsers.FourBitNumber,
                    TypeParsers.UShort),
                [MidiCsvRecordTypes.Events.ControlChange] = GetChannelEventParser<ControlChangeEvent>(2),
                [MidiCsvRecordTypes.Events.ProgramChange] = GetChannelEventParser<ProgramChangeEvent>(1),
                [MidiCsvRecordTypes.Events.ChannelAftertouch] = GetChannelEventParser<ChannelAftertouchEvent>(1),
                [MidiCsvRecordTypes.Events.NoteAftertouch] = GetChannelEventParser<ChannelAftertouchEvent>(2),
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
                    TypeParsers.UShort),
                [DryWetMidiRecordTypes.Events.PortPrefix] = GetEventParser(
                    x => new PortPrefixEvent((byte)x[0]),
                    TypeParsers.Byte),
                [DryWetMidiRecordTypes.Events.ChannelPrefix] = GetEventParser(
                    x => new ChannelPrefixEvent((byte)x[0]),
                    TypeParsers.Byte),
                [DryWetMidiRecordTypes.Events.TimeSignature] = GetEventParser(
                    x => new TimeSignatureEvent((byte)x[0], (byte)x[1], (byte)x[2], (byte)x[3]),
                    TypeParsers.Byte,
                    TypeParsers.Byte,
                    TypeParsers.Byte,
                    TypeParsers.Byte),
                [DryWetMidiRecordTypes.Events.KeySignature] = GetEventParser(
                    x => new KeySignatureEvent((sbyte)x[0], (byte)x[1]),
                    TypeParsers.SByte,
                    TypeParsers.Byte),
                [DryWetMidiRecordTypes.Events.SetTempo] = GetEventParser(
                    x => new SetTempoEvent((long)x[0]),
                    TypeParsers.Long),
                [DryWetMidiRecordTypes.Events.SmpteOffset] = GetEventParser(
                    x => new SmpteOffsetEvent(SmpteOffsetEvent.GetFormat((byte)x[0]),
                                              SmpteOffsetEvent.GetHours((byte)x[0]),
                                              (byte)x[1],
                                              (byte)x[2],
                                              (byte)x[3],
                                              (byte)x[4]),
                    TypeParsers.Byte,
                    TypeParsers.Byte,
                    TypeParsers.Byte,
                    TypeParsers.Byte,
                    TypeParsers.Byte),
                [DryWetMidiRecordTypes.Events.SequencerSpecific] = GetBytesBasedEventParser(
                    x => new SequencerSpecificEvent((byte[])x[1])),
                [DryWetMidiRecordTypes.Events.UnknownMeta] = GetBytesBasedEventParser(
                    x => new UnknownMetaEvent((byte)x[0], (byte[])x[2]),
                    TypeParsers.Byte),
                [DryWetMidiRecordTypes.Events.NoteOn] = GetChannelEventParser<NoteOnEvent>(2),
                [DryWetMidiRecordTypes.Events.NoteOff] = GetChannelEventParser<NoteOffEvent>(2),
                [DryWetMidiRecordTypes.Events.PitchBend] = GetEventParser(
                    x => new PitchBendEvent((ushort)x[1]) { Channel = (FourBitNumber)x[0] },
                    TypeParsers.FourBitNumber,
                    TypeParsers.UShort),
                [DryWetMidiRecordTypes.Events.ControlChange] = GetChannelEventParser<ControlChangeEvent>(2),
                [DryWetMidiRecordTypes.Events.ProgramChange] = GetChannelEventParser<ProgramChangeEvent>(1),
                [DryWetMidiRecordTypes.Events.ChannelAftertouch] = GetChannelEventParser<ChannelAftertouchEvent>(1),
                [DryWetMidiRecordTypes.Events.NoteAftertouch] = GetChannelEventParser<ChannelAftertouchEvent>(2),
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

        private static EventParser GetBytesBasedEventParser(Func<object[], MidiEvent> eventCreator, params Func<string, object>[] parametersParsers)
        {
            return (p, s) =>
            {
                if (p.Length < parametersParsers.Length)
                    ThrowBadFormat("Invalid number of parameters provided.");

                var parameters = new List<object>(parametersParsers.Length);
                var i = 0;

                for (i = 0; i < parametersParsers.Length; i++)
                {
                    var parameterParser = parametersParsers[i];

                    try
                    {
                        var parameter = parameterParser(p[i]);
                        parameters.Add(parameter);
                    }
                    catch
                    {
                        ThrowBadFormat($"Parameter ({i}) is invalid.");
                    }
                }

                if (p.Length < i)
                    ThrowBadFormat("Invalid number of parameters provided.");

                int bytesNumber = 0;

                try
                {
                    bytesNumber = int.Parse(p[i]);
                    parameters.Add(bytesNumber);
                }
                catch
                {
                    ThrowBadFormat($"Parameter ({i}) is invalid.");
                }

                i++;
                if (p.Length < i + bytesNumber)
                    ThrowBadFormat("Invalid number of parameters provided.");

                try
                {
                    var bytes = p.Skip(i)
                                 .Select(x =>
                                 {
                                     var b = byte.Parse(x);
                                     i++;
                                     return b;
                                 })
                                 .ToArray();
                    parameters.Add(bytes);
                }
                catch
                {
                    ThrowBadFormat($"Parameter ({i}) is invalid.");
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
                TypeParsers.String);
        }

        private static EventParser GetChannelEventParser<TEvent>(int parametersNumber)
            where TEvent : ChannelEvent
        {
            return GetEventParser(
                x =>
                {
                    var channelEvent = Activator.CreateInstance<TEvent>();
                    channelEvent.Channel = (FourBitNumber)x[0];

                    var parametersField = typeof(ChannelEvent).GetField("_parameters", BindingFlags.Instance | BindingFlags.NonPublic);
                    var parameters = (SevenBitNumber[])parametersField.GetValue(channelEvent);

                    for (int i = 0; i < parametersNumber; i++)
                    {
                        parameters[i] = (SevenBitNumber)x[i + 1];
                    }

                    return channelEvent;
                },
                new[] { TypeParsers.FourBitNumber }
                    .Concat(Enumerable.Range(0, parametersNumber).Select(i => TypeParsers.SevenBitNumber))
                    .ToArray());
        }

        private static EventParser GetEventParser(Func<object[], MidiEvent> eventCreator, params Func<string, object>[] parametersParsers)
        {
            return (p, s) =>
            {
                if (p.Length < parametersParsers.Length)
                    ThrowBadFormat("Invalid number of parameters provided.");

                var parameters = new List<object>(parametersParsers.Length);

                for (int i = 0; i < parametersParsers.Length; i++)
                {
                    var parameterParser = parametersParsers[i];

                    try
                    {
                        var parameter = parameterParser(p[i]);
                        parameters.Add(parameter);
                    }
                    catch
                    {
                        ThrowBadFormat($"Parameter ({i}) is invalid.");
                    }
                }

                return eventCreator(parameters.ToArray());
            };
        }

        private static void ThrowBadFormat(string message)
        {
            throw new FormatException(message);
        }

        #endregion
    }
}
