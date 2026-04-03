using System;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed class MidiEventTests
    {
        #region Nested classes

        private sealed class CustomMetaEvent : MetaEvent
        {
            private byte _data;

            public CustomMetaEvent() =>
                _data = 0;

            public CustomMetaEvent(byte data) =>
                _data = data;

            protected override MidiEvent CloneEvent() =>
                new CustomMetaEvent(_data);

            protected override int GetContentSize(WritingSettings settings) =>
                1;

            protected override void ReadContent(MidiReader reader, ReadingSettings settings, int size) =>
                _data = reader.ReadByte();

            protected override void WriteContent(MidiWriter writer, WritingSettings settings) =>
                writer.WriteByte(_data);

            public override string ToString() =>
                _data.ToString();

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

        private static readonly object[] EventsWriteRead =
        {
            new object[] { new NormalSysExEvent(), new NormalSysExEvent() },
            new object[] { new NormalSysExEvent(new byte[] { 0xF0 }), new NormalSysExEvent() },
            new object[] { new NormalSysExEvent(new byte[] { 0xF0, 0xAB, 0x65, 0xF7 }), new NormalSysExEvent(new byte[] { 0xF0, 0xAB, 0x65, 0xF7 }) },
            new object[] { new NormalSysExEvent(new byte[] { 0xAB, 0x65, 0xF7 }), new NormalSysExEvent(new byte[] { 0xF0, 0xAB, 0x65, 0xF7 }) },
            new object[] { new EscapeSysExEvent(), new EscapeSysExEvent() },
            new object[] { new EscapeSysExEvent(new byte[] { 0xF7 }), new EscapeSysExEvent() },
            new object[] { new EscapeSysExEvent(new byte[] { 0xF7, 0xAB, 0x65, 0xF7 }), new EscapeSysExEvent(new byte[] { 0xF7, 0xAB, 0x65, 0xF7 }) },
            new object[] { new EscapeSysExEvent(new byte[] { 0xAB, 0x65, 0xF7 }), new EscapeSysExEvent(new byte[] { 0xF7, 0xAB, 0x65, 0xF7 }) },
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

            new object[] { new CustomMetaEvent(10), new CustomMetaEvent(10) },
        };

        #endregion

        #region Test methods

        [Test]
        public void AllEventTypesAreCorrect()
        {
            foreach (var type in TypesProvider.GetAllEventTypes())
            {
                var instance = type == typeof(UnknownMetaEvent)
                    ? new UnknownMetaEvent(0)
                    : (MidiEvent)Activator.CreateInstance(type, true);
                var eventType = instance.EventType;
                ClassicAssert.IsTrue(
                    type.Name.StartsWith(eventType.ToString()),
                    $"Type '{eventType}' is invalid for events of type '{type.Name}'.");
            }
        }

        [Test]
        public void AllEventTypesHaveParameterlessConstructor()
        {
            foreach (var type in TypesProvider.GetAllEventTypes())
            {
                if (type == typeof(UnknownMetaEvent) || type == typeof(EndOfTrackEvent))
                    continue;

                ClassicAssert.IsNotNull(
                    type.GetConstructor(Type.EmptyTypes),
                    $"Type '{type.Name}' has no parameterless constructor.");
            }
        }

        [Test]
        public void CloneEvent()
        {
            foreach (var type in TypesProvider.GetAllEventTypes())
            {
                var midiEvent = type == typeof(UnknownMetaEvent)
                    ? new UnknownMetaEvent(1)
                    : (MidiEvent)Activator.CreateInstance(type, true);

                if (midiEvent is ChannelEvent channelEvent)
                    channelEvent.Channel = (FourBitNumber)(DryWetMidi.Common.Random.Instance.Next(5) + 5);

                if (midiEvent is BaseTextEvent baseTextEvent)
                    baseTextEvent.Text = DryWetMidi.Common.Random.Instance.Next(1000).ToString();

                midiEvent.DeltaTime = DryWetMidi.Common.Random.Instance.Next(1000) + 1;

                var midiEventClone = midiEvent.Clone();
                MidiAsserts.AreEqual(midiEvent, midiEventClone, true, $"Clone of {type} is invalid.");
            }
        }

        [TestCaseSource(nameof(EventsWriteRead))]
        public void AllEventTypesAreReadCorrectly(
            MidiEvent midiEvent,
            MidiEvent expectedMidiEvent)
        {
            byte customMetaEventStatusByte = 0x5A;

            var writingSettings = new WritingSettings
            {
                CustomMetaEventTypes = new EventTypesCollection
                {
                    { typeof(CustomMetaEvent), customMetaEventStatusByte }
                }
            };

            var readingSettings = new ReadingSettings
            {
                CustomMetaEventTypes = new EventTypesCollection
                {
                    { typeof(CustomMetaEvent), customMetaEventStatusByte }
                },
                SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOn,
                EndOfTrackStoringPolicy = EndOfTrackStoringPolicy.Store
            };

            var midiFile = MidiFileTestUtilities.Read(
                new MidiFile(new TrackChunk(new[] { midiEvent })),
                writingSettings,
                readingSettings,
                MidiFileFormat.SingleTrack);
            var readMidiEvent = midiFile.GetTrackChunks().First().Events.First();
            MidiAsserts.AreEqual(expectedMidiEvent, readMidiEvent, true, "MIDI event is invalid.");
        }

        [Test]
        public void AllEventTypesAreReadCorrectly_AllEventsTypesChecked()
        {
            var checkedEventsTypes = EventsWriteRead
                .Select(row => ((MidiEvent)((object[])row)[0]).EventType)
                .Distinct();
            
            var allEventsTypes = Enum
                .GetValues(typeof(MidiEventType))
                .Cast<MidiEventType>()
                .Except(TypesProvider
                    .GetAllEventTypes()
                    .Where(t => typeof(SystemRealTimeEvent).IsAssignableFrom(t) || typeof(SystemCommonEvent).IsAssignableFrom(t))
                    .Select(t => ((MidiEvent)Activator.CreateInstance(t)).EventType))
                .ToArray();
            
            CollectionAssert.AreEquivalent(
                allEventsTypes,
                checkedEventsTypes,
                "Not all event types are checked for equality.");
        }

        [Test]
        public void CheckDeltaTimeWriteRead_Max()
        {
            var midiEvent = new TextEvent("A") { DeltaTime = long.MaxValue };
            var midiFile = MidiFileTestUtilities.Read(
                new MidiFile(new TrackChunk(new[] { midiEvent })),
                null,
                null,
                MidiFileFormat.SingleTrack);
            var readMidiEvent = midiFile.GetTrackChunks().First().Events.First();
            MidiAsserts.AreEqual(midiEvent, readMidiEvent, true, "MIDI event is invalid.");
        }

        [Test]
        public void GetStandardMetaEventStatusBytes()
        {
            var statusBytes = MetaEvent.GetStandardMetaEventStatusBytes();
            CollectionAssert.AreEqual(
                new[]
                {
                    0x00,
                    0x01,
                    0x02,
                    0x03,
                    0x04,
                    0x05,
                    0x06,
                    0x07,
                    0x08,
                    0x09,
                    0x20,
                    0x21,
                    0x2F,
                    0x51,
                    0x54,
                    0x58,
                    0x59,
                    0x7F
                },
                statusBytes,
                "Status bytes are invalid.");
        }

        #endregion
    }
}
