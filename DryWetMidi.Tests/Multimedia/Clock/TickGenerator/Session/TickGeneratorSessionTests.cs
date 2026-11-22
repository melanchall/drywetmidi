using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Runtime.CompilerServices;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed class TickGeneratorSessionTests
    {
        #region Delegates

        private delegate IntPtr CreateSessionHandleDelegate();

        #endregion

        #region Test methods

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
