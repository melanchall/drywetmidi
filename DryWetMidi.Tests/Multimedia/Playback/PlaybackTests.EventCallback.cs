using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;
using Melanchall.DryWetMidi.Interaction;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Tests.Multimedia
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
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ProgramChangeEvent((SevenBitNumber)33)).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(200), TempoMap),
                    new TimedEvent(new StopEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(400), TempoMap),
                },
                initialEventCallback: (e, rt, t) => null,
                actions: new[]
                {
                    new PlaybackAction(TimeSpan.FromMilliseconds(250),
                        p => p.EventCallback = (e, rt, t) => null),
                },
                expectedReceivedEvents: new ReceivedEvent[] { });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void EventCallback_WithNotes_ReturnNull()
        {
            CheckEventCallback(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(0), TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(200), TempoMap),
                    new TimedEvent(new ProgramChangeEvent((SevenBitNumber)33)).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(233), TempoMap),
                },
                initialEventCallback: (e, rt, t) => null,
                actions: new[]
                {
                    new PlaybackAction(TimeSpan.FromMilliseconds(250),
                        p => p.EventCallback = (e, rt, t) => null),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(200))
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void EventCallback_ReturnNull_ReturnOriginal()
        {
            CheckEventCallback(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ProgramChangeEvent((SevenBitNumber)33)).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(200), TempoMap),
                    new TimedEvent(new StopEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(400), TempoMap),
                },
                initialEventCallback: (e, rt, t) => null,
                actions: new[]
                {
                    new PlaybackAction(TimeSpan.FromMilliseconds(250),
                        p => p.EventCallback = (e, rt, t) => e),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new StopEvent(), TimeSpan.FromMilliseconds(400))
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void EventCallback_ReturnOriginal_ReturnNull()
        {
            CheckEventCallback(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ProgramChangeEvent((SevenBitNumber)33)).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(200), TempoMap),
                    new TimedEvent(new StopEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(400), TempoMap),
                },
                initialEventCallback: (e, rt, t) => e,
                actions: new[]
                {
                    new PlaybackAction(TimeSpan.FromMilliseconds(250),
                        p => p.EventCallback = (e, rt, t) => null),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)33), TimeSpan.FromMilliseconds(200))
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void EventCallback_ReturnOriginal_ReturnNew()
        {
            CheckEventCallback(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(0), TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(200), TempoMap),
                    new TimedEvent(new ProgramChangeEvent((SevenBitNumber)33)).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap),
                    new TimedEvent(new StopEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(600), TempoMap),
                },
                initialEventCallback: (e, rt, t) => e,
                actions: new[]
                {
                    new PlaybackAction(TimeSpan.FromMilliseconds(550),
                        p => p.EventCallback = EventCallback),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(200)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)33), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(MidiEvent, TimeSpan.FromMilliseconds(600))
                });
        }

        #endregion

        #region Private methods

        private void CheckEventCallback(
            ICollection<ITimedObject> initialPlaybackObjects,
            EventCallback initialEventCallback,
            PlaybackAction[] actions,
            ICollection<ReceivedEvent> expectedReceivedEvents)
        {
            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: initialPlaybackObjects,
                actions: actions,
                expectedReceivedEvents: expectedReceivedEvents,
                setupPlayback: playback => playback.EventCallback = initialEventCallback);
        }

        #endregion
    }
}
