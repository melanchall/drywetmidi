using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Devices
{
    [TestFixture]
    public sealed partial class PlaybackTests
    {
        #region Constants

        private static readonly MidiEvent MidiEvent = new ProgramChangeEvent((SevenBitNumber)99);

        private static readonly EventCallback EventCallback = (e, rt, t) =>
        {
            return MidiEvent;
        };

        #endregion

        #region Test methods

        [Retry(RetriesNumber)]
        [Test]
        public void EventCallback_ReturnNull()
        {
            CheckEventCallback(
                eventsToSend: new[]
                {
                    new EventToSend(new ProgramChangeEvent((SevenBitNumber)33), TimeSpan.FromMilliseconds(200)),
                    new EventToSend(new StopEvent(), TimeSpan.FromMilliseconds(200))
                },
                expectedReceivedEvents: new ReceivedEvent[] { },
                changeCallbackAfter: TimeSpan.FromMilliseconds(250),
                eventCallback: (e, rt, t) => null,
                secondEventCallback: (e, rt, t) => null);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void EventCallback_WithNotes_ReturnNull()
        {
            CheckEventCallback(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), TimeSpan.FromMilliseconds(200)),
                    new EventToSend(new ProgramChangeEvent((SevenBitNumber)33), TimeSpan.FromMilliseconds(200))
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(200))
                },
                changeCallbackAfter: TimeSpan.FromMilliseconds(250),
                eventCallback: (e, rt, t) => null,
                secondEventCallback: (e, rt, t) => null);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void EventCallback_ReturnNull_ReturnOriginal()
        {
            CheckEventCallback(
                eventsToSend: new[]
                {
                    new EventToSend(new ProgramChangeEvent((SevenBitNumber)33), TimeSpan.FromMilliseconds(200)),
                    new EventToSend(new StopEvent(), TimeSpan.FromMilliseconds(200))
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new StopEvent(), TimeSpan.FromMilliseconds(400))
                },
                changeCallbackAfter: TimeSpan.FromMilliseconds(250),
                eventCallback: (e, rt, t) => null,
                secondEventCallback: (e, rt, t) => e);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void EventCallback_ReturnOriginal_ReturnNull()
        {
            CheckEventCallback(
                eventsToSend: new[]
                {
                    new EventToSend(new ProgramChangeEvent((SevenBitNumber)33), TimeSpan.FromMilliseconds(200)),
                    new EventToSend(new StopEvent(), TimeSpan.FromMilliseconds(200))
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)33), TimeSpan.FromMilliseconds(200))
                },
                changeCallbackAfter: TimeSpan.FromMilliseconds(250),
                eventCallback: (e, rt, t) => e,
                secondEventCallback: (e, rt, t) => null);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void EventCallback_ReturnOriginal_ReturnNew()
        {
            CheckEventCallback(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), TimeSpan.FromMilliseconds(200)),
                    new EventToSend(new ProgramChangeEvent((SevenBitNumber)33), TimeSpan.FromMilliseconds(300)),
                    new EventToSend(new StopEvent(), TimeSpan.FromMilliseconds(100))
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)33), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(MidiEvent, TimeSpan.FromMilliseconds(600))
                },
                changeCallbackAfter: TimeSpan.FromMilliseconds(550),
                eventCallback: (e, rt, t) => e,
                secondEventCallback: EventCallback);
        }

        #endregion
    }
}
