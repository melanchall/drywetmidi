using System;
using System.Linq;
using System.Threading;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;
using System.Diagnostics;

namespace Melanchall.DryWetMidi.Tests.Devices
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
                if (_thread != null)
                    return;

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

        #region Test mthods

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
            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }, TimeSpan.Zero),
                new EventToSend(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 }, TimeSpan.FromSeconds(2)),
                new EventToSend(new NoteOnEvent(), TimeSpan.FromSeconds(1)),
                new EventToSend(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.Zero),
                new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(3)),
                new EventToSend(new NoteOffEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.Zero)
            };

            var maximumEventSendReceiveDelay = TimeSpan.FromMilliseconds(10);

            CheckPlayback(
                eventsToSend,
                1.0,
                beforePlaybackStarted: NoPlaybackAction,
                startPlayback: (context, playback) =>
                {
                    var thread = new Thread(() =>
                    {
                        for (var i = 0; playback.IsRunning; i++)
                        {
                            if (i % 1000 == 0)
                                playback.TickClock();
                        }
                    });

                    playback.Start();
                    thread.Start();
                },
                afterPlaybackStarted: (context, playback) =>
                {
                    Assert.LessOrEqual(context.Stopwatch.Elapsed, maximumEventSendReceiveDelay, "Playback blocks current thread.");
                    Assert.IsTrue(playback.IsRunning, "Playback is not running after start.");
                },
                waiting: (context, playback) =>
                {
                    var timeout = context.ExpectedTimes.Last() + maximumEventSendReceiveDelay;
                    var areEventsReceived = SpinWait.SpinUntil(() => context.ReceivedEvents.Count == eventsToSend.Length, timeout);
                    Assert.IsTrue(areEventsReceived, $"Events are not received for timeout {timeout}.");
                },
                finalChecks: (context, playback) =>
                {
                    var playbackStopped = SpinWait.SpinUntil(() => !playback.IsRunning, maximumEventSendReceiveDelay);
                    Assert.IsTrue(playbackStopped, "Playback is running after completed.");
                },
                createTickGeneratorCallback: () => null);
        }

        #endregion

        #region Private methods

        private void CheckPlayback_TickGenerator(Func<TickGenerator> createTickGeneratorCallback, TimeSpan maximumEventSendReceiveDelay)
        {
            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }, TimeSpan.Zero),
                new EventToSend(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 }, TimeSpan.FromSeconds(2)),
                new EventToSend(new NoteOnEvent(), TimeSpan.FromSeconds(1)),
                new EventToSend(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.Zero),
                new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(3)),
                new EventToSend(new NoteOffEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.Zero)
            };

            CheckPlayback(
                eventsToSend,
                1.0,
                beforePlaybackStarted: NoPlaybackAction,
                startPlayback: (context, playback) => playback.Start(),
                afterPlaybackStarted: (context, playback) =>
                {
                    Assert.LessOrEqual(context.Stopwatch.Elapsed, maximumEventSendReceiveDelay, "Playback blocks current thread.");
                    Assert.IsTrue(playback.IsRunning, "Playback is not running after start.");
                },
                waiting: (context, playback) =>
                {
                    var timeout = context.ExpectedTimes.Last() + maximumEventSendReceiveDelay;
                    var areEventsReceived = SpinWait.SpinUntil(() => context.ReceivedEvents.Count == eventsToSend.Length, timeout);
                    Assert.IsTrue(areEventsReceived, $"Events are not received for timeout {timeout}.");
                },
                finalChecks: (context, playback) =>
                {
                    var playbackStopped = SpinWait.SpinUntil(() => !playback.IsRunning, maximumEventSendReceiveDelay);
                    Assert.IsTrue(playbackStopped, "Playback is running after completed.");
                },
                createTickGeneratorCallback: createTickGeneratorCallback);
        }

        #endregion
    }
}
