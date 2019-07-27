using System;
using Melanchall.DryWetMidi.Common;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Common
{
    [TestFixture]
    public sealed class SevenBitNumberTests
    {
        #region Test methods

        [Test]
        public void Parse_InvalidFormat()
        {
            Assert.Throws<FormatException>(() => SevenBitNumber.Parse("sdsd"));
        }

        [Test]
        public void Parse_OutOfRange()
        {
            Assert.Throws<FormatException>(() => SevenBitNumber.Parse("200"));
            Assert.Throws<FormatException>(() => SevenBitNumber.Parse("128"));
        }

        [Test]
        public void Parse()
        {
            Assert.AreEqual((SevenBitNumber)12, SevenBitNumber.Parse("12"));
            Assert.AreEqual((SevenBitNumber)0, SevenBitNumber.Parse("0"));
            Assert.AreEqual((SevenBitNumber)127, SevenBitNumber.Parse("127"));
        }

        [Test]
        public void TryParse_InvalidFormat()
        {
            Assert.AreEqual(false, SevenBitNumber.TryParse("sdsd", out _));
        }

        [Test]
        public void TryParse_OutOfRange()
        {
            Assert.AreEqual(false, SevenBitNumber.TryParse("200", out _));
            Assert.AreEqual(false, SevenBitNumber.TryParse("128", out _));
        }

        [Test]
        public void TryParse()
        {
            SevenBitNumber result;

            SevenBitNumber.TryParse("12", out result);
            Assert.AreEqual((SevenBitNumber)12, result);

            SevenBitNumber.TryParse("0", out result);
            Assert.AreEqual((SevenBitNumber)0, result);

            SevenBitNumber.TryParse("127", out result);
            Assert.AreEqual((SevenBitNumber)127, result);
        }

        #endregion
    }
}
