using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tools
{
    internal static class EventParser
    {
        #region Delegates

        public delegate MidiEvent Parser(string[] parameters, CsvSerializationSettings settings);

        #endregion

        #region Constants

        private static readonly Dictionary<MidiEventType, Parser> EventsParsers =
            new Dictionary<MidiEventType, Parser>()
            {
                [MidiEventType.SequenceTrackName] = GetTextEventParser<SequenceTrackNameEvent>(),
                [MidiEventType.CopyrightNotice] = GetTextEventParser<CopyrightNoticeEvent>(),
                [MidiEventType.InstrumentName] = GetTextEventParser<InstrumentNameEvent>(),
                [MidiEventType.Marker] = GetTextEventParser<MarkerEvent>(),
                [MidiEventType.CuePoint] = GetTextEventParser<CuePointEvent>(),
                [MidiEventType.Lyric] = GetTextEventParser<LyricEvent>(),
                [MidiEventType.Text] = GetTextEventParser<TextEvent>(),
                [MidiEventType.DeviceName] = GetTextEventParser<DeviceNameEvent>(),
                [MidiEventType.ProgramName] = GetTextEventParser<ProgramNameEvent>(),
                [MidiEventType.SequenceNumber] = GetEventParser(
                    x => new SequenceNumberEvent((ushort)x[0]),
                    TypeParser.UShort),
                [MidiEventType.PortPrefix] = GetEventParser(
                    x => new PortPrefixEvent((byte)x[0]),
                    TypeParser.Byte),
                [MidiEventType.ChannelPrefix] = GetEventParser(
                    x => new ChannelPrefixEvent((byte)x[0]),
                    TypeParser.Byte),
                [MidiEventType.TimeSignature] = GetEventParser(
                    x => new TimeSignatureEvent((byte)x[0], (byte)x[1], (byte)x[2], (byte)x[3]),
                    TypeParser.Byte,
                    TypeParser.Byte,
                    TypeParser.Byte,
                    TypeParser.Byte),
                [MidiEventType.KeySignature] = GetEventParser(
                    x => new KeySignatureEvent((sbyte)x[0], (byte)x[1]),
                    TypeParser.SByte,
                    TypeParser.Byte),
                [MidiEventType.SetTempo] = GetEventParser(
                    x => new SetTempoEvent((long)x[0]),
                    TypeParser.Long),
                [MidiEventType.SmpteOffset] = GetEventParser(
                    x => new SmpteOffsetEvent(
                        (SmpteFormat)Enum.Parse(typeof(SmpteFormat), x[0].ToString()),
                        (byte)x[1],
                        (byte)x[2],
                        (byte)x[3],
                        (byte)x[4],
                        (byte)x[5]),
                    TypeParser.String,
                    TypeParser.Byte,
                    TypeParser.Byte,
                    TypeParser.Byte,
                    TypeParser.Byte,
                    TypeParser.Byte),
                [MidiEventType.SequencerSpecific] = GetBytesBasedEventParser(
                    x => new SequencerSpecificEvent((byte[])x[0])),
                [MidiEventType.UnknownMeta] = GetBytesBasedEventParser(
                    x => new UnknownMetaEvent((byte)x[0], (byte[])x[1]),
                    TypeParser.Byte),
                [MidiEventType.NoteOn] = GetNoteEventParser<NoteOnEvent>(2),
                [MidiEventType.NoteOff] = GetNoteEventParser<NoteOffEvent>(2),
                [MidiEventType.PitchBend] = GetEventParser(
                    x => new PitchBendEvent((ushort)x[1]) { Channel = (FourBitNumber)x[0] },
                    TypeParser.FourBitNumber,
                    TypeParser.UShort),
                [MidiEventType.ControlChange] = GetChannelEventParser<ControlChangeEvent>(2),
                [MidiEventType.ProgramChange] = GetChannelEventParser<ProgramChangeEvent>(1),
                [MidiEventType.ChannelAftertouch] = GetChannelEventParser<ChannelAftertouchEvent>(1),
                [MidiEventType.NoteAftertouch] = GetNoteEventParser<NoteAftertouchEvent>(2),
                [MidiEventType.NormalSysEx] = GetBytesBasedEventParser(
                    x => new NormalSysExEvent((byte[])x[0])),
                [MidiEventType.EscapeSysEx] = GetBytesBasedEventParser(
                    x => new EscapeSysExEvent((byte[])x[0])),
                [MidiEventType.Start] = GetEventParser(
                    x => new StartEvent()),
                [MidiEventType.Stop] = GetEventParser(
                    x => new StopEvent()),
                [MidiEventType.Reset] = GetEventParser(
                    x => new ResetEvent()),
                [MidiEventType.Continue] = GetEventParser(
                    x => new ContinueEvent()),
                [MidiEventType.TuneRequest] = GetEventParser(
                    x => new TuneRequestEvent()),
                [MidiEventType.TimingClock] = GetEventParser(
                    x => new TimingClockEvent()),
                [MidiEventType.ActiveSensing] = GetEventParser(
                    x => new ActiveSensingEvent()),
                [MidiEventType.SongSelect] = GetEventParser(
                    x => new SongSelectEvent((SevenBitNumber)x[0]),
                    TypeParser.SevenBitNumber),
                [MidiEventType.SongPositionPointer] = GetEventParser(
                    x => new SongPositionPointerEvent((ushort)x[0]),
                    TypeParser.UShort),
                [MidiEventType.MidiTimeCode] = GetEventParser(
                    x => new MidiTimeCodeEvent(
                        (MidiTimeCodeComponent)Enum.Parse(typeof(MidiTimeCodeComponent), x[0].ToString()),
                        (FourBitNumber)x[1]),
                    TypeParser.String,
                    TypeParser.FourBitNumber),
            };

        #endregion

        #region Methods

        public static MidiEvent ParseEvent(MidiEventType eventType, string[] parameters, CsvSerializationSettings settings)
        {
            return EventsParsers[eventType](parameters, settings);
        }

        private static Parser GetBytesBasedEventParser(Func<object[], MidiEvent> eventCreator, params ParameterParser[] parametersParsers)
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

                try
                {
                    var bytes = TypeParser.BytesArray(p.Last(), s);
                    parameters.Add(bytes);
                }
                catch
                {
                    CsvError.ThrowBadFormat($"Parameter ({i}) is invalid.");
                }

                return eventCreator(parameters.ToArray());
            };
        }

        private static Parser GetTextEventParser<TEvent>()
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

        private static Parser GetNoteEventParser<TEvent>(int parametersNumber)
            where TEvent : ChannelEvent
        {
            return GetChannelEventParser<TEvent>(
                new[] { TypeParser.NoteNumber }
                .Concat(Enumerable.Range(0, parametersNumber - 1).Select(i => TypeParser.SevenBitNumber))
                .ToArray());
        }

        private static Parser GetChannelEventParser<TEvent>(int parametersNumber)
            where TEvent : ChannelEvent
        {
            return GetChannelEventParser<TEvent>(Enumerable.Range(0, parametersNumber)
                                                           .Select(i => TypeParser.SevenBitNumber)
                                                           .ToArray());
        }

        private static Parser GetChannelEventParser<TEvent>(ParameterParser[] parametersParsers)
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

        private static Parser GetEventParser(Func<object[], MidiEvent> eventCreator, params ParameterParser[] parametersParsers)
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
