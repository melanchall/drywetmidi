using System;
using System.Diagnostics;
using System.Threading;

namespace Melanchall.DryWetMidi.Tests.Common
{
    public static class WaitOperations
    {
        #region Methods

        public static void Wait(TimeSpan waitTime) =>
            Wait((long)waitTime.TotalMilliseconds);

        public static void Wait(long waitTime)
        {
            var stopwatch = Stopwatch.StartNew();

            while (stopwatch.ElapsedMilliseconds < waitTime)
            {
                Sleep();
            }

            stopwatch.Stop();
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
