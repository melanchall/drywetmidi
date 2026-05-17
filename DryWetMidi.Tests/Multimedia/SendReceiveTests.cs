using System;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Attributes;
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

        [MultimediaTestRetry]
        [Test]
        public void CheckEventsReceiving()
        {
            using (var outputEndpoint = OutputEndpoint.GetByName(SendReceiveUtilities.DeviceToTestOnName))
            using (var inputEndpoint = InputEndpoint.GetByName(SendReceiveUtilities.DeviceToTestOnName))
            {
                outputEndpoint.PrepareForEventsSending();
                inputEndpoint.StartEventsListening();

                SendReceiveUtilities.CheckEventsReceiving(
                    new[]
                    {
                        new TimestampedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }, TimeSpan.Zero),
                        new TimestampedEvent(new NormalSysExEvent(new byte[] { 1, 2, 3, 0xF7 }), TimeSpan.FromSeconds(1)),
                        new TimestampedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 }, TimeSpan.FromSeconds(3)),
                        new TimestampedEvent(new NormalSysExEvent(new byte[] { 4, 5, 6, 0xF7 }), TimeSpan.FromSeconds(5)),
                        new TimestampedEvent(new SongSelectEvent((SevenBitNumber)20), TimeSpan.FromSeconds(5)),
                        new TimestampedEvent(new TuneRequestEvent(), TimeSpan.FromMilliseconds(5200)),
                    },
                    outputEndpoint,
                    inputEndpoint);

                inputEndpoint.StopEventsListening();
            }
        }

        [MultimediaTestRetry]
        [Test]
        public void CheckEventsReceiving_AllEventTypes_ExceptSysEx()
        {
            var events = TypesProvider.GetAllEventTypes()
                .Where(t => !typeof(MetaEvent).IsAssignableFrom(t) && !typeof(SysExEvent).IsAssignableFrom(t))
                .Select(t => (MidiEvent)Activator.CreateInstance(t))
                .ToArray();

            CollectionAssert.IsNotEmpty(events, "Events collection is empty.");

            using (var outputEndpoint = OutputEndpoint.GetByName(SendReceiveUtilities.DeviceToTestOnName))
            using (var inputEndpoint = InputEndpoint.GetByName(SendReceiveUtilities.DeviceToTestOnName))
            {
                outputEndpoint.PrepareForEventsSending();
                inputEndpoint.StartEventsListening();

                SendReceiveUtilities.CheckEventsReceiving(
                    events.Select((e, i) => new TimestampedEvent(e, TimeSpan.FromMilliseconds(50).MultiplyBy(i))).ToArray(),
                    outputEndpoint,
                    inputEndpoint);

                inputEndpoint.StopEventsListening();
            }
        }

        #endregion
    }
}
