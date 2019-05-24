using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Smf;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Smf
{
    [TestFixture]
    public sealed class MidiEventTests
    {
        [Test]
        public void AllEventTypesAreCorrect()
        {
            foreach (var type in GetAllEventTypes())
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
            foreach (var type in GetAllEventTypes())
            {
                if (type == typeof(UnknownMetaEvent))
                    continue;

                Assert.IsNotNull(
                    type.GetConstructor(Type.EmptyTypes),
                    $"Type '{type.Name}' has no parameterless constructor.");
            }
        }

        private static IEnumerable<Type> GetAllEventTypes()
        {
            var midiEventType = typeof(MidiEvent);
            return midiEventType
                .Assembly
                .GetTypes()
                .Where(t => !t.IsAbstract && t.IsSubclassOf(midiEventType))
                .ToList();
        }
    }
}
