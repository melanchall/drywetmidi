using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed class MidiDevicesSessionTests
    {
        #region Delegates

        private delegate IntPtr CreateSessionHandleDelegate(
            MidiDevicesSessionApi.InputDeviceCallback inputDeviceCallback,
            MidiDevicesSessionApi.OutputDeviceCallback outputDeviceCallback);

        #endregion

        #region Test methods

#if TEST
        [Test]
        [Platform("Win")]
        public void CheckMidiDevicesSession_DisposeManually_Win() =>
            CheckMidiDevicesSession_DisposeManually(CreateSessionHandle_Win, null, null);

        [Test]
        [Platform("MacOsX")]
        public void CheckMidiDevicesSession_DisposeManually_Mac()
        {
            MidiDevicesSessionApi.InputDeviceCallback inputDeviceCallback = InputDeviceCallback;
            MidiDevicesSessionApi.OutputDeviceCallback outputDeviceCallback = OutputDeviceCallback;

            CheckMidiDevicesSession_DisposeManually(CreateSessionHandle_Mac, inputDeviceCallback, outputDeviceCallback);
        }

        [Test]
        [Platform("Win")]
        public void CheckMidiDevicesSession_AbandonAndWaitForFinalizer_Win() =>
            CheckMidiDevicesSession_AbandonAndWaitForFinalizer(CreateSessionHandle_Win, null, null);

        [Test]
        [Platform("MacOsX")]
        public void CheckMidiDevicesSession_AbandonAndWaitForFinalizer_Mac()
        {
            MidiDevicesSessionApi.InputDeviceCallback inputDeviceCallback = InputDeviceCallback;
            MidiDevicesSessionApi.OutputDeviceCallback outputDeviceCallback = OutputDeviceCallback;

            CheckMidiDevicesSession_AbandonAndWaitForFinalizer(CreateSessionHandle_Mac, inputDeviceCallback, outputDeviceCallback);
        }

        [Test]
        [Platform("Win")]
        public void CheckMidiDevicesSession_CloseViaApi_Win()
        {
            var sessionHandle = CreateSessionHandle_Win(null, null);
            var result = MidiDevicesSessionApi.Api_CloseSession(sessionHandle);
            ClassicAssert.AreEqual(
                MidiDevicesSessionApi.SESSION_CLOSERESULT.SESSION_CLOSERESULT_OK,
                result,
                "Session was not closed successfully.");
        }

        [Test]
        [Platform("MacOsX")]
        public void CheckMidiDevicesSession_CloseViaApi_Mac()
        {
            MidiDevicesSessionApi.InputDeviceCallback inputDeviceCallback = InputDeviceCallback;
            MidiDevicesSessionApi.OutputDeviceCallback outputDeviceCallback = OutputDeviceCallback;

            var sessionHandle = CreateSessionHandle_Mac(inputDeviceCallback, outputDeviceCallback);
            var result = MidiDevicesSessionApi.Api_CloseSession(sessionHandle);
            ClassicAssert.AreEqual(
                MidiDevicesSessionApi.SESSION_CLOSERESULT.SESSION_CLOSERESULT_OK,
                result,
                "Session was not closed successfully.");
        }
#endif

        #endregion

        #region Private methods

#if TEST
        private void CheckMidiDevicesSession_DisposeManually(
            CreateSessionHandleDelegate createSessionHandle,
            MidiDevicesSessionApi.InputDeviceCallback inputDeviceCallback,
            MidiDevicesSessionApi.OutputDeviceCallback outputDeviceCallback)
        {
            var checkpoints = new TestCheckpoints();

            var rawHandle = createSessionHandle(inputDeviceCallback, outputDeviceCallback);
            var handle = new MidiDevicesSessionHandle(rawHandle);
            handle.TestCheckpoints = checkpoints;

            checkpoints.CheckCheckpointsAreNotReached(
                MidiDevicesSessionCheckpointNames.HandleFinalizerEntered,
                MidiDevicesSessionCheckpointNames.SessionClosedInHandleFinalizer);

            handle.Dispose();

            checkpoints.CheckCheckpointsReached(
                MidiDevicesSessionCheckpointNames.HandleFinalizerEntered,
                MidiDevicesSessionCheckpointNames.SessionClosedInHandleFinalizer);
            
            ClassicAssert.IsTrue(handle.IsClosed, "Handle is not closed after disposing.");
        }

        private void CheckMidiDevicesSession_AbandonAndWaitForFinalizer(
            CreateSessionHandleDelegate createSessionHandle,
            MidiDevicesSessionApi.InputDeviceCallback inputDeviceCallback,
            MidiDevicesSessionApi.OutputDeviceCallback outputDeviceCallback)
        {
            var checkpoints = new TestCheckpoints();

            CreateAndAbandonMidiDevicesSession(checkpoints, createSessionHandle, inputDeviceCallback, outputDeviceCallback);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            checkpoints.CheckCheckpointsReached(
                MidiDevicesSessionCheckpointNames.HandleFinalizerEntered,
                MidiDevicesSessionCheckpointNames.SessionClosedInHandleFinalizer);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void CreateAndAbandonMidiDevicesSession(
            TestCheckpoints checkpoints,
            CreateSessionHandleDelegate createSessionHandle,
            MidiDevicesSessionApi.InputDeviceCallback inputDeviceCallback,
            MidiDevicesSessionApi.OutputDeviceCallback outputDeviceCallback)
        {
            var rawHandle = createSessionHandle(inputDeviceCallback, outputDeviceCallback);
            var handle = new MidiDevicesSessionHandle(rawHandle);
            handle.TestCheckpoints = checkpoints;

            // Don't dispose - let it go out of scope
            // Finalizer will run eventually
        }
#endif

        private static IntPtr GetSessionName()
        {
            var name = Guid.NewGuid().ToString();
            return Marshal.StringToHGlobalAuto(name);
        }

        private static IntPtr CreateSessionHandle_Win(
            MidiDevicesSessionApi.InputDeviceCallback inputDeviceCallback,
            MidiDevicesSessionApi.OutputDeviceCallback outputDeviceCallback)
        {
            MidiDevicesSessionApi.Api_OpenSession_Win(
                GetSessionName(),
                out var rawHandle,
                out var errorCode);

            return rawHandle;
        }

        private static IntPtr CreateSessionHandle_Mac(
            MidiDevicesSessionApi.InputDeviceCallback inputDeviceCallback,
            MidiDevicesSessionApi.OutputDeviceCallback outputDeviceCallback)
        {
            Console.WriteLine("Creating session...");

            MidiDevicesSessionApi.Api_OpenSession_Mac(
                GetSessionName(),
                inputDeviceCallback,
                outputDeviceCallback,
                out var rawHandle,
                out var errorCode);

            Console.WriteLine("Session has been created.");
            return rawHandle;
        }

        private static void InputDeviceCallback(IntPtr info, bool operation)
        {
        }

        private static void OutputDeviceCallback(IntPtr info, bool operation)
        {
        }

        #endregion
    }
}
