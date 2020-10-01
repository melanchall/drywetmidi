using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class MidiFileSplitterTests
    {
        #region Test methods

        [Test]
        public void TransferContext_ChannelAftertouch()
        {
            CheckContextTransfer_ChannelEvent_SingleParameter(
                eventCreator: parameter => new ChannelAftertouchEvent(parameter),
                parameterConverter: value => (SevenBitNumber)value);
        }

        [Test]
        public void TransferContext_PitchBend()
        {
            CheckContextTransfer_ChannelEvent_SingleParameter(
                eventCreator: parameter => new PitchBendEvent(parameter),
                parameterConverter: value => (ushort)value);
        }

        [Test]
        public void TransferContext_ProgramChange()
        {
            CheckContextTransfer_ChannelEvent_SingleParameter(
                eventCreator: parameter => new ProgramChangeEvent(parameter),
                parameterConverter: value => (SevenBitNumber)value);
        }

        [Test]
        public void TransferContext_CopyrightNotice()
        {
            CheckContextTransfer_BaseTextEvent(
                eventCreator: text => new CopyrightNoticeEvent(text));
        }

        [Test]
        public void TransferContext_InstrumentName()
        {
            CheckContextTransfer_BaseTextEvent(
                eventCreator: text => new InstrumentNameEvent(text));
        }

        [Test]
        public void TransferContext_ProgramName()
        {
            CheckContextTransfer_BaseTextEvent(
                eventCreator: text => new ProgramNameEvent(text));
        }

        [Test]
        public void TransferContext_SequenceTrackName()
        {
            CheckContextTransfer_BaseTextEvent(
                eventCreator: text => new SequenceTrackNameEvent(text));
        }

        [Test]
        public void TransferContext_DeviceName()
        {
            CheckContextTransfer_BaseTextEvent(
                eventCreator: text => new DeviceNameEvent(text));
        }

        [Test]
        public void TransferContext_PortPrefix()
        {
            CheckContextTransfer_MetaEvent_SingleParameter(
                eventCreator: parameter => new PortPrefixEvent(parameter),
                parameterConverter: value => (byte)value);
        }

        [Test]
        public void TransferContext_SetTempo()
        {
            CheckContextTransfer_MetaEvent_SingleParameter(
                eventCreator: parameter => new SetTempoEvent(parameter),
                parameterConverter: value => (long)value);
        }

        [Test]
        public void TransferContext_ChannelPrefix()
        {
            CheckContextTransfer_MetaEvent_SingleParameter(
                eventCreator: parameter => new ChannelPrefixEvent(parameter),
                parameterConverter: value => (byte)value);
        }

        [Test]
        public void TransferContext_SequenceNumber()
        {
            CheckContextTransfer_MetaEvent_SingleParameter(
                eventCreator: parameter => new SequenceNumberEvent(parameter),
                parameterConverter: value => (ushort)value);
        }

        [Test]
        public void TransferContext_ControlChange()
        {
            CheckContextTransfer_ChannelEvent_TwoParameters(
                eventCreator: (parameter1, parameter2) => new ControlChangeEvent(parameter1, parameter2));
        }

        [Test]
        public void TransferContext_NoteAftertouch()
        {
            CheckContextTransfer_ChannelEvent_TwoParameters(
                eventCreator: (parameter1, parameter2) => new NoteAftertouchEvent(parameter1, parameter2));
        }

        [Test]
        public void TransferContext_KeySignature()
        {
            CheckContextTransfer(
                eventCreator: parameterValues => new KeySignatureEvent((sbyte)parameterValues[0], (byte)parameterValues[1]),
                value1: new object[] { (sbyte)-7, (byte)0 },
                value2: new object[] { (sbyte)-6, (byte)1 },
                value3: new object[] { (sbyte)-5, (byte)0 },
                value4: new object[] { (sbyte)-4, (byte)1 },
                value5: new object[] { (sbyte)-3, (byte)0 },
                value6: new object[] { (sbyte)-2, (byte)1 },
                value7: new object[] { (sbyte)-1, (byte)0 },
                value8: new object[] { (sbyte)0, (byte)1 },
                value9: new object[] { (sbyte)1, (byte)0 },
                value10: new object[] { (sbyte)2, (byte)1 },
                value11: new object[] { (sbyte)3, (byte)0 });
        }

        [Test]
        public void TransferContext_SmpteOffset()
        {
            CheckContextTransfer(
                eventCreator: parameterValues => new SmpteOffsetEvent(
                    (SmpteFormat)parameterValues[0],
                    (byte)parameterValues[1],
                    (byte)parameterValues[2],
                    (byte)parameterValues[3],
                    (byte)parameterValues[4],
                    (byte)parameterValues[5]),
                value1: new object[] { SmpteFormat.Thirty, (byte)1, (byte)2, (byte)3, (byte)4, (byte)5 },
                value2: new object[] { SmpteFormat.Thirty, (byte)2, (byte)2, (byte)3, (byte)4, (byte)5 },
                value3: new object[] { SmpteFormat.Thirty, (byte)3, (byte)4, (byte)3, (byte)4, (byte)5 },
                value4: new object[] { SmpteFormat.Thirty, (byte)4, (byte)4, (byte)3, (byte)4, (byte)5 },
                value5: new object[] { SmpteFormat.ThirtyDrop, (byte)5, (byte)6, (byte)3, (byte)4, (byte)5 },
                value6: new object[] { SmpteFormat.ThirtyDrop, (byte)6, (byte)6, (byte)3, (byte)4, (byte)5 },
                value7: new object[] { SmpteFormat.TwentyFive, (byte)7, (byte)7, (byte)3, (byte)4, (byte)5 },
                value8: new object[] { SmpteFormat.TwentyFive, (byte)8, (byte)7, (byte)3, (byte)4, (byte)5 },
                value9: new object[] { SmpteFormat.TwentyFive, (byte)9, (byte)7, (byte)3, (byte)4, (byte)5 },
                value10: new object[] { SmpteFormat.TwentyFour, (byte)10, (byte)1, (byte)3, (byte)4, (byte)5 },
                value11: new object[] { SmpteFormat.TwentyFour, (byte)11, (byte)1, (byte)3, (byte)4, (byte)5 });
        }

        [Test]
        public void TransferContext_TimeSignature()
        {
            CheckContextTransfer(
                eventCreator: parameterValues => new TimeSignatureEvent(
                    (byte)parameterValues[0],
                    (byte)parameterValues[1],
                    (byte)parameterValues[2],
                    (byte)parameterValues[3]),
                value1: new object[] { (byte)1, (byte)2, (byte)3, (byte)4 },
                value2: new object[] { (byte)2, (byte)2, (byte)4, (byte)4 },
                value3: new object[] { (byte)3, (byte)2, (byte)5, (byte)4 },
                value4: new object[] { (byte)4, (byte)2, (byte)6, (byte)4 },
                value5: new object[] { (byte)5, (byte)2, (byte)7, (byte)4 },
                value6: new object[] { (byte)6, (byte)2, (byte)8, (byte)4 },
                value7: new object[] { (byte)7, (byte)2, (byte)9, (byte)4 },
                value8: new object[] { (byte)8, (byte)2, (byte)10, (byte)4 },
                value9: new object[] { (byte)9, (byte)2, (byte)11, (byte)4 },
                value10: new object[] { (byte)10, (byte)2, (byte)12, (byte)4 },
                value11: new object[] { (byte)11, (byte)2, (byte)13, (byte)4 });
        }

        #endregion

        #region Private methods

        public void CheckContextTransfer_ChannelEvent_TwoParameters<TEvent>(Func<SevenBitNumber, SevenBitNumber, TEvent> eventCreator)
            where TEvent : ChannelEvent
        {
            TimedEvent CreateTimedEvent(int firstParameterValue, int secondParameterValue, int channel, long time)
            {
                var result = eventCreator((SevenBitNumber)firstParameterValue, (SevenBitNumber)secondParameterValue);
                result.Channel = (FourBitNumber)channel;
                return new TimedEvent(result, time);
            }

            CheckContextTransfer(
                inputTimedEvents: new[]
                {
                    new[]
                    {
                        CreateTimedEvent(10, 20, 0, 0),
                        CreateTimedEvent(20, 30, 1, 10),
                        CreateTimedEvent(10, 40, 1, 20),
                        new TimedEvent(new TextEvent("X"), 105),
                        CreateTimedEvent(10, 50, 0, 110),
                        CreateTimedEvent(70, 80, 0, 120),
                        CreateTimedEvent(20, 10, 1, 150),
                        new TimedEvent(new TextEvent("Y"), 200),
                    }
                },
                outputTimedEvents: new[]
                {
                    new[] // 0-100
                    {
                        CreateTimedEvent(10, 20, 0, 0),
                        CreateTimedEvent(20, 30, 1, 10),
                        CreateTimedEvent(10, 40, 1, 20),
                    },
                    new[] // 100-200
                    {
                        CreateTimedEvent(10, 20, 0, 100),
                        CreateTimedEvent(20, 30, 1, 100),
                        CreateTimedEvent(10, 40, 1, 100),
                        new TimedEvent(new TextEvent("X"), 105),
                        CreateTimedEvent(10, 50, 0, 110),
                        CreateTimedEvent(70, 80, 0, 120),
                        CreateTimedEvent(20, 10, 1, 150),
                    },
                    new[] // 200-300
                    {
                        CreateTimedEvent(10, 50, 0, 200),
                        CreateTimedEvent(20, 10, 1, 200),
                        CreateTimedEvent(10, 40, 1, 200),
                        CreateTimedEvent(70, 80, 0, 200),
                        new TimedEvent(new TextEvent("Y"), 200),
                    }
                });
        }

        public void CheckContextTransfer_MetaEvent_SingleParameter<TParameter, TEvent>(Func<TParameter, TEvent> eventCreator, Func<int, TParameter> parameterConverter)
            where TEvent : MetaEvent
        {
            CheckContextTransfer(
                eventCreator: parameterValues => eventCreator(parameterConverter((int)parameterValues[0])),
                value1: new object[] { 10 },
                value2: new object[] { 20 },
                value3: new object[] { 30 },
                value4: new object[] { 40 },
                value5: new object[] { 50 },
                value6: new object[] { 60 },
                value7: new object[] { 70 },
                value8: new object[] { 80 },
                value9: new object[] { 90 },
                value10: new object[] { 100 },
                value11: new object[] { 110 });
        }

        public void CheckContextTransfer_BaseTextEvent<TEvent>(Func<string, TEvent> eventCreator)
            where TEvent : BaseTextEvent
        {
            CheckContextTransfer(
                eventCreator: parameterValues => eventCreator((string)parameterValues[0]),
                value1: new object[] { "A" },
                value2: new object[] { "B" },
                value3: new object[] { "C" },
                value4: new object[] { "D" },
                value5: new object[] { "E" },
                value6: new object[] { "F" },
                value7: new object[] { "G" },
                value8: new object[] { "H" },
                value9: new object[] { "I" },
                value10: new object[] { "J" },
                value11: new object[] { "K" });
        }

        public void CheckContextTransfer<TEvent>(
            Func<object[], TEvent> eventCreator,
            object[] value1,
            object[] value2,
            object[] value3,
            object[] value4,
            object[] value5,
            object[] value6,
            object[] value7,
            object[] value8,
            object[] value9,
            object[] value10,
            object[] value11)
            where TEvent : MidiEvent
        {
            TimedEvent CreateTimedEvent(object[] parameterValues, long time)
            {
                var result = eventCreator(parameterValues);
                return new TimedEvent(result, time);
            }

            CheckContextTransfer(
                inputTimedEvents: new[]
                {
                    new[]
                    {
                        CreateTimedEvent(value1, 0),
                        CreateTimedEvent(value2, 20),
                        CreateTimedEvent(value3, 150),
                        CreateTimedEvent(value4, 250),
                    },
                    new[]
                    {
                        CreateTimedEvent(value5, 170),
                        CreateTimedEvent(value6, 180),
                        CreateTimedEvent(value7, 300),
                        CreateTimedEvent(value8, 310),
                        new TimedEvent(new TextEvent("X"), 400),
                        CreateTimedEvent(value9, 410),
                    },
                    new[]
                    {
                        CreateTimedEvent(value10, 410),
                        CreateTimedEvent(value11, 420),
                        new TimedEvent(new TextEvent("Y"), 500),
                    }
                },
                outputTimedEvents: new[]
                {
                    new[] // 0-100
                    {
                        CreateTimedEvent(value1, 0),
                        CreateTimedEvent(value2, 20),
                    },
                    new[] // 100-200
                    {
                        CreateTimedEvent(value2, 100),
                        CreateTimedEvent(value3, 150),
                        CreateTimedEvent(value5, 170),
                        CreateTimedEvent(value6, 180),
                    },
                    new[] // 200-300
                    {
                        CreateTimedEvent(value3, 200),
                        CreateTimedEvent(value6, 200),
                        CreateTimedEvent(value4, 250),
                    },
                    new[] // 300-400
                    {
                        CreateTimedEvent(value4, 300),
                        CreateTimedEvent(value6, 300),
                        CreateTimedEvent(value7, 300),
                        CreateTimedEvent(value8, 310),
                    },
                    new[] // 400-500
                    {
                        CreateTimedEvent(value4, 400),
                        CreateTimedEvent(value8, 400),
                        new TimedEvent(new TextEvent("X"), 400),
                        CreateTimedEvent(value9, 410),
                        CreateTimedEvent(value10, 410),
                        CreateTimedEvent(value11, 420),
                    },
                    new[] // 500-600
                    {
                        CreateTimedEvent(value4, 500),
                        CreateTimedEvent(value9, 500),
                        CreateTimedEvent(value11, 500),
                        new TimedEvent(new TextEvent("Y"), 500),
                    }
                });
        }

        public void CheckContextTransfer_ChannelEvent_SingleParameter<TEvent, TParameter>(Func<TParameter, TEvent> eventCreator, Func<int, TParameter> parameterConverter)
            where TEvent : ChannelEvent
        {
            TimedEvent CreateTimedEvent(int parameterValue, int channel, long time)
            {
                var result = eventCreator(parameterConverter(parameterValue));
                result.Channel = (FourBitNumber)channel;
                return new TimedEvent(result, time);
            }

            CheckContextTransfer(
                inputTimedEvents: new[]
                {
                    new[]
                    {
                        CreateTimedEvent(50,  0, 0),
                        CreateTimedEvent(75,  3, 20),
                        CreateTimedEvent(25,  0, 150),
                        CreateTimedEvent(105, 3, 250),
                    },
                    new[]
                    {
                        CreateTimedEvent(115, 0, 170),
                        CreateTimedEvent(125, 0, 180),
                        CreateTimedEvent(15,  3, 300),
                        CreateTimedEvent(5,   3, 310),
                        new TimedEvent(new TextEvent("A"), 400),
                        CreateTimedEvent(20,  5, 410),
                    },
                    new[]
                    {
                        CreateTimedEvent(30,  5, 410),
                        CreateTimedEvent(40,  5, 420),
                        new TimedEvent(new TextEvent("B"), 500),
                    }
                },
                outputTimedEvents: new[]
                {
                    new[] // 0-100
                    {
                        CreateTimedEvent(50,  0, 0),
                        CreateTimedEvent(75,  3, 20),
                    },
                    new[] // 100-200
                    {
                        CreateTimedEvent(50,  0, 100),
                        CreateTimedEvent(75,  3, 100),
                        CreateTimedEvent(25,  0, 150),
                        CreateTimedEvent(115, 0, 170),
                        CreateTimedEvent(125, 0, 180),
                    },
                    new[] // 200-300
                    {
                        CreateTimedEvent(25,  0, 200),
                        CreateTimedEvent(75,  3, 200),
                        CreateTimedEvent(125, 0, 200),
                        CreateTimedEvent(105, 3, 250),
                    },
                    new[] // 300-400
                    {
                        CreateTimedEvent(25,  0, 300),
                        CreateTimedEvent(105, 3, 300),
                        CreateTimedEvent(125, 0, 300),
                        CreateTimedEvent(15,  3, 300),
                        CreateTimedEvent(5,   3, 310),
                    },
                    new[] // 400-500
                    {
                        CreateTimedEvent(25,  0, 400),
                        CreateTimedEvent(105, 3, 400),
                        CreateTimedEvent(125, 0, 400),
                        CreateTimedEvent(5,   3, 400),
                        new TimedEvent(new TextEvent("A"), 400),
                        CreateTimedEvent(20,  5, 410),
                        CreateTimedEvent(30,  5, 410),
                        CreateTimedEvent(40,  5, 420),
                    },
                    new[] // 500-600
                    {
                        CreateTimedEvent(25,  0, 500),
                        CreateTimedEvent(105, 3, 500),
                        CreateTimedEvent(125, 0, 500),
                        CreateTimedEvent(5,   3, 500),
                        CreateTimedEvent(20,  5, 500),
                        CreateTimedEvent(40,  5, 500),
                        new TimedEvent(new TextEvent("B"), 500),
                    }
                });
        }

        private static void CheckContextTransfer(
            TimedEvent[][] inputTimedEvents,
            TimedEvent[][] outputTimedEvents)
        {
            CompareTimedEventsSplittingByGrid(
                inputTimedEvents: inputTimedEvents,
                grid: new SteppedGrid((MidiTimeSpan)100),
                settings: new SliceMidiFileSettings
                {
                    PreserveTrackChunks = true,
                    PreserveTimes = true
                },
                outputTimedEvents: outputTimedEvents);
        }

        #endregion
    }
}
