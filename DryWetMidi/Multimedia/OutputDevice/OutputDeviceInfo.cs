using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class OutputDeviceInfo : NativeHandle
    {
        public OutputDeviceInfo()
            : base()
        {
        }

        public OutputDeviceInfo(IntPtr handle)
            : base(handle)
        {
        }

        protected override bool ReleaseHandle()
        {
            lock (Lock)
            {
#if TEST
                TestCheckpoints?.SetCheckpointReached(OutputDeviceCheckpointsNames.ReleaseInfoHandleEntered);
#endif

                OutputDeviceApi.Api_DeleteDeviceInfo(handle);

#if TEST
                TestCheckpoints?.SetCheckpointReached(OutputDeviceCheckpointsNames.InfoDeletedInReleaseInfoHandle);
#endif

                return true;
            }
        }
    }
}
