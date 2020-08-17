using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed class MidiEventTests
    {
        #region Constants

        private static readonly NumbersProvider NumbersProvider = new NumbersProvider();

        private static readonly Dictionary<Type, Action<MidiEvent, Random>> NonDefaultMidiEventsModifiers =
            new Dictionary<Type, Action<MidiEvent, Random>>
            {
                // SysEx

                [typeof(SysExEvent)] = (midiEvent, random) =>
                {
                    ((SysExEvent)midiEvent).Data = NumbersProvider.GetNonDefaultBytesArray(3, 10, 100);
                },

                // Channel

                [typeof(ChannelEvent)] = (midiEvent, random) =>
                {
                    ((ChannelEvent)midiEvent).Channel = NumbersProvider.GetNonDefaultFourBitNumber();
                },
                [typeof(NoteEvent)] = (midiEvent, random) =>
                {
                    var noteEvent = (NoteEvent)midiEvent;
                    noteEvent.NoteNumber = NumbersProvider.GetNonDefaultSevenBitNumber();
                    noteEvent.Velocity = NumbersProvider.GetNonDefaultSevenBitNumber();
                },
                [typeof(ChannelAftertouchEvent)] = (midiEvent, random) =>
                {
                    ((ChannelAftertouchEvent)midiEvent).AftertouchValue = NumbersProvider.GetNonDefaultSevenBitNumber();
                },
                [typeof(ControlChangeEvent)] = (midiEvent, random) =>
                {
                    var controlChangeEvent = (ControlChangeEvent)midiEvent;
                    controlChangeEvent.ControlNumber = NumbersProvider.GetNonDefaultSevenBitNumber();
                    controlChangeEvent.ControlValue = NumbersProvider.GetNonDefaultSevenBitNumber();
                },
                [typeof(NoteAftertouchEvent)] = (midiEvent, random) =>
                {
                    var noteAftertouchEvent = (NoteAftertouchEvent)midiEvent;
                    noteAftertouchEvent.NoteNumber = NumbersProvider.GetNonDefaultSevenBitNumber();
                    noteAftertouchEvent.AftertouchValue = NumbersProvider.GetNonDefaultSevenBitNumber();
                },
                [typeof(PitchBendEvent)] = (midiEvent, random) =>
                {
                    ((PitchBendEvent)midiEvent).PitchValue = NumbersProvider.GetNonDefaultUShort(PitchBendEvent.MaxPitchValue);
                },
                [typeof(ProgramChangeEvent)] = (midiEvent, random) =>
                {
                    ((ProgramChangeEvent)midiEvent).ProgramNumber = NumbersProvider.GetNonDefaultSevenBitNumber();
                },

                // Meta

                [typeof(BaseTextEvent)] = (midiEvent, random) =>
                {
                    ((BaseTextEvent)midiEvent).Text = Guid.NewGuid().ToString();
                },
                [typeof(SequencerSpecificEvent)] = (midiEvent, random) =>
                {
                    ((SequencerSpecificEvent)midiEvent).Data = NumbersProvider.GetNonDefaultBytesArray(3, 10, 100);
                },
                [typeof(ChannelPrefixEvent)] = (midiEvent, random) =>
                {
                    ((ChannelPrefixEvent)midiEvent).Channel = NumbersProvider.GetNonDefaultByte();
                },
                [typeof(KeySignatureEvent)] = (midiEvent, random) =>
                {
                    var keySignatureEvent = (KeySignatureEvent)midiEvent;
                    keySignatureEvent.Key = (sbyte)random.Next(1, 8);
                    keySignatureEvent.Scale = 1;
                },
                [typeof(PortPrefixEvent)] = (midiEvent, random) =>
                {
                    ((PortPrefixEvent)midiEvent).Port = NumbersProvider.GetNonDefaultByte();
                },
                [typeof(SequenceNumberEvent)] = (midiEvent, random) =>
                {
                    ((SequenceNumberEvent)midiEvent).Number = NumbersProvider.GetNonDefaultUShort();
                },
                [typeof(SetTempoEvent)] = (midiEvent, random) =>
                {
                    ((SetTempoEvent)midiEvent).MicrosecondsPerQuarterNote = random.Next(1, 16777216);
                },
                [typeof(SmpteOffsetEvent)] = (midiEvent, random) =>
                {
                    var smpteOffsetEvent = (SmpteOffsetEvent)midiEvent;
                    smpteOffsetEvent.Format = SmpteFormat.Thirty;
                    smpteOffsetEvent.Frames = (byte)random.Next(1, 30);
                    smpteOffsetEvent.SubFrames = (byte)random.Next(1, 100);
                    smpteOffsetEvent.Hours = (byte)random.Next(1, 24);
                    smpteOffsetEvent.Minutes = (byte)random.Next(1, 60);
                    smpteOffsetEvent.Seconds = (byte)random.Next(1, 60);
                },
                [typeof(TimeSignatureEvent)] = (midiEvent, random) =>
                {
                    var timeSignatureEvent = (TimeSignatureEvent)midiEvent;
                    timeSignatureEvent.Numerator = NumbersProvider.GetNonDefaultByte();
                    timeSignatureEvent.Denominator = NumbersProvider.GetNonDefaultByte(v => MathUtilities.IsPowerOfTwo(v));
                    timeSignatureEvent.ClocksPerClick = NumbersProvider.GetNonDefaultByte();
                    timeSignatureEvent.ThirtySecondNotesPerBeat = NumbersProvider.GetNonDefaultByte();
                },
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
                    : (MidiEvent)Activator.CreateInstance(type);
                var eventType = instance.EventType;
                Assert.IsTrue(
                    type.Name.StartsWith(eventType.ToString()),
                    $"Type '{eventType}' is invalid for events of type '{type.Name}'.");
            }
        }

        [Test]
        public void AllEventTypesHaveParameterlessConstructor()
        {
            foreach (var type in TypesProvider.GetAllEventTypes())
            {
                if (type == typeof(UnknownMetaEvent))
                    continue;

                Assert.IsNotNull(
                    type.GetConstructor(Type.EmptyTypes),
                    $"Type '{type.Name}' has no parameterless constructor.");
            }
        }

        [Test]
        public void CloneEvent()
        {
            var random = new Random();

            foreach (var type in TypesProvider.GetAllEventTypes())
            {
                var midiEvent = type == typeof(UnknownMetaEvent)
                    ? new UnknownMetaEvent(1)
                    : (MidiEvent)Activator.CreateInstance(type);

                if (midiEvent is ChannelEvent channelEvent)
                    channelEvent.Channel = (FourBitNumber)(random.Next(5) + 5);

                if (midiEvent is BaseTextEvent baseTextEvent)
                    baseTextEvent.Text = random.Next(1000).ToString();

                midiEvent.DeltaTime = random.Next(1000) + 1;

                var midiEventClone = midiEvent.Clone();
                MidiAsserts.AreEventsEqual(midiEvent, midiEventClone, true, $"Clone of {type} is invalid.");
            }
        }

        [Repeat(10)]
        [Test]
        public void AllEventTypesAreReadCorrectly_NonDefault()
        {
            AllEventTypesAreReadCorrectly(CreateNonDefaultMidiEvent);
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

        #region Private methods

        public void AllEventTypesAreReadCorrectly(Func<Type, MidiEvent> createMidiEvent)
        {
            var events = TypesProvider.GetAllEventTypes()
                .Where(t => !typeof(SystemCommonEvent).IsAssignableFrom(t) &&
                            !typeof(SystemRealTimeEvent).IsAssignableFrom(t) &&
                            t != typeof(EndOfTrackEvent) &&
                            t != typeof(UnknownMetaEvent))
                .Select(createMidiEvent)
                .ToArray();

            var midiFile = MidiFileTestUtilities.Read(
                new MidiFile(new TrackChunk(events)),
                null,
                new ReadingSettings { SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOn },
                MidiFileFormat.SingleTrack);

            var readEvents = midiFile.GetEvents().ToArray();

            for (var i = 0; i < events.Length; i++)
            {
                var expectedEvent = events[i];
                var actualEvent = readEvents[i];

                MidiAsserts.AreEventsEqual(expectedEvent, actualEvent, true, $"Event {i} is invalid.");
            }
        }

        private static MidiEvent CreateNonDefaultMidiEvent(Type midiEventType)
        {
            var midiEvent = CreateDefaultMidiEvent(midiEventType);
            
            var random = new Random();
            var modifiersCount = 0;

            foreach (var modifier in NonDefaultMidiEventsModifiers)
            {
                if (!modifier.Key.IsAssignableFrom(midiEventType))
                    continue;

                modifier.Value(midiEvent, random);
                modifiersCount++;
            }

            var expectedModifiersCount = 1;
            if (midiEvent is ChannelEvent)
                expectedModifiersCount++;

            Assert.AreEqual(expectedModifiersCount, modifiersCount, $"Non-default MIDI event modifiers count is invalid for '{midiEventType}'.");
            return midiEvent;
        }

        private static MidiEvent CreateDefaultMidiEvent(Type midiEventType)
        {
            return (MidiEvent)Activator.CreateInstance(midiEventType);
        }

        #endregion
    }
}
