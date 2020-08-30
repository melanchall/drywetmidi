using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class TuningBankSelectParameterTests : RegisteredParameterTests<TuningBankSelectParameter>
    {
        #region Test methods

        [Test]
        public void CheckDefaultData()
        {
            var parameter = GetDefaultParameter();
            Assert.AreEqual((SevenBitNumber)0, parameter.BankNumber, "Default bank number is invalid.");

            CheckTimedEvents(
                parameter,
                GetExpectedRpnSequence(0x00));
        }

        [TestCase(0, 0x00)]
        [TestCase(127, 0x7F)]
        public void CheckData_FromConstructor(byte bankNumber, byte expectedDataByte)
        {
            var parameter = GetNonDefaultParameter((SevenBitNumber)bankNumber);
            CheckTimedEvents(
                parameter,
                GetExpectedRpnSequence(expectedDataByte));
        }

        [TestCase(0, 0x00)]
        [TestCase(127, 0x7F)]
        public void CheckData_FromProperty(byte bankNumber, byte expectedDataByte)
        {
            var parameter = GetDefaultParameter();
            parameter.BankNumber = (SevenBitNumber)bankNumber;

            CheckTimedEvents(
                parameter,
                GetExpectedRpnSequence(expectedDataByte));
        }

        #endregion

        #region Private methods

        private (byte, byte)[] GetExpectedRpnSequence(byte expectedDataByte) =>
            new (byte, byte)[]
            {
                (101, 0x00),
                (100, 0x04),
                (006, expectedDataByte),
                (101, 0x7F),
                (100, 0x7F)
            };

        private TuningBankSelectParameter GetNonDefaultParameter(SevenBitNumber bankNumber) =>
            GetParameter(() => new TuningBankSelectParameter(bankNumber));

        #endregion
    }
}
