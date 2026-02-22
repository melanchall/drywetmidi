using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class OutputDeviceHandle : NativeHandle
    {
        public OutputDeviceHandle()
            : base()
        {
        }

        public OutputDeviceHandle(IntPtr handle)
            : base(handle)
        {
        }

        protected override bool ReleaseHandle()
        {
#if TEST
            TestCheckpoints?.SetCheckpointReached(OutputDeviceCheckpointsNames.HandleFinalizerEntered);
#endif

            var closeResult = OutputDeviceApi.Api_CloseDevice(handle, out _);
            if (closeResult != OutputDeviceApi.OUT_CLOSERESULT.OUT_CLOSERESULT_OK)
                return false;

#if TEST
            TestCheckpoints?.SetCheckpointReached(OutputDeviceCheckpointsNames.DeviceClosedInHandleFinalizer);
#endif

            return true;
        }
    }
}
