using System;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class ChannelFineTuningParameterTests : RegisteredParameterTests<ChannelFineTuningParameter>
    {
        #region Test methods

        [Test]
        public void CheckDefaultData()
        {
            var parameter = GetDefaultParameter();
            Assert.AreEqual(0, parameter.Cents, "Default cents number is invalid.");

            CheckTimedEvents(
                parameter,
                GetExpectedRpnSequence(0x40, 0x00));
        }

        [TestCase(-100, 0x00, 0x00)]
        [TestCase(0, 0x40, 0x00)]
        [TestCase(100, 0x7F, 0x7F)]
        public void CheckData_FromConstructor(float cents, byte expectedDataMsb, byte expectedDataLsb)
        {
            var parameter = GetNonDefaultParameter(cents);
            CheckTimedEvents(
                parameter,
                GetExpectedRpnSequence(expectedDataMsb, expectedDataLsb));
        }

        [TestCase(-100, 0x00, 0x00)]
        [TestCase(0, 0x40, 0x00)]
        [TestCase(100, 0x7F, 0x7F)]
        public void CheckData_FromProperty(float cents, byte expectedDataMsb, byte expectedDataLsb)
        {
            var parameter = GetDefaultParameter();
            parameter.Cents = cents;

            CheckTimedEvents(
                parameter,
                GetExpectedRpnSequence(expectedDataMsb, expectedDataLsb));
        }

        [TestCase(-101f)]
        [TestCase(-100.01f)]
        [TestCase(101f)]
        [TestCase(100.001f)]
        public void CheckOutOfRangeData(float cents)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => GetNonDefaultParameter(cents),
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
                (100, 0x01),
                (006, expectedDataMsb),
                (038, expectedDataLsb),
                (101, 0x7F),
                (100, 0x7F)
            };

        private ChannelFineTuningParameter GetNonDefaultParameter(float cents) =>
            GetParameter(() => new ChannelFineTuningParameter(cents));

        #endregion
    }
}
