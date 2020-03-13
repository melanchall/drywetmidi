using System;
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

        [Test]
        public void AllEventTypesAreReadCorrectly()
        {
            var events = TypesProvider.GetAllEventTypes()
                .Where(t => !typeof(SystemCommonEvent).IsAssignableFrom(t) &&
                            !typeof(SystemRealTimeEvent).IsAssignableFrom(t) &&
                            t != typeof(EndOfTrackEvent) &&
                            t != typeof(UnknownMetaEvent))
                .Select(t =>
                {
                    var instance = (MidiEvent)Activator.CreateInstance(t);

                    if (instance is SysExEvent sysExEvent)
                        sysExEvent.Data = new byte[] { 1, 2, 3 };

                    if (instance is SequencerSpecificEvent sequencerSpecificEvent)
                        sequencerSpecificEvent.Data = new byte[] { 1, 2, 3 };

                    if (instance is NoteOnEvent noteOnEvent)
                        noteOnEvent.Velocity = (SevenBitNumber)100;

                    if (instance is BaseTextEvent baseTextEvent)
                        baseTextEvent.Text = Guid.NewGuid().ToString();

                    return instance;
                })
                .ToArray();

            var midiFile = MidiFileTestUtilities.Read(new MidiFile(new TrackChunk(events)), null, null, MidiFileFormat.SingleTrack);
            var readEvents = midiFile.GetEvents().ToArray();

            for (var i = 0; i < events.Length; i++)
            {
                var expectedEvent = events[i];
                var actualEvent = readEvents[i];

                MidiAsserts.AreEventsEqual(expectedEvent, actualEvent, true, $"Event {i} is invalid.");
            }
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
