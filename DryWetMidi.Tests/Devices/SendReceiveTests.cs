using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Devices
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
            SendReceiveUtilities.CheckEventsReceiving(new[]
            {
                new EventToSend(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }, TimeSpan.Zero),
                new EventToSend(new NormalSysExEvent(new byte[] { 1, 2, 3, 0xF7 }), TimeSpan.FromSeconds(1)),
                new EventToSend(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 }, TimeSpan.FromSeconds(2)),
                new EventToSend(new NormalSysExEvent(new byte[] { 4, 5, 6, 0xF7 }), TimeSpan.FromSeconds(2)),
                new EventToSend(new SongSelectEvent((SevenBitNumber)20), TimeSpan.Zero),
                new EventToSend(new TuneRequestEvent(), TimeSpan.FromMilliseconds(200)),
            });
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
            SendReceiveUtilities.CheckEventsReceiving(events.Select(e => new EventToSend(e, TimeSpan.Zero)).ToArray());
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckFileEventsReceiving()
        {
            var filesToTestCount = 5;
            var maxFileDuration = TimeSpan.FromSeconds(10);

            var filesToTest = TestFilesProvider.GetValidFiles(
                    f => f.GetTrackChunks().Count() == 1,
                    f => (TimeSpan)f.GetDuration<MetricTimeSpan>() < maxFileDuration)
                .OrderByDescending(f => f.GetDuration<MetricTimeSpan>())
                .Take(filesToTestCount)
                .ToArray();
            Debug.Assert(filesToTest.Length == filesToTestCount, "Not enough files for test.");

            for (var i = 0; i < filesToTest.Length; i++)
            {
                var file = filesToTest[i];
                var tempoMap = file.GetTempoMap();

                var eventsToSend = new List<EventToSend>();
                var currentTime = TimeSpan.Zero;

                foreach (var timedEvent in file.GetTimedEvents().Where(e => !(e.Event is MetaEvent)))
                {
                    var time = (TimeSpan)timedEvent.TimeAs<MetricTimeSpan>(tempoMap);
                    var eventToSend = new EventToSend(timedEvent.Event, time - currentTime);
                    currentTime = time;
                    eventsToSend.Add(eventToSend);
                }

                SendReceiveUtilities.CheckEventsReceiving(eventsToSend);
            }
        }

        #endregion
    }
}
