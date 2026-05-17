using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class InputEndpointHandle : NativeHandle
    {
        public InputEndpointHandle()
            : base()
        {
        }

        public InputEndpointHandle(IntPtr handle)
            : base(handle)
        {
        }

        protected override bool ReleaseHandle()
        {
            lock (Lock)
            {
#if TEST
                TestCheckpoints?.SetCheckpointReached(InputEndpointCheckpointsNames.ReleaseHandleEntered);
#endif

                var disconnectResult = InputEndpointApi.Api_Disconnect(handle, out var errorCode);
                var disconnected = disconnectResult == InputEndpointApi.IN_DISCONNECTRESULT.IN_DISCONNECTRESULT_OK;

#if TEST
                TestCheckpoints?.SetCheckpointReached(InputEndpointCheckpointsNames.DisconnectDeviceExecutedInReleaseHandle);

                if (disconnected)
                    TestCheckpoints?.SetCheckpointReached(InputEndpointCheckpointsNames.DisconnectDeviceSuccessInReleaseHandle);
                else
                    TestCheckpoints?.SetErrorReached($"Failed to disconnect input device: {disconnectResult} ({errorCode}).");
#endif

                var closeResult = InputEndpointApi.Api_CloseDevice(handle, out errorCode);
                var closed = closeResult == InputEndpointApi.IN_CLOSERESULT.IN_CLOSERESULT_OK;

#if TEST
                TestCheckpoints?.SetCheckpointReached(InputEndpointCheckpointsNames.CloseDeviceExecutedInReleaseHandle);

                if (closed)
                    TestCheckpoints?.SetCheckpointReached(InputEndpointCheckpointsNames.CloseDeviceSuccessInReleaseHandle);
                else
                    TestCheckpoints?.SetErrorReached($"Failed to close input device: {closeResult} ({errorCode}).");
#endif

                return closed && disconnected;
            }
        }
    }
}
