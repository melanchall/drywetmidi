using Melanchall.DryWetMidi.Smf;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Melanchall.DryWetMidi.Tests.Smf
{
    public abstract class BaseEventTests<TEvent> where TEvent : MidiEvent
    {
        #region Test methods

        [TestMethod]
        [Description("The Clone method produces an instance that equals the original one with delta-time of zero.")]
        public void OriginalAndCloneAreEqual_ZeroDeltaTime()
        {
            var midiEvent = CreateEvent1();
            Assert.AreEqual(midiEvent, midiEvent.Clone());
        }

        [TestMethod]
        [Description("The Clone method produces an instance that equals the original one with nonzero delta-time.")]
        public void OriginalAndCloneAreEqual_NonzeroDeltaTime()
        {
            var midiEvent = CreateEvent1();
            midiEvent.DeltaTime = 100;

            Assert.AreEqual(midiEvent, midiEvent.Clone());
            Assert.AreEqual(midiEvent.DeltaTime, midiEvent.Clone().DeltaTime);
        }

        #endregion

        #region Abstract methods

        protected abstract TEvent CreateEvent1();

        protected abstract TEvent CreateEvent2();

        #endregion
    }
}
