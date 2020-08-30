using System;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class ChannelCoarseTuningParameterTests : RegisteredParameterTests<ChannelCoarseTuningParameter>
    {
        #region Test methods

        [Test]
        public void CheckDefaultData()
        {
            var parameter = GetDefaultParameter();
            Assert.AreEqual(0, parameter.HalfSteps, "Default half-steps number is invalid.");
            
            CheckTimedEvents(
                parameter,
                GetExpectedRpnSequence(0x40));
        }

        [TestCase(-64, 0x00)]
        [TestCase(0, 0x40)]
        [TestCase(63, 0x7F)]
        public void CheckData_FromConstructor(sbyte halfSteps, byte expectedDataByte)
        {
            var parameter = GetNonDefaultParameter(halfSteps);
            CheckTimedEvents(
                parameter,
                GetExpectedRpnSequence(expectedDataByte));
        }

        [TestCase(-64, 0x00)]
        [TestCase(0, 0x40)]
        [TestCase(63, 0x7F)]
        public void CheckData_FromProperty(sbyte halfSteps, byte expectedDataByte)
        {
            var parameter = GetDefaultParameter();
            parameter.HalfSteps = halfSteps;

            CheckTimedEvents(
                parameter,
                GetExpectedRpnSequence(expectedDataByte));
        }

        [TestCase(-65)]
        [TestCase(64)]
        public void CheckOutOfRangeData(sbyte halfSteps)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => GetNonDefaultParameter(halfSteps),
                "Exception wasn't thrown from constructor.");

            var parameter = GetDefaultParameter();
            Assert.Throws<ArgumentOutOfRangeException>(
                () => parameter.HalfSteps = halfSteps,
                "Exception wasn't thrown from property.");
        }

        #endregion

        #region Private methods

        private (byte, byte)[] GetExpectedRpnSequence(byte expectedDataByte) =>
            new (byte, byte)[]
            {
                (101, 0x00),
                (100, 0x02),
                (006, expectedDataByte),
                (101, 0x7F),
                (100, 0x7F)
            };

        private ChannelCoarseTuningParameter GetNonDefaultParameter(sbyte halfSteps) =>
            GetParameter(() => new ChannelCoarseTuningParameter(halfSteps));

        #endregion
    }
}
