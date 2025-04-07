using Melanchall.DryWetMidi.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed class MidiEventEqualityTests
    {
        #region Test methods

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
