using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Tools
{
    internal static class EventParser
    {
        #region Nested delegates

        public delegate MidiEvent Parser(string[] parameters, CsvDeserializationSettings settings);

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
                    (TypeParser.DataType.UShort, "number")),
                [MidiEventType.PortPrefix] = GetEventParser(
                    x => new PortPrefixEvent((byte)x[0]),
                    (TypeParser.DataType.Byte, "port")),
                [MidiEventType.ChannelPrefix] = GetEventParser(
                    x => new ChannelPrefixEvent((byte)x[0]),
                    (TypeParser.DataType.Byte, "channel")),
                [MidiEventType.TimeSignature] = GetEventParser(
                    x => new TimeSignatureEvent((byte)x[0], (byte)x[1], (byte)x[2], (byte)x[3]),
                    (TypeParser.DataType.Byte, "numerator"),
                    (TypeParser.DataType.Byte, "denominator"),
                    (TypeParser.DataType.Byte, "clocks per click"),
                    (TypeParser.DataType.Byte, "thirty-second notes per beat")),
                [MidiEventType.KeySignature] = GetEventParser(
                    x => new KeySignatureEvent((sbyte)x[0], (byte)x[1]),
                    (TypeParser.DataType.SByte, "key"),
                    (TypeParser.DataType.Byte, "scale")),
                [MidiEventType.SetTempo] = GetEventParser(
                    x => new SetTempoEvent((long)x[0]),
                    (TypeParser.DataType.Long, "microseconds per quarter note")),
                [MidiEventType.SmpteOffset] = GetEventParser(
                    x => new SmpteOffsetEvent(
                        (SmpteFormat)Enum.Parse(typeof(SmpteFormat), x[0].ToString() ?? string.Empty),
                        (byte)x[1],
                        (byte)x[2],
                        (byte)x[3],
                        (byte)x[4],
                        (byte)x[5]),
                    (TypeParser.DataType.String, "format"),
                    (TypeParser.DataType.Byte, "hours"),
                    (TypeParser.DataType.Byte, "minutes"),
                    (TypeParser.DataType.Byte, "seconds"),
                    (TypeParser.DataType.Byte, "frames"),
                    (TypeParser.DataType.Byte, "sub-frames")),
                [MidiEventType.SequencerSpecific] = GetEventParser(
                    x => new SequencerSpecificEvent((byte[])x[0]),
                    (TypeParser.DataType.BytesArray, "data")),
                [MidiEventType.UnknownMeta] = GetEventParser(
                    x => new UnknownMetaEvent((byte)x[0], (byte[])x[1]),
                    (TypeParser.DataType.Byte, "status byte"),
                    (TypeParser.DataType.BytesArray, "data")),
                [MidiEventType.NoteOn] = GetEventParser(
                    x => new NoteOnEvent((SevenBitNumber)x[1], (SevenBitNumber)x[2]) { Channel = (FourBitNumber)x[0] },
                    (TypeParser.DataType.FourBitNumber, "channel"),
                    (TypeParser.DataType.NoteNumber, "note number"),
                    (TypeParser.DataType.SevenBitNumber, "velocity")),
                [MidiEventType.NoteOff] = GetEventParser(
                    x => new NoteOffEvent((SevenBitNumber)x[1], (SevenBitNumber)x[2]) { Channel = (FourBitNumber)x[0] },
                    (TypeParser.DataType.FourBitNumber, "channel"),
                    (TypeParser.DataType.NoteNumber, "note number"),
                    (TypeParser.DataType.SevenBitNumber, "velocity")),
                [MidiEventType.PitchBend] = GetEventParser(
                    x => new PitchBendEvent((ushort)x[1]) { Channel = (FourBitNumber)x[0] },
                    (TypeParser.DataType.FourBitNumber, "channel"),
                    (TypeParser.DataType.UShort, "pitch value")),
                [MidiEventType.ControlChange] = GetEventParser(
                    x => new ControlChangeEvent((SevenBitNumber)x[1], (SevenBitNumber)x[2]) { Channel = (FourBitNumber)x[0] },
                    (TypeParser.DataType.FourBitNumber, "channel"),
                    (TypeParser.DataType.SevenBitNumber, "control number"),
                    (TypeParser.DataType.SevenBitNumber, "control value")),
                [MidiEventType.ProgramChange] = GetEventParser(
                    x => new ProgramChangeEvent((SevenBitNumber)x[1]) { Channel = (FourBitNumber)x[0] },
                    (TypeParser.DataType.FourBitNumber, "channel"),
                    (TypeParser.DataType.SevenBitNumber, "program number")),
                [MidiEventType.ChannelAftertouch] = GetEventParser(
                    x => new ChannelAftertouchEvent((SevenBitNumber)x[1]) { Channel = (FourBitNumber)x[0] },
                    (TypeParser.DataType.FourBitNumber, "channel"),
                    (TypeParser.DataType.SevenBitNumber, "aftertouch value")),
                [MidiEventType.NoteAftertouch] = GetEventParser(
                    x => new NoteAftertouchEvent((SevenBitNumber)x[1], (SevenBitNumber)x[2]) { Channel = (FourBitNumber)x[0] },
                    (TypeParser.DataType.FourBitNumber, "channel"),
                    (TypeParser.DataType.NoteNumber, "note number"),
                    (TypeParser.DataType.SevenBitNumber, "aftertouch value")),
                [MidiEventType.NormalSysEx] = GetEventParser(
                    x => new NormalSysExEvent((byte[])x[0]),
                    (TypeParser.DataType.BytesArray, "data")),
                [MidiEventType.EscapeSysEx] = GetEventParser(
                    x => new EscapeSysExEvent((byte[])x[0]),
                    (TypeParser.DataType.BytesArray, "data")),
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
                    (TypeParser.DataType.SevenBitNumber, "number")),
                [MidiEventType.SongPositionPointer] = GetEventParser(
                    x => new SongPositionPointerEvent((ushort)x[0]),
                    (TypeParser.DataType.UShort, "pointer value")),
                [MidiEventType.MidiTimeCode] = GetEventParser(
                    x => new MidiTimeCodeEvent(
                        (MidiTimeCodeComponent)Enum.Parse(typeof(MidiTimeCodeComponent), x[0].ToString() ?? string.Empty),
                        (FourBitNumber)x[1]),
                    (TypeParser.DataType.String, "component"),
                    (TypeParser.DataType.FourBitNumber, "component value")),
            };

        #endregion

        #region Methods

        public static MidiEvent ParseEvent(
            MidiEventType eventType,
            string[] parameters,
            CsvDeserializationSettings settings)
        {
            return EventsParsers[eventType](parameters, settings);
        }

        private static Parser GetTextEventParser<TEvent>()
            where TEvent : BaseTextEvent, new()
        {
            return GetEventParser(
                x => new TEvent { Text = (string)x[0] },
                (TypeParser.DataType.String, "text"));
        }

        private static Parser GetEventParser(
            Func<object[], MidiEvent> eventCreator,
            params (TypeParser.DataType Type, string ValueDescription)[] parametersParsers)
        {
            return (p, s) =>
            {
                if (p.Length < parametersParsers.Length)
                    throw CsvError.BadFormat($"Invalid number of parameters provided ({p.Length} with {parametersParsers.Length} expected).");

                var parameters = new List<object>(parametersParsers.Length);

                for (int i = 0; i < parametersParsers.Length; i++)
                {
                    var parameterParser = parametersParsers[i];

                    var parameter = TypeParser.Parse(p[i], parameterParser.Type, parameterParser.ValueDescription, null, s);
                    parameters.Add(parameter);
                }

                return eventCreator(parameters.ToArray());
            };
        }

        #endregion
    }
}
