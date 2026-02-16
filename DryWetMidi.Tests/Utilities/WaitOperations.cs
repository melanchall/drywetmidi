using System;
using System.Diagnostics;
using System.Threading;

namespace Melanchall.DryWetMidi.Tests
{
    public static class WaitOperations
    {
        #region Methods

        public static long Wait(TimeSpan waitTime) =>
            Wait((long)waitTime.TotalMilliseconds);

        public static long Wait(long waitTimeMs)
        {
            var stopwatch = Stopwatch.StartNew();

            while (stopwatch.ElapsedMilliseconds < waitTimeMs)
            {
                Yield();
            }

            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        public static void WaitPrecisely(TimeSpan waitTime) =>
            WaitPrecisely((long)waitTime.TotalMilliseconds);

        public static void WaitPrecisely(long waitTimeMs)
        {
            var stopwatch = Stopwatch.StartNew();

            while (stopwatch.ElapsedMilliseconds < waitTimeMs)
            {
                Yield();
            }
        }

        public static bool Wait(Func<bool> exitCondition) =>
            Wait(exitCondition, TimeSpan.MaxValue);

        public static bool Wait(Func<bool> exitCondition, TimeSpan timeout) =>
            Wait(exitCondition, (long)timeout.TotalMilliseconds);

        public static bool Wait(Func<bool> exitCondition, long timeout)
        {
            var stopwatch = Stopwatch.StartNew();

            while (!exitCondition() && stopwatch.ElapsedMilliseconds < timeout)
            {
                Yield();
            }

            stopwatch.Stop();
            return exitCondition();
        }

        private static void Yield()
        {
            Thread.Yield();
        }

        #endregion
    }
}
