using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class TimedEventTests
    {
        #region Test methods

        [Test]
        [Description("Check that clone of a timed event equals to the original one.")]
        public void Clone()
        {
            var timedEvent = new TimedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)50));
            Assert.IsTrue(TimedEventEquality.AreEqual(timedEvent, timedEvent.Clone(), true),
                          "Clone of a timed event doesn't equal to the original one.");
        }

        [Test]
        public void CheckTimeChangedEvent_ZeroTime_NoChange()
        {
            CheckTimeChangedEvent_NoChange(GetTimedEvent_ZeroTime());
        }

        [Test]
        public void CheckTimeChangedEvent_NonZeroTime_NoChange()
        {
            CheckTimeChangedEvent_NoChange(GetTimedEvent_NonzeroTime());
        }

        [Test]
        public void CheckTimeChangedEvent_ZeroTime_Changed()
        {
            CheckTimeChangedEvent_Changed(GetTimedEvent_ZeroTime());
        }

        [Test]
        public void CheckTimeChangedEvent_NonZeroTime_Changed()
        {
            CheckTimeChangedEvent_Changed(GetTimedEvent_NonzeroTime());
        }

        #endregion

        #region Private methods

        private static void CheckTimeChangedEvent_NoChange(TimedEvent timedEvent)
        {
            object timeChangedSender = null;
            TimeChangedEventArgs timeChangedEventArgs = null;

            timedEvent.TimeChanged += (sender, eventArgs) =>
            {
                timeChangedSender = sender;
                timeChangedEventArgs = eventArgs;
            };

            timedEvent.Time = timedEvent.Time;

            Assert.IsNull(timeChangedSender, "Sender is not null.");
            Assert.IsNull(timeChangedEventArgs, "Event args is not null.");
        }

        private static void CheckTimeChangedEvent_Changed(TimedEvent timedEvent)
        {
            object timeChangedSender = null;
            TimeChangedEventArgs timeChangedEventArgs = null;

            timedEvent.TimeChanged += (sender, eventArgs) =>
            {
                timeChangedSender = sender;
                timeChangedEventArgs = eventArgs;
            };

            var oldTime = timedEvent.Time;
            timedEvent.Time += 100;

            Assert.AreSame(timedEvent, timeChangedSender, "Sender is invalid.");

            Assert.IsNotNull(timeChangedEventArgs, "Event args is null.");
            Assert.AreEqual(oldTime, timeChangedEventArgs.OldTime, "Old time is invalid.");
            Assert.AreEqual(timedEvent.Time, timeChangedEventArgs.NewTime, "New time is invalid.");
            Assert.AreNotEqual(oldTime, timedEvent.Time, "New time is equal to old one.");
        }

        private static TimedEvent GetTimedEvent_ZeroTime()
        {
            return new TimedEvent(new ProgramChangeEvent((SevenBitNumber)10), 0);
        }

        private static TimedEvent GetTimedEvent_NonzeroTime()
        {
            return new TimedEvent(new ProgramChangeEvent((SevenBitNumber)10), 100);
        }

        #endregion
    }
}
