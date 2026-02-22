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
            TestCheckpoints?.SetCheckpointReached(VirtualDeviceCheckpointsNames.HandleFinalizerEntered);
#endif

            var closeResult = VirtualDeviceApi.Api_CloseDevice(handle, out var errorCode);
            if (closeResult != VirtualDeviceApi.VIRTUAL_CLOSERESULT.VIRTUAL_CLOSERESULT_OK)
                return false;

#if TEST
            TestCheckpoints?.SetCheckpointReached(VirtualDeviceCheckpointsNames.DeviceClosedInHandleFinalizer);
#endif

            return true;
        }
    }
}
