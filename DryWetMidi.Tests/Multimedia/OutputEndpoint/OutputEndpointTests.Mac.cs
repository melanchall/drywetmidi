using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Attributes;
using NUnit.Framework;
using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class OutputEndpointTests
    {
        #region Test methods

        [Test]
        [MacOnly]
        public void SendEvent_SysEx_LargerThanUInt16_Mac()
        {
            var data = Enumerable
                .Repeat((byte)0x01, 70000)
                .Concat(new byte[] { 0xF7 })
                .ToArray();

            SendEvents(
                new[] { new NormalSysExEvent(data) },
                eventSendReceiveDelay: TimeSpan.FromSeconds(10));
        }

        #endregion
    }
}
