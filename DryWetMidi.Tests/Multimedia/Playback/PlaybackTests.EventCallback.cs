using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public void EventCallback_ThrowException()
        {
            var errorOccurredData = new List<PlaybackErrorOccurredEventArgs>();
            var errorMessage = "FAIL!";

            CheckEventCallback(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ProgramChangeEvent((SevenBitNumber)33)).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(200), TempoMap),
                    new TimedEvent(new TextEvent("A")).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(400), TempoMap),
                },
                initialEventCallback: (e, rt, t) => null,
                actions: Array.Empty<PlaybackAction>(),
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new ProgramChangeEvent((SevenBitNumber)33), TimeSpan.FromMilliseconds(200)),
                    new SentReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(400)),
                },
                setupPlayback: playback =>
                {
                    errorOccurredData.Clear();

                    playback.EventCallback = (e, rt, t) => throw new InvalidOperationException(errorMessage);
                    playback.ErrorOccurred += (s, e) => errorOccurredData.Add(e);
                },
                additionalChecks: (p, e) =>
                {
                    ClassicAssert.AreEqual(2, errorOccurredData.Count, "Invalid number of errors.");

                    foreach (var errorData in errorOccurredData)
                    {
                        ClassicAssert.AreEqual(PlaybackSite.EventCallback, errorData.Site, "Invalid site.");
                        ClassicAssert.IsInstanceOf<InvalidOperationException>(errorData.Exception, "Invalid exception type.");
                        ClassicAssert.AreEqual(errorMessage, errorData.Exception.Message, "Invalid exception message.");
                    }
                });
        }

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
            ICollection<SentReceivedEvent> expectedReceivedEvents,
            Action<Playback> setupPlayback = null,
            Action<Playback, ICollection<SentReceivedEvent>> additionalChecks = null)
        {
            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: initialPlaybackObjects,
                actions: actions,
                expectedReceivedEvents: expectedReceivedEvents,
                setupPlayback: playback =>
                {
                    playback.EventCallback = initialEventCallback;
                    setupPlayback?.Invoke(playback);
                },
                additionalChecks: additionalChecks);
        }

        #endregion
    }
}
