using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Melanchall.DryWetMidi.Multimedia.MidiDevicesSessionApi;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed class TickGeneratorSessionTests
    {
        #region Delegates

        private delegate IntPtr CreateSessionHandleDelegate();

        #endregion

        #region Test methods

#if TEST
        [Test]
        public void CheckTickGeneratorSession_DisposeManually()
        {
            var checkpoints = new TestCheckpoints();

            TickGeneratorSessionApi.Api_OpenSession(
                out var rawHandle,
                out var errorCode);

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
        public void CheckTickGeneratorSession_AbandonAndWaitForFinalizer()
        {
            var checkpoints = new TestCheckpoints();

            CreateAndAbandonTickGeneratorSession(checkpoints);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            checkpoints.CheckCheckpointsReached(
                MidiDevicesSessionCheckpointNames.HandleFinalizerEntered,
                MidiDevicesSessionCheckpointNames.SessionClosedInHandleFinalizer);
        }

        [Test]
        public void CheckTickGeneratorSession_CloseViaApi()
        {
            TickGeneratorSessionApi.Api_OpenSession(
                out var sessionHandle,
                out var errorCode);

            var result = TickGeneratorSessionApi.Api_CloseSession(sessionHandle, out errorCode);
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
            TestCheckpoints checkpoints)
        {
            TickGeneratorSessionApi.Api_OpenSession(
                out var rawHandle,
                out var errorCode);

            var handle = new TickGeneratorSessionHandle(rawHandle);
            handle.TestCheckpoints = checkpoints;

            // Don't dispose - let it go out of scope
            // Finalizer will run eventually
        }
#endif

        #endregion
    }
}
