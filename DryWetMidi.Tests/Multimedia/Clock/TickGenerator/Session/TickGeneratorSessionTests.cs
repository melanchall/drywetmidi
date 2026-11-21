using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Attributes;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed class TickGeneratorSessionTests
    {
        #region Delegates

        private delegate IntPtr CreateSessionHandleDelegate();

        #endregion

        #region Test methods

        [MacOnly]
        [Test]
        public void CloseImmediatelyAfterOpening()
        {
            for (var i = 0; i < 20; i++)
            {
                var openResult = TickGeneratorSessionApi.Api_OpenSession(out var sessionHandle, out var errorCode);
                ClassicAssert.AreEqual(
                    TickGeneratorSessionApi.TGSESSION_OPENRESULT.TGSESSION_OPENRESULT_OK,
                    openResult,
                    $"Session {i} was not opened successfully; error code: {errorCode}.");

                var closeResult = TickGeneratorSessionApi.Api_CloseSession(sessionHandle, out errorCode);

                ClassicAssert.AreEqual(
                    TickGeneratorSessionApi.TGSESSION_CLOSERESULT.TGSESSION_CLOSERESULT_OK,
                    closeResult,
                    $"Session {i} was not closed successfully; error code: {errorCode}.");
            }
        }

        [MacOnly]
        [Test]
        public void StopAndCloseWhileTickCallbackIsRunning()
        {
            using (var callbackEntered = new ManualResetEventSlim())
            using (var releaseCallback = new ManualResetEventSlim())
            using (var callbackExited = new ManualResetEventSlim())
            {
                TickGeneratorApi.TimerCallback_Mac callback = () =>
                {
                    callbackEntered.Set();
                    releaseCallback.Wait(TimeSpan.FromSeconds(10));
                    callbackExited.Set();
                };

                var openResult = TickGeneratorSessionApi.Api_OpenSession(out var sessionHandle, out var errorCode);
                ClassicAssert.AreEqual(
                    TickGeneratorSessionApi.TGSESSION_OPENRESULT.TGSESSION_OPENRESULT_OK,
                    openResult,
                    $"Session was not opened successfully; error code: {errorCode}.");

                var startResult = TickGeneratorApi.Api_StartHighPrecisionTickGenerator_Mac(
                    1,
                    sessionHandle,
                    callback,
                    out var tickGeneratorInfo,
                    out errorCode);

                if (startResult != TickGeneratorApi.TG_STARTRESULT.TG_STARTRESULT_OK)
                {
                    TickGeneratorSessionApi.Api_CloseSession(sessionHandle, out _);
                    ClassicAssert.Fail($"Tick generator was not started; result: {startResult}, error code: {errorCode}.");
                }

                var sessionClosed = false;
                var generatorStopped = false;

                try
                {
                    ClassicAssert.IsTrue(callbackEntered.Wait(TimeSpan.FromSeconds(5)), "Tick callback did not start.");

                    var stopResult = TickGeneratorApi.Api_StopHighPrecisionTickGenerator(sessionHandle, tickGeneratorInfo, out errorCode);
                    generatorStopped = true;
                    ClassicAssert.AreEqual(
                        TickGeneratorApi.TG_STOPRESULT.TG_STOPRESULT_OK,
                        stopResult,
                        $"Tick generator was not stopped; error code: {errorCode}.");

                    var closeResult = TickGeneratorSessionApi.Api_CloseSession(sessionHandle, out errorCode);
                    sessionClosed = true;

                    ClassicAssert.AreEqual(
                        TickGeneratorSessionApi.TGSESSION_CLOSERESULT.TGSESSION_CLOSERESULT_THREADEXITTIMEOUT,
                        closeResult,
                        $"Close did not report the active callback; error code: {errorCode}.");
                }
                finally
                {
                    releaseCallback.Set();

                    if (!generatorStopped)
                        TickGeneratorApi.Api_StopHighPrecisionTickGenerator(sessionHandle, tickGeneratorInfo, out _);

                    if (!sessionClosed)
                        TickGeneratorSessionApi.Api_CloseSession(sessionHandle, out _);

                    if (callbackEntered.IsSet)
                        ClassicAssert.IsTrue(callbackExited.Wait(TimeSpan.FromSeconds(5)), "Tick callback did not finish.");

                    GC.KeepAlive(callback);
                }
            }
        }

        [MacOnly]
        [Test]
        public void StopTickGeneratorFromItsCallback()
        {
            using (var allowStop = new ManualResetEventSlim())
            using (var callbackFinished = new ManualResetEventSlim())
            {
                var tickGeneratorInfo = IntPtr.Zero;
                var stopResult = TickGeneratorApi.TG_STOPRESULT.TG_STOPRESULT_CANTKILLEVENT;
                var stopErrorCode = 0;

                var openResult = TickGeneratorSessionApi.Api_OpenSession(out var sessionHandle, out var errorCode);
                ClassicAssert.AreEqual(
                    TickGeneratorSessionApi.TGSESSION_OPENRESULT.TGSESSION_OPENRESULT_OK,
                    openResult,
                    $"Session was not opened successfully; error code: {errorCode}.");

                TickGeneratorApi.TimerCallback_Mac callback = () =>
                {
                    if (allowStop.Wait(TimeSpan.FromSeconds(10)))
                        stopResult = TickGeneratorApi.Api_StopHighPrecisionTickGenerator(sessionHandle, tickGeneratorInfo, out stopErrorCode);

                    callbackFinished.Set();
                };

                var startResult = TickGeneratorApi.Api_StartHighPrecisionTickGenerator_Mac(
                    1,
                    sessionHandle,
                    callback,
                    out tickGeneratorInfo,
                    out errorCode);

                if (startResult != TickGeneratorApi.TG_STARTRESULT.TG_STARTRESULT_OK)
                {
                    TickGeneratorSessionApi.Api_CloseSession(sessionHandle, out _);
                    ClassicAssert.Fail($"Tick generator was not started; result: {startResult}, error code: {errorCode}.");
                }

                allowStop.Set();
                var callbackCompleted = callbackFinished.Wait(TimeSpan.FromSeconds(5));
                var closeResult = TickGeneratorSessionApi.Api_CloseSession(sessionHandle, out errorCode);

                ClassicAssert.IsTrue(callbackCompleted, "Tick callback did not finish after stopping its generator.");
                ClassicAssert.AreEqual(
                    TickGeneratorApi.TG_STOPRESULT.TG_STOPRESULT_OK,
                    stopResult,
                    $"Tick generator was not stopped by its callback; error code: {stopErrorCode}.");
                ClassicAssert.AreEqual(
                    TickGeneratorSessionApi.TGSESSION_CLOSERESULT.TGSESSION_CLOSERESULT_OK,
                    closeResult,
                    $"Session was not closed successfully; error code: {errorCode}.");

                GC.KeepAlive(callback);
            }
        }

#if TEST
        [Test]
        public void CheckTickGeneratorSession_DisposeManually([Values(0, 50, 5000)] int waitAfterSessionCreatedMs)
        {
            var checkpoints = new TestCheckpoints();

            var rawHandle = CreateTickGeneratorSessionHandle(waitAfterSessionCreatedMs);
            var handle = new TickGeneratorSessionHandle(rawHandle);
            handle.TestCheckpoints = checkpoints;

            checkpoints.CheckCheckpointsAreNotReached(
                TickGeneratorSessionCheckpointNames.HandleFinalizerEntered,
                TickGeneratorSessionCheckpointNames.SessionClosedInHandleFinalizer);

            handle.Dispose();

            checkpoints.CheckCheckpointsReached(
                TickGeneratorSessionCheckpointNames.HandleFinalizerEntered,
                TickGeneratorSessionCheckpointNames.SessionClosedInHandleFinalizer);

            ClassicAssert.IsTrue(handle.IsClosed, "Handle is not closed after disposing.");
        }

        [Test]
        public void CheckTickGeneratorSession_AbandonAndWaitForFinalizer([Values(0, 50, 5000)] int waitAfterSessionCreatedMs)
        {
            var checkpoints = new TestCheckpoints();

            CreateAndAbandonTickGeneratorSession(checkpoints, waitAfterSessionCreatedMs);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            checkpoints.CheckCheckpointsReached(
                MidiDevicesSessionCheckpointNames.ReleaseHandleEntered,
                MidiDevicesSessionCheckpointNames.CloseSessionInReleaseHandle);
        }

        [Test]
        public void CheckTickGeneratorSession_CloseViaApi([Values(0, 50, 5000)] int waitAfterSessionCreatedMs)
        {
            var sessionHandle = CreateTickGeneratorSessionHandle(waitAfterSessionCreatedMs);

            var result = TickGeneratorSessionApi.Api_CloseSession(sessionHandle, out var errorCode);
            ClassicAssert.AreEqual(
                TickGeneratorSessionApi.TGSESSION_CLOSERESULT.TGSESSION_CLOSERESULT_OK,
                result,
                "Session was not closed successfully.");
        }
#endif

        #endregion

        #region Private methods

#if TEST
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void CreateAndAbandonTickGeneratorSession(
            TestCheckpoints checkpoints,
            int waitAfterSessionCreatedMs)
        {
            var rawHandle = CreateTickGeneratorSessionHandle(waitAfterSessionCreatedMs);
            var handle = new TickGeneratorSessionHandle(rawHandle);
            handle.TestCheckpoints = checkpoints;

            // Don't dispose - let it go out of scope
            // Finalizer will run eventually
        }

        private IntPtr CreateTickGeneratorSessionHandle(
            int waitAfterSessionCreatedMs)
        {
            var result = TickGeneratorSessionApi.Api_OpenSession(
                out var rawHandle,
                out var errorCode);

            ClassicAssert.AreEqual(
                TickGeneratorSessionApi.TGSESSION_OPENRESULT.TGSESSION_OPENRESULT_OK,
                result,
                "Session was not opened successfully.");

            if (waitAfterSessionCreatedMs > 0)
                WaitOperations.Wait(waitAfterSessionCreatedMs);

            return rawHandle;
        }
#endif

        #endregion
    }
}
