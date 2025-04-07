using System;
using Melanchall.DryWetMidi.Common;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Common
{
    [TestFixture]
    public sealed class FourBitNumberTests
    {
        #region Test methods

        [Test]
        public void Parse_InvalidFormat()
        {
            ClassicAssert.Throws<FormatException>(() => FourBitNumber.Parse("sdsd"));
        }

        [Test]
        public void Parse_OutOfRange()
        {
            ClassicAssert.Throws<FormatException>(() => FourBitNumber.Parse("200"));
            ClassicAssert.Throws<FormatException>(() => FourBitNumber.Parse("16"));
        }

        [Test]
        public void Parse()
        {
            ClassicAssert.AreEqual((FourBitNumber)12, FourBitNumber.Parse("12"));
            ClassicAssert.AreEqual((FourBitNumber)0, FourBitNumber.Parse("0"));
            ClassicAssert.AreEqual((FourBitNumber)15, FourBitNumber.Parse("15"));
        }

        [Test]
        public void TryParse_InvalidFormat()
        {
            ClassicAssert.AreEqual(false, FourBitNumber.TryParse("sdsd", out _));
        }

        [Test]
        public void TryParse_OutOfRange()
        {
            ClassicAssert.AreEqual(false, FourBitNumber.TryParse("200", out _));
            ClassicAssert.AreEqual(false, FourBitNumber.TryParse("16", out _));
        }

        [Test]
        public void TryParse()
        {
            FourBitNumber result;

            FourBitNumber.TryParse("12", out result);
            ClassicAssert.AreEqual((FourBitNumber)12, result);

            FourBitNumber.TryParse("0", out result);
            ClassicAssert.AreEqual((FourBitNumber)0, result);

            FourBitNumber.TryParse("15", out result);
            ClassicAssert.AreEqual((FourBitNumber)15, result);
        }

        #endregion
    }
}
