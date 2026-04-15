using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class InputDeviceHandle : NativeHandle
    {
        public InputDeviceHandle()
            : base()
        {
        }

        public InputDeviceHandle(IntPtr handle)
            : base(handle)
        {
        }

        protected override bool ReleaseHandle()
        {
            lock (Lock)
            {
#if TEST
                TestCheckpoints?.SetCheckpointReached(InputDeviceCheckpointsNames.ReleaseHandleEntered);
#endif

                var disconnectResult = InputDeviceApi.Api_Disconnect(handle, out var errorCode);
                var disconnected = disconnectResult == InputDeviceApi.IN_DISCONNECTRESULT.IN_DISCONNECTRESULT_OK;

#if TEST
                TestCheckpoints?.SetCheckpointReached(InputDeviceCheckpointsNames.DisconnectDeviceExecutedInReleaseHandle);

                if (disconnected)
                    TestCheckpoints?.SetCheckpointReached(InputDeviceCheckpointsNames.DisconnectDeviceSuccessInReleaseHandle);
                else
                    TestCheckpoints?.SetErrorReached($"Failed to disconnect input device: {disconnectResult} ({errorCode}).");
#endif

                var closeResult = InputDeviceApi.Api_CloseDevice(handle, out errorCode);
                var closed = closeResult == InputDeviceApi.IN_CLOSERESULT.IN_CLOSERESULT_OK;

#if TEST
                TestCheckpoints?.SetCheckpointReached(InputDeviceCheckpointsNames.CloseDeviceExecutedInReleaseHandle);

                if (closed)
                    TestCheckpoints?.SetCheckpointReached(InputDeviceCheckpointsNames.CloseDeviceSuccessInReleaseHandle);
                else
                    TestCheckpoints?.SetErrorReached($"Failed to close input device: {closeResult} ({errorCode}).");
#endif

                return closed && disconnected;
            }
        }
    }
}
