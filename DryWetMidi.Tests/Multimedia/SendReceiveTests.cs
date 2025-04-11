using System;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed class SendReceiveTests
    {
        #region Constants

        private const int RetriesNumber = 3;

        #endregion

        #region Test methods

        [Retry(RetriesNumber)]
        [Test]
        public void CheckEventsReceiving()
        {
            using (var outputDevice = OutputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
            using (var inputDevice = InputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
            {
                inputDevice.StartEventsListening();

                SendReceiveUtilities.CheckEventsReceiving(
                    new[]
                    {
                        new EventToSend2(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }, TimeSpan.Zero),
                        new EventToSend2(new NormalSysExEvent(new byte[] { 1, 2, 3, 0xF7 }), TimeSpan.FromSeconds(1)),
                        new EventToSend2(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 }, TimeSpan.FromSeconds(3)),
                        new EventToSend2(new NormalSysExEvent(new byte[] { 4, 5, 6, 0xF7 }), TimeSpan.FromSeconds(5)),
                        new EventToSend2(new SongSelectEvent((SevenBitNumber)20), TimeSpan.FromSeconds(5)),
                        new EventToSend2(new TuneRequestEvent(), TimeSpan.FromMilliseconds(5200)),
                    },
                    outputDevice,
                    inputDevice);

                inputDevice.StopEventsListening();
            }
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckEventsReceiving_AllEventTypes_ExceptSysEx()
        {
            var events = TypesProvider.GetAllEventTypes()
                .Where(t => !typeof(MetaEvent).IsAssignableFrom(t) && !typeof(SysExEvent).IsAssignableFrom(t))
                .Select(t => (MidiEvent)Activator.CreateInstance(t))
                .ToArray();

            CollectionAssert.IsNotEmpty(events, "Events collection is empty.");

            using (var outputDevice = OutputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
            using (var inputDevice = InputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
            {
                inputDevice.StartEventsListening();

                SendReceiveUtilities.CheckEventsReceiving(
                    events.Select((e, i) => new EventToSend2(e, TimeSpan.FromMilliseconds(50).MultiplyBy(i))).ToArray(),
                    outputDevice,
                    inputDevice);

                inputDevice.StopEventsListening();
            }
        }

        #endregion
    }
}
