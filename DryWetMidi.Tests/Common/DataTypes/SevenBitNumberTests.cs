using System;
using Melanchall.DryWetMidi.Common;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Common
{
    [TestFixture]
    public sealed class SevenBitNumberTests
    {
        #region Test methods

        [Test]
        public void Parse_InvalidFormat()
        {
            ClassicAssert.Throws<FormatException>(() => SevenBitNumber.Parse("sdsd"));
        }

        [Test]
        public void Parse_OutOfRange()
        {
            ClassicAssert.Throws<FormatException>(() => SevenBitNumber.Parse("200"));
            ClassicAssert.Throws<FormatException>(() => SevenBitNumber.Parse("128"));
        }

        [Test]
        public void Parse()
        {
            ClassicAssert.AreEqual((SevenBitNumber)12, SevenBitNumber.Parse("12"));
            ClassicAssert.AreEqual((SevenBitNumber)0, SevenBitNumber.Parse("0"));
            ClassicAssert.AreEqual((SevenBitNumber)127, SevenBitNumber.Parse("127"));
        }

        [Test]
        public void TryParse_InvalidFormat()
        {
            ClassicAssert.AreEqual(false, SevenBitNumber.TryParse("sdsd", out _));
        }

        [Test]
        public void TryParse_OutOfRange()
        {
            ClassicAssert.AreEqual(false, SevenBitNumber.TryParse("200", out _));
            ClassicAssert.AreEqual(false, SevenBitNumber.TryParse("128", out _));
        }

        [Test]
        public void TryParse()
        {
            SevenBitNumber result;

            SevenBitNumber.TryParse("12", out result);
            ClassicAssert.AreEqual((SevenBitNumber)12, result);

            SevenBitNumber.TryParse("0", out result);
            ClassicAssert.AreEqual((SevenBitNumber)0, result);

            SevenBitNumber.TryParse("127", out result);
            ClassicAssert.AreEqual((SevenBitNumber)127, result);
        }

        #endregion
    }
}
