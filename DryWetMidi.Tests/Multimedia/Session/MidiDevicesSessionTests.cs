using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Configuration;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Runtime.CompilerServices;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed class MidiDevicesSessionTests
    {
        #region Delegates

        private delegate IntPtr CreateSessionHandleDelegate(
            MidiDevicesSessionApi.InputEndpointCallback inputEndpointCallback,
            MidiDevicesSessionApi.OutputEndpointCallback outputEndpointCallback,
            int waitAfterSessionCreatedMs);

        #endregion

        #region Test methods

#if TEST
        [Test]
        public void CheckMidiDevicesSession_DisposeManually([Values(0, 50, 5000)] int waitAfterSessionCreatedMs)
        {
            MidiDevicesSessionApi.InputEndpointCallback inputEndpointCallback = InputEndpointCallback;
            MidiDevicesSessionApi.OutputEndpointCallback outputEndpointCallback = OutputEndpointCallback;

            CheckMidiDevicesSession_DisposeManually(CreateSessionHandle, inputEndpointCallback, outputEndpointCallback, waitAfterSessionCreatedMs);
        }

        [Test]
        public void CheckMidiDevicesSession_AbandonAndWaitForFinalizer([Values(0, 50, 5000)] int waitAfterSessionCreatedMs)
        {
            MidiDevicesSessionApi.InputEndpointCallback inputEndpointCallback = InputEndpointCallback;
            MidiDevicesSessionApi.OutputEndpointCallback outputEndpointCallback = OutputEndpointCallback;

            CheckMidiDevicesSession_AbandonAndWaitForFinalizer(CreateSessionHandle, inputEndpointCallback, outputEndpointCallback, waitAfterSessionCreatedMs);
        }

        [Test]
        public void CheckMidiDevicesSession_CloseViaApi([Values(0, 50, 5000)] int waitAfterSessionCreatedMs)
        {
            MidiDevicesSessionApi.InputEndpointCallback inputEndpointCallback = InputEndpointCallback;
            MidiDevicesSessionApi.OutputEndpointCallback outputEndpointCallback = OutputEndpointCallback;

            var sessionHandle = CreateSessionHandle(inputEndpointCallback, outputEndpointCallback, waitAfterSessionCreatedMs);
            var result = MidiDevicesSessionApi.Api_CloseSession(sessionHandle);
            ClassicAssert.AreEqual(
                MidiDevicesSessionApi.SESSION_CLOSERESULT.SESSION_CLOSERESULT_OK,
                result,
                "Session was not closed successfully.");
        }

        [Test]
        public void CheckMidiDevicesSession_SessionAndConfigurationDisposeOrder(
            [Values(0, 50, 5000)] int waitAfterConfigurationCreatedMs,
            [Values(0, 50, 5000)] int waitAfterSessionCreatedMs,
            [Values] bool disposeConfigurationFirst)
        {
            var checkpoints = new TestCheckpoints();

            //

            MidiConfigurationApi.NativeApiActivityCallback nativeApiActivityCallback = NativeApiActivityCallback;

            var getConfigurationResult = MidiConfigurationApi.Api_GetConfiguration(
                true,
                nativeApiActivityCallback,
                out var configurationRawHandle,
                out _);

            ClassicAssert.AreEqual(
                MidiConfigurationApi.CONFIGURATION_GETRESULT.CONFIGURATION_GETRESULT_OK,
                getConfigurationResult,
                "Configuration was not retrieved successfully.");

            if (waitAfterConfigurationCreatedMs > 0)
                WaitOperations.Wait(waitAfterConfigurationCreatedMs);

            var configurationHandle = new MidiConfigurationHandle(configurationRawHandle)
            {
                TestCheckpoints = checkpoints
            };

            //

            MidiDevicesSessionApi.InputEndpointCallback inputEndpointCallback = InputEndpointCallback;
            MidiDevicesSessionApi.OutputEndpointCallback outputEndpointCallback = OutputEndpointCallback;

            var openSessionResult = MidiDevicesSessionApi.Api_OpenSession(
                Guid.NewGuid().ToString(),
                configurationHandle,
                inputEndpointCallback,
                outputEndpointCallback,
                out var sessionRawHandle,
                out _);

            ClassicAssert.AreEqual(
                MidiDevicesSessionApi.SESSION_OPENRESULT.SESSION_OPENRESULT_OK,
                openSessionResult,
                "Session was not opened successfully.");

            if (waitAfterSessionCreatedMs > 0)
                WaitOperations.Wait(waitAfterSessionCreatedMs);

            var sessionHandle = new MidiDevicesSessionHandle(sessionRawHandle)
            {
                TestCheckpoints = checkpoints
            };

            //

            checkpoints.CheckCheckpointsAreNotReached(
                MidiDevicesSessionCheckpointNames.ReleaseHandleEntered,
                MidiDevicesSessionCheckpointNames.CloseSessionInReleaseHandle,
                MidiConfigurationCheckpointNames.ReleaseHandleEntered,
                MidiConfigurationCheckpointNames.CleanupConfigurationInReleaseHandle);

            //

            Action disposeConfiguration = () =>
            {
                configurationHandle.Dispose();
                checkpoints.CheckCheckpointsReached(
                    MidiConfigurationCheckpointNames.ReleaseHandleEntered,
                    MidiConfigurationCheckpointNames.CleanupConfigurationInReleaseHandle);
                ClassicAssert.IsTrue(configurationHandle.IsClosed, "Configuration handle is not closed after disposing.");
            };

            Action disposeSession = () =>
            {
                sessionHandle.Dispose();
                checkpoints.CheckCheckpointsReached(
                    MidiDevicesSessionCheckpointNames.ReleaseHandleEntered,
                    MidiDevicesSessionCheckpointNames.CloseSessionInReleaseHandle);
                ClassicAssert.IsTrue(sessionHandle.IsClosed, "Session handle is not closed after disposing.");
            };

            if (disposeConfigurationFirst)
            {
                disposeConfiguration();
                disposeSession();
            }
            else
            {
                disposeSession();
                disposeConfiguration();
            }
        }
#endif

        #endregion

        #region Private methods

#if TEST
        private void CheckMidiDevicesSession_DisposeManually(
            CreateSessionHandleDelegate createSessionHandle,
            MidiDevicesSessionApi.InputEndpointCallback inputEndpointCallback,
            MidiDevicesSessionApi.OutputEndpointCallback outputEndpointCallback,
            int waitAfterSessionCreatedMs)
        {
            var checkpoints = new TestCheckpoints();

            var rawHandle = createSessionHandle(inputEndpointCallback, outputEndpointCallback, waitAfterSessionCreatedMs);
            var handle = new MidiDevicesSessionHandle(rawHandle);
            handle.TestCheckpoints = checkpoints;

            checkpoints.CheckCheckpointsAreNotReached(
                MidiDevicesSessionCheckpointNames.ReleaseHandleEntered,
                MidiDevicesSessionCheckpointNames.CloseSessionInReleaseHandle);

            handle.Dispose();

            checkpoints.CheckCheckpointsReached(
                MidiDevicesSessionCheckpointNames.ReleaseHandleEntered,
                MidiDevicesSessionCheckpointNames.CloseSessionInReleaseHandle);
            
            ClassicAssert.IsTrue(handle.IsClosed, "Handle is not closed after disposing.");
        }

        private void CheckMidiDevicesSession_AbandonAndWaitForFinalizer(
            CreateSessionHandleDelegate createSessionHandle,
            MidiDevicesSessionApi.InputEndpointCallback inputEndpointCallback,
            MidiDevicesSessionApi.OutputEndpointCallback outputEndpointCallback,
            int waitAfterSessionCreatedMs)
        {
            var checkpoints = new TestCheckpoints();

            CreateAndAbandonMidiDevicesSession(
                checkpoints,
                createSessionHandle,
                inputEndpointCallback,
                outputEndpointCallback,
                waitAfterSessionCreatedMs);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            checkpoints.CheckCheckpointsReached(
                MidiDevicesSessionCheckpointNames.ReleaseHandleEntered,
                MidiDevicesSessionCheckpointNames.CloseSessionInReleaseHandle);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void CreateAndAbandonMidiDevicesSession(
            TestCheckpoints checkpoints,
            CreateSessionHandleDelegate createSessionHandle,
            MidiDevicesSessionApi.InputEndpointCallback inputEndpointCallback,
            MidiDevicesSessionApi.OutputEndpointCallback outputEndpointCallback,
            int waitAfterSessionCreatedMs)
        {
            var rawHandle = createSessionHandle(inputEndpointCallback, outputEndpointCallback, waitAfterSessionCreatedMs);
            var handle = new MidiDevicesSessionHandle(rawHandle);
            handle.TestCheckpoints = checkpoints;

            // Don't dispose - let it go out of scope
            // Finalizer will run eventually
        }
#endif

        private static IntPtr CreateSessionHandle(
            MidiDevicesSessionApi.InputEndpointCallback inputEndpointCallback,
            MidiDevicesSessionApi.OutputEndpointCallback outputEndpointCallback,
            int waitAfterSessionCreatedMs)
        {
            var openSessionResult = MidiDevicesSessionApi.Api_OpenSession(
                Guid.NewGuid().ToString(),
                MidiConfiguration.GetConfigurationHandle(),
                inputEndpointCallback,
                outputEndpointCallback,
                out var sessionRawHandle,
                out _);

            ClassicAssert.AreEqual(
                MidiDevicesSessionApi.SESSION_OPENRESULT.SESSION_OPENRESULT_OK,
                openSessionResult,
                "Session was not opened successfully.");

            if (waitAfterSessionCreatedMs > 0)
                WaitOperations.Wait(waitAfterSessionCreatedMs);

            return sessionRawHandle;
        }

        private static void InputEndpointCallback(IntPtr info, MidiDevicesSessionApi.SESSION_CALLBACKOPERATION operation)
        {
        }

        private static void OutputEndpointCallback(IntPtr info, MidiDevicesSessionApi.SESSION_CALLBACKOPERATION operation)
        {
        }

        private static void NativeApiActivityCallback(IntPtr record)
        {
        }

        #endregion
    }
}
