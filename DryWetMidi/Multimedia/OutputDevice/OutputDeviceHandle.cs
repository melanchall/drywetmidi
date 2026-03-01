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
            TestCheckpoints?.SetCheckpointReached(OutputDeviceCheckpointsNames.ReleaseHandleEntered);
#endif

            var closeResult = OutputDeviceApi.Api_CloseDevice(handle, out _);
            var closed = closeResult == OutputDeviceApi.OUT_CLOSERESULT.OUT_CLOSERESULT_OK;

#if TEST
            TestCheckpoints?.SetCheckpointReached(OutputDeviceCheckpointsNames.CloseDeviceExecutedInReleaseHandle);

            if (closed)
                TestCheckpoints?.SetCheckpointReached(OutputDeviceCheckpointsNames.CloseDeviceSuccessInReleaseHandle);
#endif

            return closed;
        }
    }
}
