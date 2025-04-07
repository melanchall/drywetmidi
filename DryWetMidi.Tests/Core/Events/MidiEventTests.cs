using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed class MidiEventTests
    {
        #region Constants

        private static readonly NumbersProvider NumbersProvider = new NumbersProvider();

        private static readonly Dictionary<Type, Action<MidiEvent>> NonDefaultMidiEventsModifiers =
            new Dictionary<Type, Action<MidiEvent>>
            {
                // SysEx

                [typeof(SysExEvent)] = midiEvent =>
                {
                    ((SysExEvent)midiEvent).Data = NumbersProvider.GetNonDefaultBytesArray(3, 10, 100);
                },

                // Channel

                [typeof(ChannelEvent)] = midiEvent =>
                {
                    ((ChannelEvent)midiEvent).Channel = NumbersProvider.GetNonDefaultFourBitNumber();
                },
                [typeof(NoteEvent)] = midiEvent =>
                {
                    var noteEvent = (NoteEvent)midiEvent;
                    noteEvent.NoteNumber = NumbersProvider.GetNonDefaultSevenBitNumber();
                    noteEvent.Velocity = NumbersProvider.GetNonDefaultSevenBitNumber();
                },
                [typeof(ChannelAftertouchEvent)] = midiEvent =>
                {
                    ((ChannelAftertouchEvent)midiEvent).AftertouchValue = NumbersProvider.GetNonDefaultSevenBitNumber();
                },
                [typeof(ControlChangeEvent)] = midiEvent =>
                {
                    var controlChangeEvent = (ControlChangeEvent)midiEvent;
                    controlChangeEvent.ControlNumber = NumbersProvider.GetNonDefaultSevenBitNumber();
                    controlChangeEvent.ControlValue = NumbersProvider.GetNonDefaultSevenBitNumber();
                },
                [typeof(NoteAftertouchEvent)] = midiEvent =>
                {
                    var noteAftertouchEvent = (NoteAftertouchEvent)midiEvent;
                    noteAftertouchEvent.NoteNumber = NumbersProvider.GetNonDefaultSevenBitNumber();
                    noteAftertouchEvent.AftertouchValue = NumbersProvider.GetNonDefaultSevenBitNumber();
                },
                [typeof(PitchBendEvent)] = midiEvent =>
                {
                    ((PitchBendEvent)midiEvent).PitchValue = NumbersProvider.GetNonDefaultUShort(PitchBendEvent.MaxPitchValue);
                },
                [typeof(ProgramChangeEvent)] = midiEvent =>
                {
                    ((ProgramChangeEvent)midiEvent).ProgramNumber = NumbersProvider.GetNonDefaultSevenBitNumber();
                },

                // Meta

                [typeof(BaseTextEvent)] = midiEvent =>
                {
                    ((BaseTextEvent)midiEvent).Text = Guid.NewGuid().ToString();
                },
                [typeof(SequencerSpecificEvent)] = midiEvent =>
                {
                    ((SequencerSpecificEvent)midiEvent).Data = NumbersProvider.GetNonDefaultBytesArray(3, 10, 100);
                },
                [typeof(ChannelPrefixEvent)] = midiEvent =>
                {
                    ((ChannelPrefixEvent)midiEvent).Channel = NumbersProvider.GetNonDefaultByte();
                },
                [typeof(KeySignatureEvent)] = midiEvent =>
                {
                    var keySignatureEvent = (KeySignatureEvent)midiEvent;
                    keySignatureEvent.Key = (sbyte)DryWetMidi.Common.Random.Instance.Next(1, 8);
                    keySignatureEvent.Scale = 1;
                },
                [typeof(PortPrefixEvent)] = midiEvent =>
                {
                    ((PortPrefixEvent)midiEvent).Port = NumbersProvider.GetNonDefaultByte();
                },
                [typeof(SequenceNumberEvent)] = midiEvent =>
                {
                    ((SequenceNumberEvent)midiEvent).Number = NumbersProvider.GetNonDefaultUShort();
                },
                [typeof(SetTempoEvent)] = midiEvent =>
                {
                    ((SetTempoEvent)midiEvent).MicrosecondsPerQuarterNote = DryWetMidi.Common.Random.Instance.Next(1, 16777216);
                },
                [typeof(SmpteOffsetEvent)] = midiEvent =>
                {
                    var smpteOffsetEvent = (SmpteOffsetEvent)midiEvent;
                    smpteOffsetEvent.Format = SmpteFormat.Thirty;
                    smpteOffsetEvent.Frames = (byte)DryWetMidi.Common.Random.Instance.Next(1, 30);
                    smpteOffsetEvent.SubFrames = (byte)DryWetMidi.Common.Random.Instance.Next(1, 100);
                    smpteOffsetEvent.Hours = (byte)DryWetMidi.Common.Random.Instance.Next(1, 24);
                    smpteOffsetEvent.Minutes = (byte)DryWetMidi.Common.Random.Instance.Next(1, 60);
                    smpteOffsetEvent.Seconds = (byte)DryWetMidi.Common.Random.Instance.Next(1, 60);
                },
                [typeof(TimeSignatureEvent)] = midiEvent =>
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

                MidiAsserts.AreEqual(expectedEvent, actualEvent, true, $"Event {i} is invalid.");
            }
        }

        private static MidiEvent CreateNonDefaultMidiEvent(Type midiEventType)
        {
            var midiEvent = CreateDefaultMidiEvent(midiEventType);
            
            var modifiersCount = 0;

            foreach (var modifier in NonDefaultMidiEventsModifiers)
            {
                if (!modifier.Key.IsAssignableFrom(midiEventType))
                    continue;

                modifier.Value(midiEvent);
                modifiersCount++;
            }

            var expectedModifiersCount = 1;
            if (midiEvent is ChannelEvent)
                expectedModifiersCount++;

            ClassicAssert.AreEqual(expectedModifiersCount, modifiersCount, $"Non-default MIDI event modifiers count is invalid for '{midiEventType}'.");
            return midiEvent;
        }

        private static MidiEvent CreateDefaultMidiEvent(Type midiEventType)
        {
            return (MidiEvent)Activator.CreateInstance(midiEventType);
        }

        #endregion
    }
}
