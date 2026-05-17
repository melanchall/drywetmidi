using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class OutputEndpointInfo : NativeHandle
    {
        public OutputEndpointInfo()
            : base()
        {
        }

        public OutputEndpointInfo(IntPtr handle)
            : base(handle)
        {
        }

        protected override bool ReleaseHandle()
        {
            lock (Lock)
            {
#if TEST
                TestCheckpoints?.SetCheckpointReached(OutputEndpointCheckpointsNames.ReleaseInfoHandleEntered);
#endif

                OutputEndpointApi.Api_DeleteDeviceInfo(handle);

#if TEST
                TestCheckpoints?.SetCheckpointReached(OutputEndpointCheckpointsNames.InfoDeletedInReleaseInfoHandle);
#endif

                return true;
            }
        }
    }
}
