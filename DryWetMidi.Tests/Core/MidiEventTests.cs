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
        public void CloneEvent_TypeIsCorrect()
        {
            foreach (var type in TypesProvider.GetAllEventTypes())
            {
                var midiEvent = type == typeof(UnknownMetaEvent)
                    ? new UnknownMetaEvent(1)
                    : (MidiEvent)Activator.CreateInstance(type);
                var midiEventClone = midiEvent.Clone();
                Assert.AreEqual(type, midiEventClone.GetType(), $"Clone of {type} is of invalid type.");
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

        #endregion
    }
}
