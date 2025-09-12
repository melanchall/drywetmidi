using System;
using System.Diagnostics;
using System.Threading;

namespace Melanchall.DryWetMidi.Tests.Common
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
                Sleep();
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
                Sleep();
            }

            stopwatch.Stop();
            return exitCondition();
        }

        private static void Sleep()
        {
            Thread.Sleep(1);
        }

        #endregion
    }
}
