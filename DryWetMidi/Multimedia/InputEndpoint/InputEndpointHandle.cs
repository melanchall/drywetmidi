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
                TestCheckpoints?.SetCheckpointReached(InputEndpointCheckpointsNames.DisconnectEndpointExecutedInReleaseHandle);

                if (disconnected)
                    TestCheckpoints?.SetCheckpointReached(InputEndpointCheckpointsNames.DisconnectEndpointSuccessInReleaseHandle);
                else
                    TestCheckpoints?.SetErrorReached($"Failed to disconnect input endpoint: {disconnectResult} ({errorCode}).");
#endif

                var closeResult = InputEndpointApi.Api_CloseEndpoint(handle, out errorCode);
                var closed = closeResult == InputEndpointApi.IN_CLOSERESULT.IN_CLOSERESULT_OK;

#if TEST
                TestCheckpoints?.SetCheckpointReached(InputEndpointCheckpointsNames.CloseEndpointExecutedInReleaseHandle);

                if (closed)
                    TestCheckpoints?.SetCheckpointReached(InputEndpointCheckpointsNames.CloseEndpointSuccessInReleaseHandle);
                else
                    TestCheckpoints?.SetErrorReached($"Failed to close input endpoint: {closeResult} ({errorCode}).");
#endif

                return closed && disconnected;
            }
        }
    }
}
