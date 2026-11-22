using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
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

        private sealed class CallbackTickGeneratorRun
        {
            public IntPtr TickGeneratorInfo { get; set; }

            public TickGeneratorApi.TimerCallback_Win? WindowsCallback { get; set; }

            public TickGeneratorApi.TimerCallback_Mac? MacCallback { get; set; }
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

        [Test]
        public void AbandonedRunningTickGeneratorIsFinalizedSafely()
        {
            var tickGeneratorReference = CreateAndAbandonRunningTickGenerator();

            ForceGarbageCollection();

            ClassicAssert.IsFalse(
                tickGeneratorReference.TryGetTarget(out _),
                "Abandoned running tick generator was not finalized.");
        }

        [Test]
        public void StopAndCloseWhileTickCallbackIsRunning()
        {
            using (var callbackEntered = new ManualResetEventSlim())
            using (var releaseCallback = new ManualResetEventSlim())
            using (var callbackExited = new ManualResetEventSlim())
            {
                var callbackTickGeneratorRun = new CallbackTickGeneratorRun();
                Action callback = () =>
                {
                    callbackEntered.Set();
                    releaseCallback.Wait(TimeSpan.FromSeconds(10));
                    callbackExited.Set();
                };

                var sessionHandle = OpenTickGeneratorSession();
                var startResult = StartCallbackTickGenerator(
                    1,
                    sessionHandle,
                    callback,
                    callbackTickGeneratorRun,
                    out var errorCode);

                if (startResult != TickGeneratorApi.TG_STARTRESULT.TG_STARTRESULT_OK)
                {
                    TickGeneratorSessionApi.Api_CloseSession(sessionHandle, out _);
                    ClassicAssert.Fail($"Tick generator was not started; result: {startResult}, error code: {errorCode}.");
                }

                var sessionClosed = false;
                var generatorStopped = false;

                try
                {
                    ClassicAssert.IsTrue(callbackEntered.Wait(WaitTime), "Tick callback did not start.");

                    var stopResult = TickGeneratorApi.Api_StopHighPrecisionTickGenerator(
                        sessionHandle,
                        callbackTickGeneratorRun.TickGeneratorInfo,
                        out errorCode);
                    generatorStopped = true;
                    ClassicAssert.AreEqual(
                        TickGeneratorApi.TG_STOPRESULT.TG_STOPRESULT_OK,
                        stopResult,
                        $"Tick generator was not stopped; error code: {errorCode}.");

                    var closeResult = TickGeneratorSessionApi.Api_CloseSession(sessionHandle, out errorCode);
                    sessionClosed = true;

                    var expectedCloseResult = CommonApi.Api_GetOsType() == CommonApi.OsType.MacOS
                        ? TickGeneratorSessionApi.TGSESSION_CLOSERESULT.TGSESSION_CLOSERESULT_THREADEXITTIMEOUT
                        : TickGeneratorSessionApi.TGSESSION_CLOSERESULT.TGSESSION_CLOSERESULT_OK;
                    ClassicAssert.AreEqual(
                        expectedCloseResult,
                        closeResult,
                        $"Session close result was unexpected; error code: {errorCode}.");
                }
                finally
                {
                    releaseCallback.Set();

                    if (!generatorStopped)
                    {
                        TickGeneratorApi.Api_StopHighPrecisionTickGenerator(
                            sessionHandle,
                            callbackTickGeneratorRun.TickGeneratorInfo,
                            out _);
                    }

                    if (!sessionClosed)
                        TickGeneratorSessionApi.Api_CloseSession(sessionHandle, out _);

                    if (callbackEntered.IsSet)
                    {
                        ClassicAssert.IsTrue(
                            callbackExited.Wait(WaitTime),
                            "Tick callback did not finish.");
                    }

                    GC.KeepAlive(callbackTickGeneratorRun.WindowsCallback);
                    GC.KeepAlive(callbackTickGeneratorRun.MacCallback);
                }
            }
        }

        [Test]
        public void StopTickGeneratorFromItsCallback()
        {
            using (var allowStop = new ManualResetEventSlim())
            using (var callbackFinished = new ManualResetEventSlim())
            {
                var callbackTickGeneratorRun = new CallbackTickGeneratorRun();
                var stopResult = TickGeneratorApi.TG_STOPRESULT.TG_STOPRESULT_CANTKILLEVENT;
                var stopErrorCode = 0;
                var stopStarted = 0;
                var sessionHandle = OpenTickGeneratorSession();

                Action callback = () =>
                {
                    if (allowStop.Wait(WaitTime) &&
                        Interlocked.CompareExchange(ref stopStarted, 1, 0) == 0)
                    {
                        stopResult = TickGeneratorApi.Api_StopHighPrecisionTickGenerator(
                            sessionHandle,
                            callbackTickGeneratorRun.TickGeneratorInfo,
                            out stopErrorCode);
                    }

                    callbackFinished.Set();
                };

                var startResult = StartCallbackTickGenerator(
                    1,
                    sessionHandle,
                    callback,
                    callbackTickGeneratorRun,
                    out var errorCode);

                if (startResult != TickGeneratorApi.TG_STARTRESULT.TG_STARTRESULT_OK)
                {
                    TickGeneratorSessionApi.Api_CloseSession(sessionHandle, out _);
                    ClassicAssert.Fail($"Tick generator was not started; result: {startResult}, error code: {errorCode}.");
                }

                var sessionClosed = false;

                try
                {
                    allowStop.Set();

                    var callbackCompleted = callbackFinished.Wait(WaitTime);
                    if (!callbackCompleted &&
                        Interlocked.CompareExchange(ref stopStarted, 1, 0) == 0)
                    {
                        stopResult = TickGeneratorApi.Api_StopHighPrecisionTickGenerator(
                            sessionHandle,
                            callbackTickGeneratorRun.TickGeneratorInfo,
                            out stopErrorCode);
                        callbackCompleted = callbackFinished.Wait(WaitTime);
                    }

                    ClassicAssert.IsTrue(callbackCompleted, "Tick callback did not finish after stopping its generator.");
                    ClassicAssert.AreEqual(
                        TickGeneratorApi.TG_STOPRESULT.TG_STOPRESULT_OK,
                        stopResult,
                        $"Tick generator was not stopped by its callback; error code: {stopErrorCode}.");

                    var closeResult = TickGeneratorSessionApi.Api_CloseSession(sessionHandle, out errorCode);
                    sessionClosed = true;

                    ClassicAssert.AreEqual(
                        TickGeneratorSessionApi.TGSESSION_CLOSERESULT.TGSESSION_CLOSERESULT_OK,
                        closeResult,
                        $"Session was not closed successfully; error code: {errorCode}.");
                }
                finally
                {
                    allowStop.Set();

                    if (Interlocked.CompareExchange(ref stopStarted, 1, 0) == 0)
                    {
                        TickGeneratorApi.Api_StopHighPrecisionTickGenerator(
                            sessionHandle,
                            callbackTickGeneratorRun.TickGeneratorInfo,
                            out _);
                    }

                    callbackFinished.Wait(WaitTime);

                    if (!sessionClosed)
                        TickGeneratorSessionApi.Api_CloseSession(sessionHandle, out _);

                    GC.KeepAlive(callbackTickGeneratorRun.WindowsCallback);
                    GC.KeepAlive(callbackTickGeneratorRun.MacCallback);
                }
            }
        }

        #endregion

        #region Private methods

        private static IntPtr OpenTickGeneratorSession()
        {
            var openResult = TickGeneratorSessionApi.Api_OpenSession(out var sessionHandle, out var errorCode);
            ClassicAssert.AreEqual(
                TickGeneratorSessionApi.TGSESSION_OPENRESULT.TGSESSION_OPENRESULT_OK,
                openResult,
                $"Tick generator session was not opened; error code: {errorCode}.");

            return sessionHandle;
        }

        private static TickGeneratorApi.TG_STARTRESULT StartCallbackTickGenerator(
            int intervalInMs,
            IntPtr sessionHandle,
            Action callback,
            CallbackTickGeneratorRun callbackTickGeneratorRun,
            out int errorCode)
        {
            if (CommonApi.Api_GetOsType() == CommonApi.OsType.Windows)
            {
                var windowsCallback = (TickGeneratorApi.TimerCallback_Win)((uID, uMsg, dwUser, dw1, dw2) => callback());
                callbackTickGeneratorRun.WindowsCallback = windowsCallback;

                var result = TickGeneratorApi.Api_StartHighPrecisionTickGenerator_Win(
                    intervalInMs,
                    sessionHandle,
                    windowsCallback,
                    out var tickGeneratorInfo,
                    out errorCode);

                callbackTickGeneratorRun.TickGeneratorInfo = tickGeneratorInfo;
                return result;
            }

            TickGeneratorApi.TimerCallback_Mac macCallback = () => callback();
            callbackTickGeneratorRun.MacCallback = macCallback;

            var macResult = TickGeneratorApi.Api_StartHighPrecisionTickGenerator_Mac(
                intervalInMs,
                sessionHandle,
                macCallback,
                out var macTickGeneratorInfo,
                out errorCode);

            callbackTickGeneratorRun.TickGeneratorInfo = macTickGeneratorInfo;
            return macResult;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference<HighPrecisionTickGenerator> CreateAndAbandonRunningTickGenerator()
        {
            var tickGenerator = new HighPrecisionTickGenerator();
            tickGenerator.TryStart(TimeSpan.FromMilliseconds(1));

            return new WeakReference<HighPrecisionTickGenerator>(tickGenerator, true);
        }

        private void CheckTicksContinueAfterGarbageCollection(RunInfo runInfo, string scenario)
        {
            ForceGarbageCollection();

            var tickCountAfterGarbageCollection = GetTickCount(runInfo);
            var tickGenerated = WaitOperations.Wait(
                () => GetTickCount(runInfo) > tickCountAfterGarbageCollection,
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
