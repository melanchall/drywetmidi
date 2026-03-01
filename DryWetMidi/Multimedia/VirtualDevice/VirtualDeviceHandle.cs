using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class VirtualDeviceHandle : NativeHandle
    {
        public VirtualDeviceHandle()
            : base()
        {
        }

        public VirtualDeviceHandle(IntPtr handle)
            : base(handle)
        {
        }

        protected override bool ReleaseHandle()
        {
#if TEST
            TestCheckpoints?.SetCheckpointReached(VirtualDeviceCheckpointsNames.ReleaseHandleEntered);
#endif

            var closeResult = VirtualDeviceApi.Api_CloseDevice(handle, out var errorCode);
            var closed = closeResult == VirtualDeviceApi.VIRTUAL_CLOSERESULT.VIRTUAL_CLOSERESULT_OK;

#if TEST
            TestCheckpoints?.SetCheckpointReached(VirtualDeviceCheckpointsNames.CloseDeviceExecutedInReleaseHandle);

            if (closed)
                TestCheckpoints?.SetCheckpointReached(VirtualDeviceCheckpointsNames.CloseDeviceSuccessInReleaseHandle);
#endif

            return closed;
        }
    }
}
