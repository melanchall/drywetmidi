using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class OutputEndpointHandle : EndpointHandle
    {
        public OutputEndpointHandle()
            : base()
        {
        }

        public OutputEndpointHandle(IntPtr infoHandle)
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
                    TestCheckpoints?.SetCheckpointReached(OutputEndpointCheckpointsNames.ReleaseHandleEntered);
#endif

                    var closeResult = OutputEndpointApi.Api_CloseEndpoint(OpenedEndpointHandle, out var errorCode);
                    closed = closeResult == OutputEndpointApi.OUT_CLOSERESULT.OUT_CLOSERESULT_OK;

#if TEST
                    TestCheckpoints?.SetCheckpointReached(OutputEndpointCheckpointsNames.CloseEndpointExecutedInReleaseHandle);

                    if (closed)
                        TestCheckpoints?.SetCheckpointReached(OutputEndpointCheckpointsNames.CloseEndpointSuccessInReleaseHandle);
                    else
                        TestCheckpoints?.SetErrorReached($"Failed to close output endpoint: {closeResult} ({errorCode}).");
#endif

                    OpenedEndpointHandle = IntPtr.Zero;
                }

#if TEST
                TestCheckpoints?.SetCheckpointReached(OutputEndpointCheckpointsNames.ReleaseInfoHandleEntered);
#endif

                OutputEndpointApi.Api_DeleteEndpointInfo(handle);

#if TEST
                TestCheckpoints?.SetCheckpointReached(OutputEndpointCheckpointsNames.InfoDeletedInReleaseInfoHandle);
#endif
            }

            return closed;
        }
    }
}
