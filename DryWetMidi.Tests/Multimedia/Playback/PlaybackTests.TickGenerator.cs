using System;
using System.Linq;
using System.Threading;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;
using System.Diagnostics;
using Melanchall.DryWetMidi.Interaction;
using System.IO;
using Melanchall.DryWetMidi.Tests.Common;

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

        #region Test mthods

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlayback_HighPrecisionTickGenerator()
        {
            CheckPlayback_TickGenerator(() => new HighPrecisionTickGenerator(), TimeSpan.FromMilliseconds(30));
        }

        //[Retry(RetriesNumber)]
        //[Test]
        public void CheckPlayback_HighPrecisionTickGenerator_DiscardResolutionIncreasingOnStop()
        {
            var processId = Process.GetCurrentProcess().Id;

            using (var playback = new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 100000 } }.GetPlayback(TempoMap.Default))
            {
                Assert.IsFalse(IsProcessIdInPowerCfgReport(processId), "Process ID is in PowerCfg report before playback started.");

                playback.Start();
                Assert.IsTrue(IsProcessIdInPowerCfgReport(processId), "Process ID isn't in PowerCfg report after playback started.");

                playback.Stop();
                Assert.IsFalse(IsProcessIdInPowerCfgReport(processId), "Process ID is in PowerCfg report after playback stopped.");

                playback.Start();
                Assert.IsTrue(IsProcessIdInPowerCfgReport(processId), "Process ID isn't in PowerCfg report after playback started again.");

                playback.Stop();
                Assert.IsFalse(IsProcessIdInPowerCfgReport(processId), "Process ID is in PowerCfg report after playback stopped again.");
            }

            Assert.IsFalse(IsProcessIdInPowerCfgReport(processId), "Process ID is in PowerCfg report after playback disposed.");
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
                    var areEventsReceived = WaitOperations.Wait(() => context.ReceivedEvents.Count == eventsToSend.Length, timeout);
                    Assert.IsTrue(areEventsReceived, $"Events are not received for timeout {timeout}.");
                },
                finalChecks: (context, playback) =>
                {
                    var playbackStopped = WaitOperations.Wait(() => !playback.IsRunning, maximumEventSendReceiveDelay);
                    Assert.IsTrue(playbackStopped, "Playback is running after completed.");
                },
                createTickGeneratorCallback: () => null);
        }

        #endregion

        #region Private methods

        private bool IsProcessIdInPowerCfgReport(int processId)
        {
            var reportFilePath = Path.Combine(Path.GetTempPath(), "powercfg_report.html");

            try
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C powercfg /energy /output \"{reportFilePath}\" /duration 3"
                });
                Assert.IsNotNull(process, "PowerCfg process is null.");

                process.WaitForExit();

                var report = FileOperations.ReadAllFileText(reportFilePath);
                return report.Contains(processId.ToString());
            }
            finally
            {
                FileOperations.DeleteFile(reportFilePath);
            }
        }

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
                    var areEventsReceived = WaitOperations.Wait(() => context.ReceivedEvents.Count == eventsToSend.Length, timeout);
                    Assert.IsTrue(areEventsReceived, $"Events are not received for timeout {timeout}.");
                },
                finalChecks: (context, playback) =>
                {
                    var playbackStopped = WaitOperations.Wait(() => !playback.IsRunning, maximumEventSendReceiveDelay);
                    Assert.IsTrue(playbackStopped, "Playback is running after completed.");
                },
                createTickGeneratorCallback: createTickGeneratorCallback);
        }

        #endregion
    }
}
