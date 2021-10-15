using System;
using System.Collections.Generic;
using System.Diagnostics;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Common;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed class HighPrecisionTickGeneratorTests
    {
        #region Test methods

        //[Retry(5)]
        //[Test]
        public void CheckInterval([Values(1, 10, 100)] int interval)
        {
            using (var tickGenerator = new HighPrecisionTickGenerator())
            {
                var stopwatch = new Stopwatch();
                var elapsedTimes = new List<long>(150);

                tickGenerator.TickGenerated += (_, __) => elapsedTimes.Add(stopwatch.ElapsedMilliseconds);

                tickGenerator.TryStart(TimeSpan.FromMilliseconds(interval));
                stopwatch.Start();

                WaitOperations.Wait(() => elapsedTimes.Count >= 100);
                tickGenerator.TryStop();
                stopwatch.Stop();

                var time = elapsedTimes[0];
                var maxDelta = 0L;
                var tolerance = interval * 0.5;

                foreach (var t in elapsedTimes)
                {
                    maxDelta = Math.Max(t - time, maxDelta);
                    time = t;
                }

                Assert.Less(maxDelta, interval + tolerance, "Max time delta is too big.");
            }
        }

        #endregion
    }
}
