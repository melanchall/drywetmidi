using Melanchall.DryWetMidi.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed class EventTypeTests
    {
        #region Test methods

        [Test]
        public void CheckEquality_SameTypeAndStatusByte()
        {
            var eventType1 = new EventType(typeof(NoteOnEvent), 0x90);
            var eventType2 = new EventType(typeof(NoteOnEvent), 0x90);

            ClassicAssert.IsTrue(eventType1 == eventType2, "Event types are not equal via ==.");
            ClassicAssert.IsFalse(eventType1 != eventType2, "Event types are not equal via !=.");
            ClassicAssert.IsTrue(eventType1.Equals(eventType2), "Event types are not equal via Equals.");
            ClassicAssert.AreEqual(eventType1.GetHashCode(), eventType2.GetHashCode(), "Hash codes are different.");
        }

        [Test]
        public void CheckEquality_DifferentType()
        {
            var eventType1 = new EventType(typeof(NoteOnEvent), 0x90);
            var eventType2 = new EventType(typeof(NoteOffEvent), 0x90);

            ClassicAssert.IsFalse(eventType1 == eventType2, "Event types are equal via ==.");
            ClassicAssert.IsTrue(eventType1 != eventType2, "Event types are equal via !=.");
            ClassicAssert.IsFalse(eventType1.Equals(eventType2), "Event types are equal via Equals.");
        }

        [Test]
        public void CheckEquality_DifferentStatusByte()
        {
            var eventType1 = new EventType(typeof(NoteOnEvent), 0x90);
            var eventType2 = new EventType(typeof(NoteOnEvent), 0x80);

            ClassicAssert.IsFalse(eventType1 == eventType2, "Event types are equal via ==.");
            ClassicAssert.IsTrue(eventType1 != eventType2, "Event types are equal via !=.");
            ClassicAssert.IsFalse(eventType1.Equals(eventType2), "Event types are equal via Equals.");
        }

        [Test]
        public void CheckEquality_Null()
        {
            var eventType = new EventType(typeof(NoteOnEvent), 0x90);
            EventType? nullEventType = null;

            ClassicAssert.IsFalse(eventType == nullEventType, "Event type is equal to null via ==.");
            ClassicAssert.IsFalse(nullEventType == eventType, "Null is equal to event type via ==.");
            ClassicAssert.IsTrue(eventType != nullEventType, "Event type is equal to null via !=.");
            ClassicAssert.IsFalse(eventType.Equals(null), "Event type is equal to null via Equals.");
        }

        #endregion
    }
}
