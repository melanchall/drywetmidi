using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class PitchBendSensitivityParameterTests : RegisteredParameterTests<PitchBendSensitivityParameter>
    {
        #region Test methods

        [Test]
        public void CheckDefaultData()
        {
            var parameter = GetDefaultParameter();
            Assert.AreEqual((SevenBitNumber)2, parameter.HalfSteps, "Default half-steps number is invalid.");
            Assert.AreEqual((SevenBitNumber)0, parameter.Cents, "Default cents number is invalid.");
            
            CheckTimedEvents(
                parameter,
                GetExpectedRpnSequence(0x02, 0x00));
        }

        [TestCase(0, 0, 0x00, 0x00)]
        [TestCase(0, 127, 0x00, 0x7F)]
        [TestCase(127, 0, 0x7F, 0x00)]
        [TestCase(127, 127, 0x7F, 0x7F)]
        public void CheckData_FromConstructor(byte halfSteps, byte cents, byte expectedDataMsb, byte expectedDataLsb)
        {
            var parameter = GetNonDefaultParameter((SevenBitNumber)halfSteps, (SevenBitNumber)cents);
            CheckTimedEvents(
                parameter,
                GetExpectedRpnSequence(expectedDataMsb, expectedDataLsb));
        }

        [TestCase(0, 0, 0x00, 0x00)]
        [TestCase(0, 127, 0x00, 0x7F)]
        [TestCase(127, 0, 0x7F, 0x00)]
        [TestCase(127, 127, 0x7F, 0x7F)]
        public void CheckData_FromProperties(byte halfSteps, byte cents, byte expectedDataMsb, byte expectedDataLsb)
        {
            var parameter = GetDefaultParameter();
            parameter.HalfSteps = (SevenBitNumber)halfSteps;
            parameter.Cents = (SevenBitNumber)cents;

            CheckTimedEvents(
                parameter,
                GetExpectedRpnSequence(expectedDataMsb, expectedDataLsb));
        }

        #endregion

        #region Private methods

        private (byte, byte)[] GetExpectedRpnSequence(byte expectedDataMsb, byte expectedDataLsb) =>
            new (byte, byte)[]
            {
                (101, 0x00),
                (100, 0x00),
                (006, expectedDataMsb),
                (038, expectedDataLsb),
                (101, 0x7F),
                (100, 0x7F)
            };

        private PitchBendSensitivityParameter GetNonDefaultParameter(SevenBitNumber halfSteps, SevenBitNumber cents) =>
            GetParameter(() => new PitchBendSensitivityParameter(halfSteps, cents));

        #endregion
    }
}
