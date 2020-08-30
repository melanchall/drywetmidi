using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class ModulationDepthRangeParameterTests : RegisteredParameterTests<ModulationDepthRangeParameter>
    {
        #region Test methods

        [Test]
        public void CheckDefaultData()
        {
            var parameter = GetDefaultParameter();
            Assert.AreEqual((SevenBitNumber)0, parameter.HalfSteps, "Default half-steps number is invalid.");
            Assert.AreEqual(50, parameter.Cents, "Default cents number is invalid.");

            CheckTimedEvents(
                parameter,
                GetExpectedRpnSequence(0x00, 0x40));
        }

        [TestCase(0, 0, 0x00, 0x00)]
        [TestCase(127, 0, 0x7F, 0x00)]
        [TestCase(0, 6.25f, 0x00, 0x08)]
        [TestCase(0, 100, 0x00, 0x7F)]
        [TestCase(127, 100, 0x7F, 0x7F)]
        public void CheckData_FromConstructor(byte halfSteps, float cents, byte expectedDataMsb, byte expectedDataLsb)
        {
            var parameter = GetNonDefaultParameter((SevenBitNumber)halfSteps, cents);
            CheckTimedEvents(
                parameter,
                GetExpectedRpnSequence(expectedDataMsb, expectedDataLsb));
        }

        [TestCase(0, 0, 0x00, 0x00)]
        [TestCase(127, 0, 0x7F, 0x00)]
        [TestCase(0, 6.25f, 0x00, 0x08)]
        [TestCase(0, 100, 0x00, 0x7F)]
        [TestCase(127, 100, 0x7F, 0x7F)]
        public void CheckData_FromProperty(byte halfSteps, float cents, byte expectedDataMsb, byte expectedDataLsb)
        {
            var parameter = GetDefaultParameter();
            parameter.HalfSteps = (SevenBitNumber)halfSteps;
            parameter.Cents = cents;

            CheckTimedEvents(
                parameter,
                GetExpectedRpnSequence(expectedDataMsb, expectedDataLsb));
        }

        [TestCase(0, -101f)]
        [TestCase(127, -100.01f)]
        [TestCase(10, 101f)]
        [TestCase(1, 100.001f)]
        public void CheckOutOfRangeData(byte halfSteps, float cents)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => GetNonDefaultParameter((SevenBitNumber)halfSteps, cents),
                "Exception wasn't thrown from constructor.");

            var parameter = GetDefaultParameter();
            Assert.Throws<ArgumentOutOfRangeException>(
                () => parameter.Cents = cents,
                "Exception wasn't thrown from property.");
        }

        #endregion

        #region Private methods

        private (byte, byte)[] GetExpectedRpnSequence(byte expectedDataMsb, byte expectedDataLsb) =>
            new (byte, byte)[]
            {
                (101, 0x00),
                (100, 0x05),
                (006, expectedDataMsb),
                (038, expectedDataLsb),
                (101, 0x7F),
                (100, 0x7F)
            };

        private ModulationDepthRangeParameter GetNonDefaultParameter(SevenBitNumber halfSteps, float cents) =>
            GetParameter(() => new ModulationDepthRangeParameter(halfSteps, cents));

        #endregion
    }
}
