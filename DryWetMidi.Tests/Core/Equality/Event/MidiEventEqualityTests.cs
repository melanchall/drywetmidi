using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed class MidiEventEqualityTests
    {
        #region Nested classes

        private sealed class CustomMetaEvent : MetaEvent
        {
            private readonly string _data;

            public CustomMetaEvent(string data) =>
                _data = data;

            protected override MidiEvent CloneEvent() =>
                new CustomMetaEvent(_data);

            protected override int GetContentSize(WritingSettings settings) =>
                throw new NotImplementedException();

            protected override void ReadContent(MidiReader reader, ReadingSettings settings, int size) =>
                throw new NotImplementedException();

            protected override void WriteContent(MidiWriter writer, WritingSettings settings) =>
                throw new NotImplementedException();

            public override string ToString() =>
                _data;

            public override bool Equals(object obj)
            {
                if (!(obj is CustomMetaEvent other))
                    return false;

                return _data == other._data;
            }

            public override int GetHashCode() =>
                _data.GetHashCode();
        }

        #endregion

        #region Constants

        private static readonly object[] EqualEvents =
        {
            new object[] { new NormalSysExEvent(), new NormalSysExEvent() },
            new object[] { new NormalSysExEvent(), new NormalSysExEvent(new byte[] { 0xF0 }) },
            new object[] { new NormalSysExEvent(new byte[] { 0xF0 }), new NormalSysExEvent() },
            new object[] { new NormalSysExEvent(new byte[] { 0xF0, 0xAB, 0x65, 0xF7 }), new NormalSysExEvent(new byte[] { 0xF0, 0xAB, 0x65, 0xF7 }) },
            new object[] { new NormalSysExEvent(new byte[] { 0xAB, 0x65, 0xF7 }), new NormalSysExEvent(new byte[] { 0xF0, 0xAB, 0x65, 0xF7 }) },
            new object[] { new NormalSysExEvent(new byte[] { 0xF0, 0xAB, 0x65, 0xF7 }), new NormalSysExEvent(new byte[] { 0xAB, 0x65, 0xF7 }) },

            new object[] { new EscapeSysExEvent(), new EscapeSysExEvent() },
            // TODO: differs by Completed property
            // new object[] { new EscapeSysExEvent(), new EscapeSysExEvent(new byte[] { 0xF7 }) },
            // new object[] { new EscapeSysExEvent(new byte[] { 0xF7 }), new EscapeSysExEvent() },
            new object[] { new EscapeSysExEvent(new byte[] { 0xF7, 0xAB, 0x65, 0xF7 }), new EscapeSysExEvent(new byte[] { 0xF7, 0xAB, 0x65, 0xF7 }) },
            new object[] { new EscapeSysExEvent(new byte[] { 0xAB, 0x65, 0xF7 }), new EscapeSysExEvent(new byte[] { 0xF7, 0xAB, 0x65, 0xF7 }) },
            new object[] { new EscapeSysExEvent(new byte[] { 0xF7, 0xAB, 0x65, 0xF7 }), new EscapeSysExEvent(new byte[] { 0xAB, 0x65, 0xF7 }) },

            new object[] { new ChannelAftertouchEvent(), new ChannelAftertouchEvent() },
            new object[] { new ChannelAftertouchEvent((SevenBitNumber)4), new ChannelAftertouchEvent((SevenBitNumber)4) },
            new object[] { new ChannelAftertouchEvent((SevenBitNumber)4) { Channel = (FourBitNumber)5 }, new ChannelAftertouchEvent((SevenBitNumber)4) { Channel = (FourBitNumber)5 } },

            new object[] { new ControlChangeEvent(), new ControlChangeEvent() },
            new object[] { new ControlChangeEvent((SevenBitNumber)4, (SevenBitNumber)70), new ControlChangeEvent((SevenBitNumber)4, (SevenBitNumber)70) },
            new object[] { new ControlChangeEvent((SevenBitNumber)4, (SevenBitNumber)70) { Channel = (FourBitNumber)5 }, new ControlChangeEvent((SevenBitNumber)4, (SevenBitNumber)70) { Channel = (FourBitNumber)5 } },

            new object[] { new NoteAftertouchEvent(), new NoteAftertouchEvent() },
            new object[] { new NoteAftertouchEvent((SevenBitNumber)4, (SevenBitNumber)70), new NoteAftertouchEvent((SevenBitNumber)4, (SevenBitNumber)70) },
            new object[] { new NoteAftertouchEvent((SevenBitNumber)4, (SevenBitNumber)70) { Channel = (FourBitNumber)5 }, new NoteAftertouchEvent((SevenBitNumber)4, (SevenBitNumber)70) { Channel = (FourBitNumber)5 } },

            new object[] { new NoteOffEvent(), new NoteOffEvent() },
            new object[] { new NoteOffEvent((SevenBitNumber)4, (SevenBitNumber)70), new NoteOffEvent((SevenBitNumber)4, (SevenBitNumber)70) },
            new object[] { new NoteOffEvent((SevenBitNumber)4, (SevenBitNumber)70) { Channel = (FourBitNumber)5 }, new NoteOffEvent((SevenBitNumber)4, (SevenBitNumber)70) { Channel = (FourBitNumber)5 } },

            new object[] { new NoteOnEvent(), new NoteOnEvent() },
            new object[] { new NoteOnEvent((SevenBitNumber)4, (SevenBitNumber)70), new NoteOnEvent((SevenBitNumber)4, (SevenBitNumber)70) },
            new object[] { new NoteOnEvent((SevenBitNumber)4, (SevenBitNumber)70) { Channel = (FourBitNumber)5 }, new NoteOnEvent((SevenBitNumber)4, (SevenBitNumber)70) { Channel = (FourBitNumber)5 } },

            new object[] { new PitchBendEvent(), new PitchBendEvent() },
            new object[] { new PitchBendEvent(1234), new PitchBendEvent(1234) },
            new object[] { new PitchBendEvent(1234) { Channel = (FourBitNumber)5 }, new PitchBendEvent(1234) { Channel = (FourBitNumber)5 } },

            new object[] { new ProgramChangeEvent(), new ProgramChangeEvent() },
            new object[] { new ProgramChangeEvent((SevenBitNumber)4), new ProgramChangeEvent((SevenBitNumber)4) },
            new object[] { new ProgramChangeEvent((SevenBitNumber)4) { Channel = (FourBitNumber)5 }, new ProgramChangeEvent((SevenBitNumber)4) { Channel = (FourBitNumber)5 } },

            new object[] { new ChannelPrefixEvent(), new ChannelPrefixEvent() },
            new object[] { new ChannelPrefixEvent(23), new ChannelPrefixEvent(23) },

            new object[] { new CopyrightNoticeEvent(), new CopyrightNoticeEvent() },
            new object[] { new CopyrightNoticeEvent("ABC"), new CopyrightNoticeEvent("ABC") },

            new object[] { new CuePointEvent(), new CuePointEvent() },
            new object[] { new CuePointEvent("ABC"), new CuePointEvent("ABC") },

            new object[] { new DeviceNameEvent(), new DeviceNameEvent() },
            new object[] { new DeviceNameEvent("ABC"), new DeviceNameEvent("ABC") },

            new object[] { new EndOfTrackEvent(), new EndOfTrackEvent() },

            new object[] { new InstrumentNameEvent(), new InstrumentNameEvent() },
            new object[] { new InstrumentNameEvent("ABC"), new InstrumentNameEvent("ABC") },

            new object[] { new KeySignatureEvent(), new KeySignatureEvent() },
            new object[] { new KeySignatureEvent(3, 1), new KeySignatureEvent(3, 1) },
            new object[] { new KeySignatureEvent(-3, 1), new KeySignatureEvent(-3, 1) },

            new object[] { new LyricEvent(), new LyricEvent() },
            new object[] { new LyricEvent("ABC"), new LyricEvent("ABC") },

            new object[] { new MarkerEvent(), new MarkerEvent() },
            new object[] { new MarkerEvent("ABC"), new MarkerEvent("ABC") },

            new object[] { new PortPrefixEvent(), new PortPrefixEvent() },
            new object[] { new PortPrefixEvent(23), new PortPrefixEvent(23) },

            new object[] { new ProgramNameEvent(), new ProgramNameEvent() },
            new object[] { new ProgramNameEvent("ABC"), new ProgramNameEvent("ABC") },

            new object[] { new SequenceNumberEvent(), new SequenceNumberEvent() },
            new object[] { new SequenceNumberEvent(45), new SequenceNumberEvent(45) },

            new object[] { new SequencerSpecificEvent(), new SequencerSpecificEvent() },
            new object[] { new SequencerSpecificEvent(new byte[] { 1, 2, 3 }), new SequencerSpecificEvent(new byte[] { 1, 2, 3 }) },

            new object[] { new SequenceTrackNameEvent(), new SequenceTrackNameEvent() },
            new object[] { new SequenceTrackNameEvent("ABC"), new SequenceTrackNameEvent("ABC") },

            new object[] { new SetTempoEvent(), new SetTempoEvent() },
            new object[] { new SetTempoEvent(123456), new SetTempoEvent(123456) },

            new object[] { new SmpteOffsetEvent(), new SmpteOffsetEvent() },
            new object[] { new SmpteOffsetEvent(SmpteFormat.TwentyFive, 1, 2, 3, 4, 5), new SmpteOffsetEvent(SmpteFormat.TwentyFive, 1, 2, 3, 4, 5) },

            new object[] { new TextEvent(), new TextEvent() },
            new object[] { new TextEvent("ABC"), new TextEvent("ABC") },

            new object[] { new TimeSignatureEvent(), new TimeSignatureEvent() },
            new object[] { new TimeSignatureEvent(2, 8, 32, 64), new TimeSignatureEvent(2, 8, 32, 64) },

            new object[] { new UnknownMetaEvent(45), new UnknownMetaEvent(45) },
            new object[] { new UnknownMetaEvent(45, new byte[] { 1, 2, 3 }), new UnknownMetaEvent(45, new byte[] { 1, 2, 3 }) },

            new object[] { new CustomMetaEvent("ABC"), new CustomMetaEvent("ABC") },

            new object[] { new MidiTimeCodeEvent(), new MidiTimeCodeEvent() },
            new object[] { new MidiTimeCodeEvent(MidiTimeCodeComponent.HoursLsb, (FourBitNumber)12), new MidiTimeCodeEvent(MidiTimeCodeComponent.HoursLsb, (FourBitNumber)12) },

            new object[] { new SongPositionPointerEvent(), new SongPositionPointerEvent() },
            new object[] { new SongPositionPointerEvent(45), new SongPositionPointerEvent(45) },

            new object[] { new SongSelectEvent(), new SongSelectEvent() },
            new object[] { new SongSelectEvent((SevenBitNumber)45), new SongSelectEvent((SevenBitNumber)45) },

            new object[] { new TuneRequestEvent(), new TuneRequestEvent() },

            new object[] { new ActiveSensingEvent(), new ActiveSensingEvent() },

            new object[] { new ContinueEvent(), new ContinueEvent() },

            new object[] { new ResetEvent(), new ResetEvent() },

            new object[] { new StartEvent(), new StartEvent() },

            new object[] { new StopEvent(), new StopEvent() },

            new object[] { new TimingClockEvent(), new TimingClockEvent() },
        };

        private static readonly object[] NonEqualEvents =
        {
            new object[] { new NormalSysExEvent(), new NormalSysExEvent(new byte[] { 0xFA }) },
            new object[] { new NormalSysExEvent(new byte[] { 0xF0, 0xAC, 0x65, 0xF7 }), new NormalSysExEvent(new byte[] { 0xF0, 0xAB, 0x65, 0xF7 }) },
            new object[] { new NormalSysExEvent(new byte[] { 0xAB, 0x65, 0xF7 }), new NormalSysExEvent(new byte[] { 0xF0, 0xAB, 0x65, 0xF8 }) },
            new object[] { new NormalSysExEvent(new byte[] { 0xF0, 0xAB, 0x64, 0xF7 }), new NormalSysExEvent(new byte[] { 0xAB, 0x65, 0xF7 }) },

            new object[] { new EscapeSysExEvent(new byte[] { 0xF5 }), new EscapeSysExEvent() },
            new object[] { new EscapeSysExEvent(new byte[] { 0xF7, 0xAC, 0x65, 0xF7 }), new EscapeSysExEvent(new byte[] { 0xF7, 0xAB, 0x65, 0xF7 }) },
            new object[] { new EscapeSysExEvent(new byte[] { 0xAB, 0x65, 0xF7 }), new EscapeSysExEvent(new byte[] { 0xF7, 0xAB, 0x65, 0xF8 }) },
            new object[] { new EscapeSysExEvent(new byte[] { 0xF7, 0xAB, 0x64, 0xF7 }), new EscapeSysExEvent(new byte[] { 0xAB, 0x65, 0xF7 }) },

            new object[] { new ChannelAftertouchEvent(), new ChannelAftertouchEvent((SevenBitNumber)4) },
            new object[] { new ChannelAftertouchEvent() { Channel = (FourBitNumber)10 }, new ChannelAftertouchEvent() },
            new object[] { new ChannelAftertouchEvent((SevenBitNumber)1), new ChannelAftertouchEvent((SevenBitNumber)4) },
            new object[] { new ChannelAftertouchEvent((SevenBitNumber)4) { Channel = (FourBitNumber)3 }, new ChannelAftertouchEvent((SevenBitNumber)4) { Channel = (FourBitNumber)5 } },

            new object[] { new ControlChangeEvent(), new ControlChangeEvent((SevenBitNumber)4, (SevenBitNumber)70) },
            new object[] { new ControlChangeEvent() { Channel = (FourBitNumber)10 }, new ControlChangeEvent() },
            new object[] { new ControlChangeEvent((SevenBitNumber)4, (SevenBitNumber)40), new ControlChangeEvent((SevenBitNumber)4, (SevenBitNumber)70) },
            new object[] { new ControlChangeEvent((SevenBitNumber)4, (SevenBitNumber)70), new ControlChangeEvent((SevenBitNumber)2, (SevenBitNumber)70) },
            new object[] { new ControlChangeEvent((SevenBitNumber)4, (SevenBitNumber)70) { Channel = (FourBitNumber)3 }, new ControlChangeEvent((SevenBitNumber)4, (SevenBitNumber)70) { Channel = (FourBitNumber)5 } },

            new object[] { new NoteAftertouchEvent((SevenBitNumber)4, (SevenBitNumber)70), new NoteAftertouchEvent() },
            new object[] { new NoteAftertouchEvent(), new NoteAftertouchEvent() { Channel = (FourBitNumber)10 } },
            new object[] { new NoteAftertouchEvent((SevenBitNumber)2, (SevenBitNumber)70), new NoteAftertouchEvent((SevenBitNumber)4, (SevenBitNumber)70) },
            new object[] { new NoteAftertouchEvent((SevenBitNumber)4, (SevenBitNumber)50), new NoteAftertouchEvent((SevenBitNumber)4, (SevenBitNumber)70) },
            new object[] { new NoteAftertouchEvent((SevenBitNumber)4, (SevenBitNumber)70) { Channel = (FourBitNumber)5 }, new NoteAftertouchEvent((SevenBitNumber)4, (SevenBitNumber)70) { Channel = (FourBitNumber)3 } },

            new object[] { new NoteOffEvent(), new NoteOffEvent((SevenBitNumber)4, (SevenBitNumber)70) },
            new object[] { new NoteOffEvent() { Channel = (FourBitNumber)10 }, new NoteOffEvent() },
            new object[] { new NoteOffEvent((SevenBitNumber)2, (SevenBitNumber)70), new NoteOffEvent((SevenBitNumber)4, (SevenBitNumber)70) },
            new object[] { new NoteOffEvent((SevenBitNumber)4, (SevenBitNumber)70), new NoteOffEvent((SevenBitNumber)4, (SevenBitNumber)40) },
            new object[] { new NoteOffEvent((SevenBitNumber)4, (SevenBitNumber)70) { Channel = (FourBitNumber)2 }, new NoteOffEvent((SevenBitNumber)4, (SevenBitNumber)70) { Channel = (FourBitNumber)5 } },

            new object[] { new NoteOnEvent((SevenBitNumber)4, (SevenBitNumber)70), new NoteOnEvent() },
            new object[] { new NoteOnEvent(), new NoteOnEvent() { Channel = (FourBitNumber)10 } },
            new object[] { new NoteOnEvent((SevenBitNumber)2, (SevenBitNumber)70), new NoteOnEvent((SevenBitNumber)4, (SevenBitNumber)70) },
            new object[] { new NoteOnEvent((SevenBitNumber)4, (SevenBitNumber)100), new NoteOnEvent((SevenBitNumber)4, (SevenBitNumber)70) },
            new object[] { new NoteOnEvent((SevenBitNumber)4, (SevenBitNumber)70) { Channel = (FourBitNumber)5 }, new NoteOnEvent((SevenBitNumber)4, (SevenBitNumber)70) { Channel = (FourBitNumber)7 } },

            new object[] { new PitchBendEvent(1234), new PitchBendEvent() },
            new object[] { new PitchBendEvent(), new PitchBendEvent() { Channel = (FourBitNumber)5 } },
            new object[] { new PitchBendEvent(34), new PitchBendEvent(1234) },
            new object[] { new PitchBendEvent(1234) { Channel = (FourBitNumber)5 }, new PitchBendEvent(1234) { Channel = (FourBitNumber)4 } },

            new object[] { new ProgramChangeEvent((SevenBitNumber)4), new ProgramChangeEvent() },
            new object[] { new ProgramChangeEvent(), new ProgramChangeEvent() { Channel = (FourBitNumber)5 } },
            new object[] { new ProgramChangeEvent((SevenBitNumber)2), new ProgramChangeEvent((SevenBitNumber)4) },
            new object[] { new ProgramChangeEvent((SevenBitNumber)4) { Channel = (FourBitNumber)5 }, new ProgramChangeEvent((SevenBitNumber)4) { Channel = (FourBitNumber)3 } },

            new object[] { new ChannelPrefixEvent(), new ChannelPrefixEvent(10) },
            new object[] { new ChannelPrefixEvent(3), new ChannelPrefixEvent(23) },

            new object[] { new CopyrightNoticeEvent(), new CopyrightNoticeEvent("A") },
            new object[] { new CopyrightNoticeEvent("A"), new CopyrightNoticeEvent("ABC") },

            new object[] { new CuePointEvent(), new CuePointEvent("A") },
            new object[] { new CuePointEvent("A"), new CuePointEvent("ABC") },

            new object[] { new DeviceNameEvent(), new DeviceNameEvent("A") },
            new object[] { new DeviceNameEvent("A"), new DeviceNameEvent("ABC") },

            new object[] { new InstrumentNameEvent(), new InstrumentNameEvent("A") },
            new object[] { new InstrumentNameEvent("A"), new InstrumentNameEvent("ABC") },

            new object[] { new KeySignatureEvent(), new KeySignatureEvent(2, 0) },
            new object[] { new KeySignatureEvent(2, 0), new KeySignatureEvent() },
            new object[] { new KeySignatureEvent(3, 1), new KeySignatureEvent(-3, 1) },
            new object[] { new KeySignatureEvent(3, 1), new KeySignatureEvent(-3, 1) },

            new object[] { new LyricEvent(), new LyricEvent("A") },
            new object[] { new LyricEvent("A"), new LyricEvent("ABC") },

            new object[] { new MarkerEvent(), new MarkerEvent("A") },
            new object[] { new MarkerEvent("A"), new MarkerEvent("ABC") },

            new object[] { new PortPrefixEvent(), new PortPrefixEvent(11) },
            new object[] { new PortPrefixEvent(3), new PortPrefixEvent(23) },

            new object[] { new ProgramNameEvent(), new ProgramNameEvent("A") },
            new object[] { new ProgramNameEvent("A"), new ProgramNameEvent("ABC") },

            new object[] { new SequenceNumberEvent(), new SequenceNumberEvent(11) },
            new object[] { new SequenceNumberEvent(5), new SequenceNumberEvent(45) },

            new object[] { new SequencerSpecificEvent(), new SequencerSpecificEvent(new byte[] { 1 }) },
            new object[] { new SequencerSpecificEvent(new byte[] { 2, 3 }), new SequencerSpecificEvent(new byte[] { 1, 2, 3 }) },

            new object[] { new SequenceTrackNameEvent(), new SequenceTrackNameEvent("A") },
            new object[] { new SequenceTrackNameEvent("A"), new SequenceTrackNameEvent("ABC") },

            new object[] { new SetTempoEvent(), new SetTempoEvent(123) },
            new object[] { new SetTempoEvent(456), new SetTempoEvent(123456) },

            new object[] { new SmpteOffsetEvent(), new SmpteOffsetEvent(SmpteFormat.TwentyFive, 1, 2, 3, 4, 5) },
            new object[] { new SmpteOffsetEvent(SmpteFormat.Thirty, 1, 2, 3, 4, 5), new SmpteOffsetEvent(SmpteFormat.TwentyFive, 1, 2, 3, 4, 5) },
            new object[] { new SmpteOffsetEvent(SmpteFormat.TwentyFive, 2, 2, 3, 4, 5), new SmpteOffsetEvent(SmpteFormat.TwentyFive, 1, 2, 3, 4, 5) },
            new object[] { new SmpteOffsetEvent(SmpteFormat.TwentyFive, 1, 3, 3, 4, 5), new SmpteOffsetEvent(SmpteFormat.TwentyFive, 1, 2, 3, 4, 5) },
            new object[] { new SmpteOffsetEvent(SmpteFormat.TwentyFive, 1, 2, 4, 4, 5), new SmpteOffsetEvent(SmpteFormat.TwentyFive, 1, 2, 3, 4, 5) },
            new object[] { new SmpteOffsetEvent(SmpteFormat.TwentyFive, 1, 2, 3, 5, 5), new SmpteOffsetEvent(SmpteFormat.TwentyFive, 1, 2, 3, 4, 5) },
            new object[] { new SmpteOffsetEvent(SmpteFormat.TwentyFive, 1, 2, 3, 4, 6), new SmpteOffsetEvent(SmpteFormat.TwentyFive, 1, 2, 3, 4, 5) },

            new object[] { new TextEvent(), new TextEvent("A") },
            new object[] { new TextEvent("A"), new TextEvent("ABC") },

            new object[] { new TimeSignatureEvent(2, 8, 32, 64), new TimeSignatureEvent() },
            new object[] { new TimeSignatureEvent(3, 8, 32, 64), new TimeSignatureEvent(2, 8, 32, 64) },
            new object[] { new TimeSignatureEvent(2, 16, 32, 64), new TimeSignatureEvent(2, 8, 32, 64) },
            new object[] { new TimeSignatureEvent(2, 8, 64, 64), new TimeSignatureEvent(2, 8, 32, 64) },
            new object[] { new TimeSignatureEvent(2, 8, 32, 128), new TimeSignatureEvent(2, 8, 32, 64) },

            new object[] { new UnknownMetaEvent(45), new UnknownMetaEvent(5) },
            new object[] { new UnknownMetaEvent(45, new byte[] { 1, 2, 3 }), new UnknownMetaEvent(45, new byte[] { 2, 3 }) },

            new object[] { new MidiTimeCodeEvent(MidiTimeCodeComponent.HoursLsb, (FourBitNumber)12), new MidiTimeCodeEvent() },
            new object[] { new MidiTimeCodeEvent(MidiTimeCodeComponent.SecondsMsb, (FourBitNumber)3), new MidiTimeCodeEvent(MidiTimeCodeComponent.HoursLsb, (FourBitNumber)12) },
            new object[] { new MidiTimeCodeEvent(MidiTimeCodeComponent.HoursLsb, (FourBitNumber)7), new MidiTimeCodeEvent(MidiTimeCodeComponent.HoursLsb, (FourBitNumber)12) },

            new object[] { new SongPositionPointerEvent(), new SongPositionPointerEvent(23) },
            new object[] { new SongPositionPointerEvent(5), new SongPositionPointerEvent(45) },

            new object[] { new SongSelectEvent((SevenBitNumber)45), new SongSelectEvent() },
            new object[] { new SongSelectEvent((SevenBitNumber)5), new SongSelectEvent((SevenBitNumber)45) },
        };

        #endregion

        #region Test methods

        [TestCaseSource(nameof(EqualEvents))]
        public void CompareEvents_Equals(MidiEvent x, MidiEvent y)
        {
            void Check(MidiEvent a, MidiEvent b, string label)
            {
                var settings = new MidiEventEqualityCheckSettings();
                var areEqual = MidiEvent.Equals(a, b, settings, out var message);

                ClassicAssert.IsTrue(areEqual, $"Events aren't equal ({label}): {message}.");
                ClassicAssert.IsNull(message, $"Message isn't null ({label}).");
            }

            Check(x, y, "direct");
            Check(y, x, "inverted");
        }

        [Test]
        public void CompareEvents_Equals_AllEventsTypesChecked()
        {
            var checkedEventsTypes = EqualEvents
                .Select(row => ((MidiEvent)((object[])row)[0]).EventType)
                .Distinct();
            var allEventsTypes = Enum.GetValues(typeof(MidiEventType)).Cast<MidiEventType>();
            CollectionAssert.AreEquivalent(
                allEventsTypes,
                checkedEventsTypes,
                "Not all event types are checked for equality.");
        }

        [TestCaseSource(nameof(NonEqualEvents))]
        public void CompareEvents_NonEquals(MidiEvent x, MidiEvent y)
        {
            void Check(MidiEvent a, MidiEvent b, string label)
            {
                var settings = new MidiEventEqualityCheckSettings();
                var areEqual = MidiEvent.Equals(a, b, settings, out var message);

                ClassicAssert.IsFalse(areEqual, $"Events are equal ({label}).");
                ClassicAssert.IsNotNull(message, $"Message is null ({label}).");
            }

            Check(x, y, "direct");
            Check(y, x, "inverted");
        }

        [Test]
        public void CompareEvents_NonEquals_AllEventsTypesChecked()
        {
            var checkedEventsTypes = NonEqualEvents
                .Select(row => ((MidiEvent)((object[])row)[0]).EventType)
                .Distinct();

            var allEventsTypes = Enum
                .GetValues(typeof(MidiEventType))
                .Cast<MidiEventType>()
                .Except(new[] { MidiEventType.EndOfTrack, MidiEventType.CustomMeta, MidiEventType.TimingClock, MidiEventType.Start, MidiEventType.Continue, MidiEventType.Stop, MidiEventType.ActiveSensing, MidiEventType.Reset, MidiEventType.TuneRequest })
                .ToArray();
            
            CollectionAssert.AreEquivalent(
                allEventsTypes,
                checkedEventsTypes,
                "Not all event types are checked for non-equality.");
        }

        [Test]
        public void CompareEvents_DifferentTypes()
        {
            var midiEvents = EqualEvents
                .Concat(NonEqualEvents)
                .Select(row => (object[])row)
                .Select(row => row.Cast<MidiEvent>().ToArray())
                .ToArray();

            var settings = new MidiEventEqualityCheckSettings();

            for (int i = 0; i < midiEvents.Length; i++)
            {
                for (int j = 0; j < midiEvents.Length; j++)
                {
                    if (i == j)
                        continue;

                    var midiEvent1 = midiEvents[i][0];
                    var midiEvent2 = midiEvents[j][0];

                    if (midiEvent1.GetType() == midiEvent2.GetType())
                        continue;

                    var areEqual = MidiEvent.Equals(midiEvent1, midiEvent2, settings, out var message);
                    ClassicAssert.IsFalse(areEqual, $"Events of different types are equal: {midiEvent1.EventType} and {midiEvent2.EventType}.");
                    ClassicAssert.IsNotNull(message, "Message is null.");
                }
            }
        }

        [Test]
        public void CompareDeltaTimes_SameDeltaTimes()
        {
            var midiEvent1 = new NoteOnEvent { DeltaTime = 100 };
            var midiEvent2 = new NoteOnEvent { DeltaTime = 100 };

            var settings = new MidiEventEqualityCheckSettings { CompareDeltaTimes = true };
            var areEqual = MidiEvent.Equals(midiEvent1, midiEvent2, settings, out var message);
            
            ClassicAssert.IsTrue(areEqual, "Events aren't equal.");
            ClassicAssert.IsNull(message, "Message isn't null.");
        }

        [Test]
        public void CompareDeltaTimes_DifferentDeltaTimes()
        {
            var midiEvent1 = new NoteOnEvent { DeltaTime = 100 };
            var midiEvent2 = new NoteOnEvent { DeltaTime = 1000 };

            var settings = new MidiEventEqualityCheckSettings { CompareDeltaTimes = true };
            var areEqual = MidiEvent.Equals(midiEvent1, midiEvent2, settings, out var message);

            ClassicAssert.IsFalse(areEqual, "Events are equal.");
            ClassicAssert.IsNotNull(message, "Message is null.");
            ClassicAssert.IsNotEmpty(message, "Message is empty.");
        }

        [TestCase(100, 100)]
        [TestCase(100, 1000)]
        public void DontCompareDeltaTimes(long firstDeltaTime, long secondDeltaTime)
        {
            var midiEvent1 = new NoteOnEvent { DeltaTime = firstDeltaTime };
            var midiEvent2 = new NoteOnEvent { DeltaTime = secondDeltaTime };

            var settings = new MidiEventEqualityCheckSettings { CompareDeltaTimes = false };
            var areEqual = MidiEvent.Equals(midiEvent1, midiEvent2, settings, out var message);

            ClassicAssert.IsTrue(areEqual, "Events aren't equal.");
            ClassicAssert.IsNull(message, "Message isn't null.");
        }

        [Test]
        public void TextComparison_Ordinal_SameTexts()
        {
            var midiEvent1 = new TextEvent { Text = "abc" };
            var midiEvent2 = new TextEvent { Text = "abc" };

            var settings = new MidiEventEqualityCheckSettings { TextComparison = System.StringComparison.Ordinal };
            var areEqual = MidiEvent.Equals(midiEvent1, midiEvent2, settings, out var message);

            ClassicAssert.IsTrue(areEqual, "Events aren't equal.");
            ClassicAssert.IsNull(message, "Message isn't null.");
        }

        [Test]
        public void TextComparison_Ordinal_DifferentTexts()
        {
            var midiEvent1 = new TextEvent { Text = "abc" };
            var midiEvent2 = new TextEvent { Text = "Abc" };

            var settings = new MidiEventEqualityCheckSettings { TextComparison = System.StringComparison.Ordinal };
            var areEqual = MidiEvent.Equals(midiEvent1, midiEvent2, settings, out var message);

            ClassicAssert.IsFalse(areEqual, "Events are equal.");
            ClassicAssert.IsNotNull(message, "Message is null.");
            ClassicAssert.IsNotEmpty(message, "Message is empty.");
        }

        [TestCase("abc", "abc")]
        [TestCase("Abc", "aBc")]
        public void TextComparison_OrdinalIgnoreCase(string firstText, string secondText)
        {
            var midiEvent1 = new TextEvent { Text = firstText };
            var midiEvent2 = new TextEvent { Text = secondText };

            var settings = new MidiEventEqualityCheckSettings { TextComparison = System.StringComparison.OrdinalIgnoreCase };
            var areEqual = MidiEvent.Equals(midiEvent1, midiEvent2, settings, out var message);

            ClassicAssert.IsTrue(areEqual, "Events aren't equal.");
            ClassicAssert.IsNull(message, "Message isn't null.");
        }

        #endregion
    }
}
