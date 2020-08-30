using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class TuningProgramChangeParameterTests : RegisteredParameterTests<TuningProgramChangeParameter>
    {
        #region Test methods

        [Test]
        public void CheckDefaultData()
        {
            var parameter = GetDefaultParameter();
            Assert.AreEqual((SevenBitNumber)0, parameter.ProgramNumber, "Default program number is invalid.");

            CheckTimedEvents(
                parameter,
                GetExpectedRpnSequence(0x00));
        }

        [TestCase(0, 0x00)]
        [TestCase(127, 0x7F)]
        public void CheckData_FromConstructor(byte programNumber, byte expectedDataByte)
        {
            var parameter = GetNonDefaultParameter((SevenBitNumber)programNumber);
            CheckTimedEvents(
                parameter,
                GetExpectedRpnSequence(expectedDataByte));
        }

        [TestCase(0, 0x00)]
        [TestCase(127, 0x7F)]
        public void CheckData_FromProperty(byte programNumber, byte expectedDataByte)
        {
            var parameter = GetDefaultParameter();
            parameter.ProgramNumber = (SevenBitNumber)programNumber;

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
                (100, 0x03),
                (006, expectedDataByte),
                (101, 0x7F),
                (100, 0x7F)
            };

        private TuningProgramChangeParameter GetNonDefaultParameter(SevenBitNumber programNumber) =>
            GetParameter(() => new TuningProgramChangeParameter(programNumber));

        #endregion
    }
}
