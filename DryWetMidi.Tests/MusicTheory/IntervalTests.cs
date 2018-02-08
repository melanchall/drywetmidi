using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.MusicTheory
{
    [TestFixture]
    public class IntervalTests
    {
        #region Test methods

        [Test]
        [Description("Get upward interval and check its direction.")]
        public void GetUp()
        {
            Assert.AreEqual(IntervalDirection.Up, Interval.GetUp(SevenBitNumber.MaxValue).Direction);
        }

        [Test]
        [Description("Get downward interval and check its direction.")]
        public void GetDown()
        {
            Assert.AreEqual(IntervalDirection.Down, Interval.GetDown(SevenBitNumber.MaxValue).Direction);
        }

        [Test]
        [Description("Get upward interval and get its downward version.")]
        public void GetUp_Down()
        {
            Assert.AreEqual(IntervalDirection.Down, Interval.GetUp(SevenBitNumber.MaxValue).Down().Direction);
        }

        [Test]
        [Description("Get downward interval and get its upward version.")]
        public void GetDown_Up()
        {
            Assert.AreEqual(IntervalDirection.Up, Interval.GetDown(SevenBitNumber.MaxValue).Up().Direction);
        }

        [Test]
        [Description("Check that interval of the same steps number are equal by reference.")]
        public void CheckReferences()
        {
            Assert.IsTrue(ReferenceEquals(Interval.FromHalfSteps(10), Interval.FromHalfSteps(10)));
        }

        #endregion
    }
}
