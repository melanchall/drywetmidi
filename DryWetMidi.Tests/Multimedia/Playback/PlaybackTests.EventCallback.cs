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

        private static readonly MidiEvent MidiEventForCallback = new ProgramChangeEvent((SevenBitNumber)99);

        private static readonly EventCallback EventCallback = (e, rt, t) =>
        {
            return MidiEventForCallback;
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
                expectedReceivedEvents: new SentReceivedEvent[] { });
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
                    new SentReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new SentReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(200))
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
                    new TimedEvent(new NoteAftertouchEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(400), TempoMap),
                },
                initialEventCallback: (e, rt, t) => null,
                actions: new[]
                {
                    new PlaybackAction(TimeSpan.FromMilliseconds(250),
                        p => p.EventCallback = (e, rt, t) => e),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteAftertouchEvent(), TimeSpan.FromMilliseconds(400))
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
                    new SentReceivedEvent(new ProgramChangeEvent((SevenBitNumber)33), TimeSpan.FromMilliseconds(200))
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
                    new TimedEvent(new NoteAftertouchEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(600), TempoMap),
                },
                initialEventCallback: (e, rt, t) => e,
                actions: new[]
                {
                    new PlaybackAction(TimeSpan.FromMilliseconds(550),
                        p => p.EventCallback = EventCallback),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new SentReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(200)),
                    new SentReceivedEvent(new ProgramChangeEvent((SevenBitNumber)33), TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(MidiEventForCallback, TimeSpan.FromMilliseconds(600))
                });
        }

        #endregion

        #region Private methods

        private void CheckEventCallback(
            ICollection<ITimedObject> initialPlaybackObjects,
            EventCallback initialEventCallback,
            PlaybackAction[] actions,
            ICollection<SentReceivedEvent> expectedReceivedEvents)
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
