using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Attributes;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed class HighPrecisionTickGeneratorTests
    {
        #region Nested classes

        private sealed class RunInfo
        {
            public RunInfo(HighPrecisionTickGenerator tickGenerator, int intervalInMs)
            {
                TickGenerator = tickGenerator;
                IntervalInMs = intervalInMs;
            }

            public HighPrecisionTickGenerator TickGenerator { get; }

            public int IntervalInMs { get; }

            public Stopwatch Stopwatch { get; } = new Stopwatch();

            public List<long> Times { get; } = new List<long>(100000);
        }

        #endregion

        #region Constants

        private static readonly TimeSpan WaitTime = TimeSpan.FromSeconds(5);

        #endregion

        #region Fields

#if NET9_0_OR_GREATER
        private readonly System.Threading.Lock _lockObject = new();
#else
        private readonly object _lockObject = new();
#endif

        #endregion

        #region Test methods

        [TimingCritical]
        [Test]
        public void CheckInterval([Values(1, 10, 100)] int intervalInMs)
        {
            var runInfo = PrepareRunInfo(intervalInMs);

            StartTickGenerator(runInfo);
            
            WaitOperations.Wait(WaitTime);
            
            StopTickGeneratorAndCheckIntervals(runInfo);
        }

        [TimingCritical]
        [Test]
        public void CheckParallelIntervals()
        {
            var runInfo1 = PrepareRunInfo(1);
            var runInfo2 = PrepareRunInfo(10);
            var runInfo3 = PrepareRunInfo(100);

            StartTickGenerator(runInfo1);
            StartTickGenerator(runInfo2);
            StartTickGenerator(runInfo3);

            WaitOperations.Wait(WaitTime);

            StopTickGeneratorAndCheckIntervals(runInfo1);
            StopTickGeneratorAndCheckIntervals(runInfo2);
            StopTickGeneratorAndCheckIntervals(runInfo3);
        }

        [MacOnly]
        [Test]
        public void TicksContinueAfterGarbageCollectionAndRestart()
        {
            var runInfo = PrepareRunInfo(1);
            StartTickGenerator(runInfo);

            try
            {
                CheckTicksContinueAfterGarbageCollection(runInfo, "while running");

                runInfo.Stopwatch.Stop();
                runInfo.TickGenerator.TryStop();

                lock (_lockObject)
                {
                    runInfo.Times.Clear();
                }

                runInfo.Stopwatch.Reset();
                runInfo.TickGenerator.TryStart(TimeSpan.FromMilliseconds(runInfo.IntervalInMs));
                runInfo.Stopwatch.Start();

                CheckTicksContinueAfterGarbageCollection(runInfo, "after restarting");
            }
            finally
            {
                runInfo.Stopwatch.Stop();
                runInfo.TickGenerator.TryStop();
                runInfo.TickGenerator.Dispose();
            }
        }

        [MacOnly]
        [Test]
        public void AbandonedRunningTickGeneratorIsFinalizedSafely()
        {
            var tickGeneratorReference = CreateAndAbandonRunningTickGenerator();

            ForceGarbageCollection();

            ClassicAssert.IsFalse(
                tickGeneratorReference.TryGetTarget(out _),
                "Abandoned running tick generator was not finalized.");
        }

        #endregion

        #region Private methods

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference<HighPrecisionTickGenerator> CreateAndAbandonRunningTickGenerator()
        {
            var tickGenerator = new HighPrecisionTickGenerator();
            tickGenerator.TryStart(TimeSpan.FromMilliseconds(1));

            return new WeakReference<HighPrecisionTickGenerator>(tickGenerator, true);
        }

        private void CheckTicksContinueAfterGarbageCollection(RunInfo runInfo, string scenario)
        {
            var tickCountBeforeGarbageCollection = GetTickCount(runInfo);

            ForceGarbageCollection();

            var tickGenerated = WaitOperations.Wait(
                () => GetTickCount(runInfo) > tickCountBeforeGarbageCollection,
                WaitTime);

            ClassicAssert.IsTrue(tickGenerated, $"No ticks were generated {scenario} forced garbage collection.");
        }

        private int GetTickCount(RunInfo runInfo)
        {
            lock (_lockObject)
            {
                return runInfo.Times.Count;
            }
        }

        private static void ForceGarbageCollection()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private static RunInfo PrepareRunInfo(int intervalInMs)
        {
            var tickGenerator = new HighPrecisionTickGenerator();
            return new RunInfo(tickGenerator, intervalInMs);
        }

        private void StartTickGenerator(RunInfo runInfo)
        {
            runInfo.TickGenerator.TickGenerated += (_, __) =>
            {
                lock (_lockObject)
                {
                    if (!runInfo.Stopwatch.IsRunning)
                        return;

                    runInfo.Times.Add(runInfo.Stopwatch.ElapsedMilliseconds);
                }
            };

            runInfo.TickGenerator.TryStart(TimeSpan.FromMilliseconds(runInfo.IntervalInMs));
            runInfo.Stopwatch.Start();
        }

        private void StopTickGeneratorAndCheckIntervals(RunInfo runInfo)
        {
            runInfo.Stopwatch.Stop();
            runInfo.TickGenerator.TryStop();
            runInfo.TickGenerator.Dispose();

            var deltas = new List<long>();
            var lastTime = 0L;

            lock (_lockObject)
            {
                foreach (var time in runInfo.Times)
                {
                    deltas.Add(time - lastTime);
                    lastTime = time;
                }
            }

            var min = deltas.Min();
            var max = deltas.Max();
            var average = deltas.Average();

            var tolerance = runInfo.IntervalInMs + 5;
            var ratio = deltas.Count(d => d >= runInfo.IntervalInMs - tolerance && d <= runInfo.IntervalInMs + tolerance) / (double)deltas.Count;
            ClassicAssert.Greater(ratio, 0.9, $"Count of good intervals of [{runInfo.IntervalInMs}] ms is too low (total count = [{deltas.Count}], min = [{min}], max = [{max}], average = [{average:0.##}]).");
        }

        #endregion
    }
}
