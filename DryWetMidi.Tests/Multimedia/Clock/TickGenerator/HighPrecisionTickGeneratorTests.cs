using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Common;
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

        #region Test methods

        [Retry(3)]
        [Test]
        public void CheckInterval([Values(1, 10, 100)] int intervalInMs)
        {
            var runInfo = PrepareRunInfo(intervalInMs);

            StartTickGenerator(runInfo);
            
            WaitOperations.Wait(WaitTime);
            
            StopTickGeneratorAndCheckIntervals(runInfo);
        }

        [Retry(3)]
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

        #endregion

        #region Private methods

        private static RunInfo PrepareRunInfo(int intervalInMs)
        {
            var tickGenerator = new HighPrecisionTickGenerator();
            return new RunInfo(tickGenerator, intervalInMs);
        }

        private static void StartTickGenerator(RunInfo runInfo)
        {
            runInfo.TickGenerator.TickGenerated += (_, __) => runInfo.Times.Add(runInfo.Stopwatch.ElapsedMilliseconds);
            runInfo.TickGenerator.TryStart(TimeSpan.FromMilliseconds(runInfo.IntervalInMs));
            runInfo.Stopwatch.Start();
        }

        private static void StopTickGeneratorAndCheckIntervals(RunInfo runInfo)
        {
            runInfo.TickGenerator.TryStop();
            runInfo.Stopwatch.Stop();
            runInfo.TickGenerator.Dispose();

            var deltas = new List<long>();
            var lastTime = 0L;

            foreach (var time in runInfo.Times)
            {
                deltas.Add(time - lastTime);
                lastTime = time;
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
