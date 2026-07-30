using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class InputEndpointHandle : EndpointHandle
    {
        public InputEndpointHandle()
            : base()
        {
        }

        public InputEndpointHandle(IntPtr infoHandle)
            : base(infoHandle)
        {
        }

        protected override bool ReleaseHandle()
        {
            var closed = true;

            lock (Lock)
            {
                if (OpenedEndpointHandle != IntPtr.Zero)
                {
#if TEST
                    TestCheckpoints?.SetCheckpointReached(InputEndpointCheckpointsNames.ReleaseHandleEntered);
#endif

                    var disconnectResult = InputEndpointApi.Api_Disconnect(OpenedEndpointHandle, out var errorCode);
                    var disconnected = disconnectResult == InputEndpointApi.IN_DISCONNECTRESULT.IN_DISCONNECTRESULT_OK;

#if TEST
                    TestCheckpoints?.SetCheckpointReached(InputEndpointCheckpointsNames.DisconnectEndpointExecutedInReleaseHandle);

                    if (disconnected)
                        TestCheckpoints?.SetCheckpointReached(InputEndpointCheckpointsNames.DisconnectEndpointSuccessInReleaseHandle);
                    else
                        TestCheckpoints?.SetErrorReached($"Failed to disconnect input endpoint: {disconnectResult} ({errorCode}).");
#endif

                    var closeResult = InputEndpointApi.Api_CloseEndpoint(OpenedEndpointHandle, out errorCode);
                    closed = closeResult == InputEndpointApi.IN_CLOSERESULT.IN_CLOSERESULT_OK && disconnected;

#if TEST
                    TestCheckpoints?.SetCheckpointReached(InputEndpointCheckpointsNames.CloseEndpointExecutedInReleaseHandle);

                    if (closed)
                        TestCheckpoints?.SetCheckpointReached(InputEndpointCheckpointsNames.CloseEndpointSuccessInReleaseHandle);
                    else
                        TestCheckpoints?.SetErrorReached($"Failed to close input endpoint: {closeResult} ({errorCode}).");
#endif
                }

#if TEST
                TestCheckpoints?.SetCheckpointReached(InputEndpointCheckpointsNames.ReleaseInfoHandleEntered);
#endif

                InputEndpointApi.Api_DeleteEndpointInfo(handle);

#if TEST
                TestCheckpoints?.SetCheckpointReached(InputEndpointCheckpointsNames.InfoDeletedInReleaseInfoHandle);
#endif
            }

            return closed;
        }
    }
}
