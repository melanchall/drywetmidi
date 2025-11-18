using System;
using System.Threading;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;
using System.Diagnostics;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class PlaybackTests
    {
        #region Nested classes

        private sealed class ThreadTickGenerator : TickGenerator
        {
            private Thread _thread;
            private bool _isRunning;
            private bool _disposed;

            protected override void Start(TimeSpan interval)
            {
                _thread = new Thread(() =>
                {
                    var stopwatch = new Stopwatch();
                    var lastMs = 0L;

                    stopwatch.Start();
                    _isRunning = true;

                    while (_isRunning)
                    {
                        var elapsedMs = stopwatch.ElapsedMilliseconds;
                        if (elapsedMs - lastMs >= interval.TotalMilliseconds)
                        {
                            GenerateTick();
                            lastMs = elapsedMs;
                        }
                    }
                });

                _thread.Start();
            }

            protected override void Stop()
            {
                _isRunning = false;
            }

            protected override void Dispose(bool disposing)
            {
                if (_disposed)
                    return;

                if (disposing)
                {
                    _isRunning = false;
                }

                _disposed = true;
            }
        }

        #endregion

        #region Test methods

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlayback_HighPrecisionTickGenerator()
        {
            CheckPlayback_TickGenerator(() => new HighPrecisionTickGenerator(), TimeSpan.FromMilliseconds(30));
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlayback_RegularPrecisionTickGenerator()
        {
            CheckPlayback_TickGenerator(() => new RegularPrecisionTickGenerator(), TimeSpan.FromMilliseconds(50));
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlayback_CustomTickGenerator()
        {
            CheckPlayback_TickGenerator(() => new ThreadTickGenerator(), TimeSpan.FromMilliseconds(10));
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlayback_ManualTicking()
        {
            Thread thread = null;

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 })
                        .SetTime((MetricTimeSpan)TimeSpan.FromSeconds(2), TempoMap),
                    new TimedEvent(new NoteOnEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromSeconds(3), TempoMap),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)50))
                        .SetTime((MetricTimeSpan)TimeSpan.FromSeconds(3), TempoMap),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromSeconds(6), TempoMap),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, (SevenBitNumber)50))
                        .SetTime((MetricTimeSpan)TimeSpan.FromSeconds(6), TempoMap),
                },
                actions: Array.Empty<PlaybackAction>(),
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }, TimeSpan.Zero),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 }, TimeSpan.FromSeconds(2)),
                    new SentReceivedEvent(new NoteOnEvent(), TimeSpan.FromSeconds(3)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.FromSeconds(3)),
                    new SentReceivedEvent(new NoteOffEvent(), TimeSpan.FromSeconds(6)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.FromSeconds(6)),
                },
                setupPlayback: playback =>
                {
                    thread = new Thread(() =>
                    {
                        for (var i = 0; playback.IsRunning; i++)
                        {
                            if (i % 1000 == 0)
                                playback.TickClock();
                        }
                    });
                },
                playbackSettings: new PlaybackSettings
                {
                    ClockSettings = new MidiClockSettings
                    {
                        CreateTickGeneratorCallback = () => null
                    }
                },
                afterStart: playback =>
                    thread.Start());
        }

        #endregion

        #region Private methods

        private void CheckPlayback_TickGenerator(Func<TickGenerator> createTickGeneratorCallback, TimeSpan maximumEventSendReceiveDelay)
        {
            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 })
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(1000), TempoMap),
                    new TimedEvent(new NoteOnEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(1500), TempoMap),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)50))
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(1500), TempoMap),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(3000), TempoMap),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, (SevenBitNumber)50))
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(3000), TempoMap),
                },
                actions: Array.Empty<PlaybackAction>(),
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }, TimeSpan.Zero),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 }, TimeSpan.FromMilliseconds(1000)),
                    new SentReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(1500)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.FromMilliseconds(1500)),
                    new SentReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(3000)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.FromMilliseconds(3000)),
                },
                sendReceiveTimeDelta: maximumEventSendReceiveDelay);
        }

        #endregion
    }
}
