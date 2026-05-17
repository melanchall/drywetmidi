using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class OutputEndpointHandle : NativeHandle
    {
        public OutputEndpointHandle()
            : base()
        {
        }

        public OutputEndpointHandle(IntPtr handle)
            : base(handle)
        {
        }

        protected override bool ReleaseHandle()
        {
#if TEST
            TestCheckpoints?.SetCheckpointReached(OutputEndpointCheckpointsNames.ReleaseHandleEntered);
#endif

            var closeResult = OutputEndpointApi.Api_CloseDevice(handle, out var errorCode);
            var closed = closeResult == OutputEndpointApi.OUT_CLOSERESULT.OUT_CLOSERESULT_OK;

#if TEST
            TestCheckpoints?.SetCheckpointReached(OutputEndpointCheckpointsNames.CloseDeviceExecutedInReleaseHandle);

            if (closed)
                TestCheckpoints?.SetCheckpointReached(OutputEndpointCheckpointsNames.CloseDeviceSuccessInReleaseHandle);
            else
                TestCheckpoints?.SetErrorReached($"Failed to close output device: {closeResult} ({errorCode}).");
#endif

            return closed;
        }
    }
}
