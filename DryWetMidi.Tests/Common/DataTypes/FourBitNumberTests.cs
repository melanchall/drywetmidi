using System;
using Melanchall.DryWetMidi.Common;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Common
{
    [TestFixture]
    public sealed class FourBitNumberTests
    {
        #region Test methods

        [Test]
        public void Parse_InvalidFormat()
        {
            Assert.Throws<FormatException>(() => FourBitNumber.Parse("sdsd"));
        }

        [Test]
        public void Parse_OutOfRange()
        {
            Assert.Throws<FormatException>(() => FourBitNumber.Parse("200"));
            Assert.Throws<FormatException>(() => FourBitNumber.Parse("16"));
        }

        [Test]
        public void Parse()
        {
            Assert.AreEqual((FourBitNumber)12, FourBitNumber.Parse("12"));
            Assert.AreEqual((FourBitNumber)0, FourBitNumber.Parse("0"));
            Assert.AreEqual((FourBitNumber)15, FourBitNumber.Parse("15"));
        }

        [Test]
        public void TryParse_InvalidFormat()
        {
            FourBitNumber result;
            Assert.AreEqual(false, FourBitNumber.TryParse("sdsd", out result));
        }

        [Test]
        public void TryParse_OutOfRange()
        {
            FourBitNumber result;
            Assert.AreEqual(false, FourBitNumber.TryParse("200", out result));
            Assert.AreEqual(false, FourBitNumber.TryParse("16", out result));
        }

        [Test]
        public void TryParse()
        {
            FourBitNumber result;

            FourBitNumber.TryParse("12", out result);
            Assert.AreEqual((FourBitNumber)12, result);

            FourBitNumber.TryParse("0", out result);
            Assert.AreEqual((FourBitNumber)0, result);

            FourBitNumber.TryParse("15", out result);
            Assert.AreEqual((FourBitNumber)15, result);
        }

        #endregion
    }
}
