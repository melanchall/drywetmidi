using System;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class TempoTests
    {
        #region Test methods

        [Test]
        public void CheckEquality_FirstNull()
        {
            var tempo1 = default(Tempo);
            var tempo2 = new Tempo(100);
            Assert.IsFalse(tempo1 == tempo2, "First tempo equals to second one.");
        }

        [Test]
        public void CheckEquality_SecondNull()
        {
            var tempo1 = new Tempo(100);
            var tempo2 = default(Tempo);
            Assert.IsFalse(tempo1 == tempo2, "First tempo equals to second one.");
        }

        [Test]
        public void CheckEquality_BothNull()
        {
            var tempo1 = default(Tempo);
            var tempo2 = default(Tempo);
            Assert.IsTrue(tempo1 == tempo2, "First tempo doesn't equal to second one.");
        }

        [Test]
        public void CheckEquality_SameValues()
        {
            var tempo1 = new Tempo(200);
            var tempo2 = new Tempo(200);
            Assert.IsTrue(tempo1 == tempo2, "First tempo doesn't equal to second one.");
        }

        [Test]
        public void CheckEquality_DifferentValues()
        {
            var tempo1 = new Tempo(200);
            var tempo2 = new Tempo(100);
            Assert.IsFalse(tempo1 == tempo2, "First tempo equals to second one.");
        }

        [Test]
        public void CheckInequality_FirstNull()
        {
            var tempo1 = default(Tempo);
            var tempo2 = new Tempo(100);
            Assert.IsTrue(tempo1 != tempo2, "First tempo equals to second one.");
        }

        [Test]
        public void CheckInequality_SecondNull()
        {
            var tempo1 = new Tempo(100);
            var tempo2 = default(Tempo);
            Assert.IsTrue(tempo1 != tempo2, "First tempo equals to second one.");
        }

        [Test]
        public void CheckInequality_BothNull()
        {
            var tempo1 = default(Tempo);
            var tempo2 = default(Tempo);
            Assert.IsFalse(tempo1 != tempo2, "First tempo doesn't equal to second one.");
        }

        [Test]
        public void CheckInequality_SameValues()
        {
            var tempo1 = new Tempo(200);
            var tempo2 = new Tempo(200);
            Assert.IsFalse(tempo1 != tempo2, "First tempo doesn't equal to second one.");
        }

        [Test]
        public void CheckInequality_DifferentValues()
        {
            var tempo1 = new Tempo(200);
            var tempo2 = new Tempo(100);
            Assert.IsTrue(tempo1 != tempo2, "First tempo equals to second one.");
        }

        [Test]
        public void Less_FirstNull()
        {
            var tempo1 = default(Tempo);
            var tempo2 = new Tempo(100);
            Assert.Throws<ArgumentNullException>(() => { var x = tempo1 < tempo2; }, "Exception not thrown for null tempo.");
        }

        [Test]
        public void Less_SecondNull()
        {
            var tempo1 = new Tempo(100);
            var tempo2 = default(Tempo);
            Assert.Throws<ArgumentNullException>(() => { var x = tempo1 < tempo2; }, "Exception not thrown for null tempo.");
        }

        [Test]
        public void Less_BothNull()
        {
            var tempo1 = default(Tempo);
            var tempo2 = default(Tempo);
            Assert.Throws<ArgumentNullException>(() => { var x = tempo1 < tempo2; }, "Exception not thrown for null tempo.");
        }

        [Test]
        public void Less_SameValues()
        {
            var tempo1 = new Tempo(200);
            var tempo2 = new Tempo(200);
            Assert.IsFalse(tempo1 < tempo2, "First tempo less than second one.");
        }

        [Test]
        public void Less_FirstLess()
        {
            var tempo1 = new Tempo(100);
            var tempo2 = new Tempo(200);
            Assert.IsTrue(tempo1 < tempo2, "First tempo doesn't less than second one.");
        }

        [Test]
        public void Less_SecondLess()
        {
            var tempo1 = new Tempo(200);
            var tempo2 = new Tempo(100);
            Assert.IsFalse(tempo1 < tempo2, "First tempo less than second one.");
        }

        [Test]
        public void LessOrEqual_FirstNull()
        {
            var tempo1 = default(Tempo);
            var tempo2 = new Tempo(100);
            Assert.Throws<ArgumentNullException>(() => { var x = tempo1 <= tempo2; }, "Exception not thrown for null tempo.");
        }

        [Test]
        public void LessOrEqual_SecondNull()
        {
            var tempo1 = new Tempo(100);
            var tempo2 = default(Tempo);
            Assert.Throws<ArgumentNullException>(() => { var x = tempo1 <= tempo2; }, "Exception not thrown for null tempo.");
        }

        [Test]
        public void LessOrEqual_BothNull()
        {
            var tempo1 = default(Tempo);
            var tempo2 = default(Tempo);
            Assert.Throws<ArgumentNullException>(() => { var x = tempo1 <= tempo2; }, "Exception not thrown for null tempo.");
        }

        [Test]
        public void LessOrEqual_SameValues()
        {
            var tempo1 = new Tempo(200);
            var tempo2 = new Tempo(200);
            Assert.IsTrue(tempo1 <= tempo2, "First tempo doesn't less than or equal to second one.");
        }

        [Test]
        public void LessOrEqual_FirstLess()
        {
            var tempo1 = new Tempo(100);
            var tempo2 = new Tempo(200);
            Assert.IsTrue(tempo1 <= tempo2, "First tempo doesn't less than or equals to second one.");
        }

        [Test]
        public void LessOrEqual_SecondLess()
        {
            var tempo1 = new Tempo(200);
            var tempo2 = new Tempo(100);
            Assert.IsFalse(tempo1 <= tempo2, "First tempo less than or equals to second one.");
        }

        [Test]
        public void Greater_FirstNull()
        {
            var tempo1 = default(Tempo);
            var tempo2 = new Tempo(100);
            Assert.Throws<ArgumentNullException>(() => { var x = tempo1 > tempo2; }, "Exception not thrown for null tempo.");
        }

        [Test]
        public void Greater_SecondNull()
        {
            var tempo1 = new Tempo(100);
            var tempo2 = default(Tempo);
            Assert.Throws<ArgumentNullException>(() => { var x = tempo1 > tempo2; }, "Exception not thrown for null tempo.");
        }

        [Test]
        public void Greater_BothNull()
        {
            var tempo1 = default(Tempo);
            var tempo2 = default(Tempo);
            Assert.Throws<ArgumentNullException>(() => { var x = tempo1 > tempo2; }, "Exception not thrown for null tempo.");
        }

        [Test]
        public void Greater_SameValues()
        {
            var tempo1 = new Tempo(200);
            var tempo2 = new Tempo(200);
            Assert.IsFalse(tempo1 > tempo2, "First tempo greater than second one.");
        }

        [Test]
        public void Greater_FirstGreater()
        {
            var tempo1 = new Tempo(200);
            var tempo2 = new Tempo(100);
            Assert.IsTrue(tempo1 > tempo2, "First tempo doesn't greater than second one.");
        }

        [Test]
        public void Greater_SecondGreater()
        {
            var tempo1 = new Tempo(100);
            var tempo2 = new Tempo(200);
            Assert.IsFalse(tempo1 > tempo2, "First tempo greater than second one.");
        }

        [Test]
        public void GreaterOrEqual_FirstNull()
        {
            var tempo1 = default(Tempo);
            var tempo2 = new Tempo(100);
            Assert.Throws<ArgumentNullException>(() => { var x = tempo1 >= tempo2; }, "Exception not thrown for null tempo.");
        }

        [Test]
        public void GreaterOrEqual_SecondNull()
        {
            var tempo1 = new Tempo(100);
            var tempo2 = default(Tempo);
            Assert.Throws<ArgumentNullException>(() => { var x = tempo1 >= tempo2; }, "Exception not thrown for null tempo.");
        }

        [Test]
        public void GreaterOrEqual_BothNull()
        {
            var tempo1 = default(Tempo);
            var tempo2 = default(Tempo);
            Assert.Throws<ArgumentNullException>(() => { var x = tempo1 >= tempo2; }, "Exception not thrown for null tempo.");
        }

        [Test]
        public void GreaterOrEqual_SameValues()
        {
            var tempo1 = new Tempo(200);
            var tempo2 = new Tempo(200);
            Assert.IsTrue(tempo1 >= tempo2, "First tempo doesn't greater than or equal to second one.");
        }

        [Test]
        public void GreaterOrEqual_FirstGreater()
        {
            var tempo1 = new Tempo(200);
            var tempo2 = new Tempo(100);
            Assert.IsTrue(tempo1 >= tempo2, "First tempo doesn't greater than or equals to second one.");
        }

        [Test]
        public void GreaterOrEqual_SecondGreater()
        {
            var tempo1 = new Tempo(100);
            var tempo2 = new Tempo(200);
            Assert.IsFalse(tempo1 >= tempo2, "First tempo greater than or equals to second one.");
        }

        #endregion
    }
}
