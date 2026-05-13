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
            MidiDevicesSessionApi.InputDeviceCallback inputDeviceCallback,
            MidiDevicesSessionApi.OutputDeviceCallback outputDeviceCallback,
            int waitAfterSessionCreatedMs);

        #endregion

        #region Test methods

#if TEST
        [Test]
        public void CheckMidiDevicesSession_DisposeManually([Values(0, 50, 5000)] int waitAfterSessionCreatedMs)
        {
            MidiDevicesSessionApi.InputDeviceCallback inputDeviceCallback = InputDeviceCallback;
            MidiDevicesSessionApi.OutputDeviceCallback outputDeviceCallback = OutputDeviceCallback;

            CheckMidiDevicesSession_DisposeManually(CreateSessionHandle, inputDeviceCallback, outputDeviceCallback, waitAfterSessionCreatedMs);
        }

        [Test]
        public void CheckMidiDevicesSession_AbandonAndWaitForFinalizer([Values(0, 50, 5000)] int waitAfterSessionCreatedMs)
        {
            MidiDevicesSessionApi.InputDeviceCallback inputDeviceCallback = InputDeviceCallback;
            MidiDevicesSessionApi.OutputDeviceCallback outputDeviceCallback = OutputDeviceCallback;

            CheckMidiDevicesSession_AbandonAndWaitForFinalizer(CreateSessionHandle, inputDeviceCallback, outputDeviceCallback, waitAfterSessionCreatedMs);
        }

        [Test]
        public void CheckMidiDevicesSession_CloseViaApi([Values(0, 50, 5000)] int waitAfterSessionCreatedMs)
        {
            MidiDevicesSessionApi.InputDeviceCallback inputDeviceCallback = InputDeviceCallback;
            MidiDevicesSessionApi.OutputDeviceCallback outputDeviceCallback = OutputDeviceCallback;

            var sessionHandle = CreateSessionHandle(inputDeviceCallback, outputDeviceCallback, waitAfterSessionCreatedMs);
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

            MidiDevicesSessionApi.InputDeviceCallback inputDeviceCallback = InputDeviceCallback;
            MidiDevicesSessionApi.OutputDeviceCallback outputDeviceCallback = OutputDeviceCallback;

            var openSessionResult = MidiDevicesSessionApi.Api_OpenSession(
                Guid.NewGuid().ToString(),
                configurationHandle,
                inputDeviceCallback,
                outputDeviceCallback,
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
            MidiDevicesSessionApi.InputDeviceCallback inputDeviceCallback,
            MidiDevicesSessionApi.OutputDeviceCallback outputDeviceCallback,
            int waitAfterSessionCreatedMs)
        {
            var checkpoints = new TestCheckpoints();

            var rawHandle = createSessionHandle(inputDeviceCallback, outputDeviceCallback, waitAfterSessionCreatedMs);
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
            MidiDevicesSessionApi.InputDeviceCallback inputDeviceCallback,
            MidiDevicesSessionApi.OutputDeviceCallback outputDeviceCallback,
            int waitAfterSessionCreatedMs)
        {
            var checkpoints = new TestCheckpoints();

            CreateAndAbandonMidiDevicesSession(
                checkpoints,
                createSessionHandle,
                inputDeviceCallback,
                outputDeviceCallback,
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
            MidiDevicesSessionApi.InputDeviceCallback inputDeviceCallback,
            MidiDevicesSessionApi.OutputDeviceCallback outputDeviceCallback,
            int waitAfterSessionCreatedMs)
        {
            var rawHandle = createSessionHandle(inputDeviceCallback, outputDeviceCallback, waitAfterSessionCreatedMs);
            var handle = new MidiDevicesSessionHandle(rawHandle);
            handle.TestCheckpoints = checkpoints;

            // Don't dispose - let it go out of scope
            // Finalizer will run eventually
        }
#endif

        private static IntPtr CreateSessionHandle(
            MidiDevicesSessionApi.InputDeviceCallback inputDeviceCallback,
            MidiDevicesSessionApi.OutputDeviceCallback outputDeviceCallback,
            int waitAfterSessionCreatedMs)
        {
            var openSessionResult = MidiDevicesSessionApi.Api_OpenSession(
                Guid.NewGuid().ToString(),
                MidiConfiguration.GetConfigurationHandle(),
                inputDeviceCallback,
                outputDeviceCallback,
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

        private static void InputDeviceCallback(IntPtr info, bool operation)
        {
        }

        private static void OutputDeviceCallback(IntPtr info, bool operation)
        {
        }

        private static void NativeApiActivityCallback(IntPtr record)
        {
        }

        #endregion
    }
}
