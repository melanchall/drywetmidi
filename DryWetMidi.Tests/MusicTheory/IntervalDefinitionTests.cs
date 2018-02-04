using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Melanchall.DryWetMidi.Tests.MusicTheory
{
    [TestClass]
    public class IntervalDefinitionTests
    {
        #region Test methods

        [TestMethod]
        [Description("Get upward interval and check its direction.")]
        public void GetUp()
        {
            Assert.AreEqual(IntervalDirection.Up, Interval.GetUp(SevenBitNumber.MaxValue).Direction);
        }

        [TestMethod]
        [Description("Get downward interval and check its direction.")]
        public void GetDown()
        {
            Assert.AreEqual(IntervalDirection.Down, Interval.GetDown(SevenBitNumber.MaxValue).Direction);
        }

        [TestMethod]
        [Description("Get upward interval and get its downward version.")]
        public void GetUp_Down()
        {
            Assert.AreEqual(IntervalDirection.Down, Interval.GetUp(SevenBitNumber.MaxValue).Down().Direction);
        }

        [TestMethod]
        [Description("Get downward interval and get its upward version.")]
        public void GetDown_Up()
        {
            Assert.AreEqual(IntervalDirection.Up, Interval.GetDown(SevenBitNumber.MaxValue).Up().Direction);
        }

        [TestMethod]
        [Description("Check that interval definitions of the same steps number are equal by reference.")]
        public void CheckReferences()
        {
            Assert.IsTrue(ReferenceEquals(Interval.FromHalfSteps(10), Interval.FromHalfSteps(10)));
        }

        #endregion
    }
}
