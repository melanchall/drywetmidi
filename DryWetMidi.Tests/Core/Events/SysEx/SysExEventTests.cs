using System;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed class SysExEventTests
    {
        #region Test methods

        [Test]
        public void CreateNormalSysEx_NotCompleted()
        {
            var data = new byte[] { 0x67, 0x45 };
            var sysExEvent = new NormalSysExEvent(data);
            CollectionAssert.AreEqual(data, sysExEvent.Data, "Data is invalid.");
            Assert.IsFalse(sysExEvent.Completed, "'Completed' value is invalid.");
        }

        [Test]
        public void CreateNormalSysEx_Completed()
        {
            var data = new byte[] { 0x67, 0x45, 0xF7 };
            var sysExEvent = new NormalSysExEvent(data);
            CollectionAssert.AreEqual(data, sysExEvent.Data, "Data is invalid.");
            Assert.IsTrue(sysExEvent.Completed, "'Completed' value is invalid.");
        }

        [Test]
        public void CreateNormalSysEx_StartedWithF0()
        {
            Assert.Throws<ArgumentException>(() => new NormalSysExEvent(new byte[] { 0xF0, 0x67 }));
        }

        [Test]
        public void CreateEscapeSysEx_NotCompleted()
        {
            var data = new byte[] { 0x67, 0x45 };
            var sysExEvent = new EscapeSysExEvent(data);
            CollectionAssert.AreEqual(data, sysExEvent.Data, "Data is invalid.");
            Assert.IsFalse(sysExEvent.Completed, "'Completed' value is invalid.");
        }

        [Test]
        public void CreateEscapeSysEx_Completed()
        {
            var data = new byte[] { 0x67, 0x45, 0xF7 };
            var sysExEvent = new EscapeSysExEvent(data);
            CollectionAssert.AreEqual(data, sysExEvent.Data, "Data is invalid.");
            Assert.IsTrue(sysExEvent.Completed, "'Completed' value is invalid.");
        }

        [Test]
        public void CreateEscapeSysEx_StartedWithF7()
        {
            Assert.Throws<ArgumentException>(() => new EscapeSysExEvent(new byte[] { 0xF7, 0x67 }));
        }

        #endregion
    }
}
