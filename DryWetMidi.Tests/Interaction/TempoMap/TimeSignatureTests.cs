using System;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class TimeSignatureTests
    {
        #region Test methods

        [Test]
        public void CheckEquality_FirstNull()
        {
            var timeSignature1 = default(TimeSignature);
            var timeSignature2 = new TimeSignature(2, 8);
            ClassicAssert.IsFalse(timeSignature1 == timeSignature2, "First time signature equals to second one.");
        }

        [Test]
        public void CheckEquality_SecondNull()
        {
            var timeSignature1 = new TimeSignature(2, 8);
            var timeSignature2 = default(TimeSignature);
            ClassicAssert.IsFalse(timeSignature1 == timeSignature2, "First time signature equals to second one.");
        }

        [Test]
        public void CheckEquality_BothNull()
        {
            var timeSignature1 = default(TimeSignature);
            var timeSignature2 = default(TimeSignature);
            ClassicAssert.IsTrue(timeSignature1 == timeSignature2, "First time signature doesn't equal to second one.");
        }

        [Test]
        public void CheckEquality_SameValues()
        {
            var timeSignature1 = new TimeSignature(2, 8);
            var timeSignature2 = new TimeSignature(2, 8);
            ClassicAssert.IsTrue(timeSignature1 == timeSignature2, "First time signature doesn't equal to second one.");
        }

        [Test]
        public void CheckEquality_DifferentValues()
        {
            var timeSignature1 = new TimeSignature(2, 8);
            var timeSignature2 = new TimeSignature(1, 16);
            ClassicAssert.IsFalse(timeSignature1 == timeSignature2, "First time signature equals to second one.");
        }

        [Test]
        public void CheckInequality_FirstNull()
        {
            var timeSignature1 = default(TimeSignature);
            var timeSignature2 = new TimeSignature(2, 8);
            ClassicAssert.IsTrue(timeSignature1 != timeSignature2, "First time signature equals to second one.");
        }

        [Test]
        public void CheckInequality_SecondNull()
        {
            var timeSignature1 = new TimeSignature(2, 8);
            var timeSignature2 = default(TimeSignature);
            ClassicAssert.IsTrue(timeSignature1 != timeSignature2, "First time signature equals to second one.");
        }

        [Test]
        public void CheckInequality_BothNull()
        {
            var timeSignature1 = default(TimeSignature);
            var timeSignature2 = default(TimeSignature);
            ClassicAssert.IsFalse(timeSignature1 != timeSignature2, "First time signature doesn't equal to second one.");
        }

        [Test]
        public void CheckInequality_SameValues()
        {
            var timeSignature1 = new TimeSignature(2, 8);
            var timeSignature2 = new TimeSignature(2, 8);
            ClassicAssert.IsFalse(timeSignature1 != timeSignature2, "First time signature doesn't equal to second one.");
        }

        [Test]
        public void CheckInequality_DifferentValues()
        {
            var timeSignature1 = new TimeSignature(2, 8);
            var timeSignature2 = new TimeSignature(1, 16);
            ClassicAssert.IsTrue(timeSignature1 != timeSignature2, "First time signature equals to second one.");
        }

        [Test]
        public void Less_FirstNull()
        {
            var timeSignature1 = default(TimeSignature);
            var timeSignature2 = new TimeSignature(2, 8);
            ClassicAssert.Throws<ArgumentNullException>(() => { var x = timeSignature1 < timeSignature2; }, "Exception not thrown for null time signature.");
        }

        [Test]
        public void Less_SecondNull()
        {
            var timeSignature1 = new TimeSignature(2, 8);
            var timeSignature2 = default(TimeSignature);
            ClassicAssert.Throws<ArgumentNullException>(() => { var x = timeSignature1 < timeSignature2; }, "Exception not thrown for null time signature.");
        }

        [Test]
        public void Less_BothNull()
        {
            var timeSignature1 = default(TimeSignature);
            var timeSignature2 = default(TimeSignature);
            ClassicAssert.Throws<ArgumentNullException>(() => { var x = timeSignature1 < timeSignature2; }, "Exception not thrown for null time signature.");
        }

        [Test]
        public void Less_SameValues()
        {
            var timeSignature1 = new TimeSignature(2, 8);
            var timeSignature2 = new TimeSignature(2, 8);
            ClassicAssert.IsFalse(timeSignature1 < timeSignature2, "First time signature less than second one.");
        }

        [Test]
        public void Less_FirstLess()
        {
            var timeSignature1 = new TimeSignature(1, 16);
            var timeSignature2 = new TimeSignature(2, 8);
            ClassicAssert.IsTrue(timeSignature1 < timeSignature2, "First time signature doesn't less than second one.");
        }

        [Test]
        public void Less_SecondLess()
        {
            var timeSignature1 = new TimeSignature(2, 8);
            var timeSignature2 = new TimeSignature(1, 16);
            ClassicAssert.IsFalse(timeSignature1 < timeSignature2, "First time signature less than second one.");
        }

        [Test]
        public void LessOrEqual_FirstNull()
        {
            var timeSignature1 = default(TimeSignature);
            var timeSignature2 = new TimeSignature(2, 8);
            ClassicAssert.Throws<ArgumentNullException>(() => { var x = timeSignature1 <= timeSignature2; }, "Exception not thrown for null time signature.");
        }

        [Test]
        public void LessOrEqual_SecondNull()
        {
            var timeSignature1 = new TimeSignature(2, 8);
            var timeSignature2 = default(TimeSignature);
            ClassicAssert.Throws<ArgumentNullException>(() => { var x = timeSignature1 <= timeSignature2; }, "Exception not thrown for null time signature.");
        }

        [Test]
        public void LessOrEqual_BothNull()
        {
            var timeSignature1 = default(TimeSignature);
            var timeSignature2 = default(TimeSignature);
            ClassicAssert.Throws<ArgumentNullException>(() => { var x = timeSignature1 <= timeSignature2; }, "Exception not thrown for null time signature.");
        }

        [Test]
        public void LessOrEqual_SameValues()
        {
            var timeSignature1 = new TimeSignature(2, 8);
            var timeSignature2 = new TimeSignature(2, 8);
            ClassicAssert.IsTrue(timeSignature1 <= timeSignature2, "First time signature doesn't less than or equal to second one.");
        }

        [Test]
        public void LessOrEqual_FirstLess()
        {
            var timeSignature1 = new TimeSignature(1, 16);
            var timeSignature2 = new TimeSignature(2, 8);
            ClassicAssert.IsTrue(timeSignature1 <= timeSignature2, "First time signature doesn't less than or equals to second one.");
        }

        [Test]
        public void LessOrEqual_SecondLess()
        {
            var timeSignature1 = new TimeSignature(2, 8);
            var timeSignature2 = new TimeSignature(1, 16);
            ClassicAssert.IsFalse(timeSignature1 <= timeSignature2, "First time signature less than or equals to second one.");
        }

        [Test]
        public void Greater_FirstNull()
        {
            var timeSignature1 = default(TimeSignature);
            var timeSignature2 = new TimeSignature(2, 8);
            ClassicAssert.Throws<ArgumentNullException>(() => { var x = timeSignature1 > timeSignature2; }, "Exception not thrown for null time signature.");
        }

        [Test]
        public void Greater_SecondNull()
        {
            var timeSignature1 = new TimeSignature(2, 8);
            var timeSignature2 = default(TimeSignature);
            ClassicAssert.Throws<ArgumentNullException>(() => { var x = timeSignature1 > timeSignature2; }, "Exception not thrown for null time signature.");
        }

        [Test]
        public void Greater_BothNull()
        {
            var timeSignature1 = default(TimeSignature);
            var timeSignature2 = default(TimeSignature);
            ClassicAssert.Throws<ArgumentNullException>(() => { var x = timeSignature1 > timeSignature2; }, "Exception not thrown for null time signature.");
        }

        [Test]
        public void Greater_SameValues()
        {
            var timeSignature1 = new TimeSignature(2, 8);
            var timeSignature2 = new TimeSignature(2, 8);
            ClassicAssert.IsFalse(timeSignature1 > timeSignature2, "First time signature greater than second one.");
        }

        [Test]
        public void Greater_FirstGreater()
        {
            var timeSignature1 = new TimeSignature(2, 8);
            var timeSignature2 = new TimeSignature(1, 16);
            ClassicAssert.IsTrue(timeSignature1 > timeSignature2, "First time signature doesn't greater than second one.");
        }

        [Test]
        public void Greater_SecondGreater()
        {
            var timeSignature1 = new TimeSignature(1, 16);
            var timeSignature2 = new TimeSignature(2, 8);
            ClassicAssert.IsFalse(timeSignature1 > timeSignature2, "First time signature greater than second one.");
        }

        [Test]
        public void GreaterOrEqual_FirstNull()
        {
            var timeSignature1 = default(TimeSignature);
            var timeSignature2 = new TimeSignature(2, 8);
            ClassicAssert.Throws<ArgumentNullException>(() => { var x = timeSignature1 >= timeSignature2; }, "Exception not thrown for null time signature.");
        }

        [Test]
        public void GreaterOrEqual_SecondNull()
        {
            var timeSignature1 = new TimeSignature(2, 8);
            var timeSignature2 = default(TimeSignature);
            ClassicAssert.Throws<ArgumentNullException>(() => { var x = timeSignature1 >= timeSignature2; }, "Exception not thrown for null time signature.");
        }

        [Test]
        public void GreaterOrEqual_BothNull()
        {
            var timeSignature1 = default(TimeSignature);
            var timeSignature2 = default(TimeSignature);
            ClassicAssert.Throws<ArgumentNullException>(() => { var x = timeSignature1 >= timeSignature2; }, "Exception not thrown for null time signature.");
        }

        [Test]
        public void GreaterOrEqual_SameValues()
        {
            var timeSignature1 = new TimeSignature(2, 8);
            var timeSignature2 = new TimeSignature(2, 8);
            ClassicAssert.IsTrue(timeSignature1 >= timeSignature2, "First time signature doesn't greater than or equal to second one.");
        }

        [Test]
        public void GreaterOrEqual_FirstGreater()
        {
            var timeSignature1 = new TimeSignature(2, 8);
            var timeSignature2 = new TimeSignature(1, 16);
            ClassicAssert.IsTrue(timeSignature1 >= timeSignature2, "First time signature doesn't greater than or equals to second one.");
        }

        [Test]
        public void GreaterOrEqual_SecondGreater()
        {
            var timeSignature1 = new TimeSignature(1, 16);
            var timeSignature2 = new TimeSignature(2, 8);
            ClassicAssert.IsFalse(timeSignature1 >= timeSignature2, "First time signature greater than or equals to second one.");
        }

        #endregion
    }
}
