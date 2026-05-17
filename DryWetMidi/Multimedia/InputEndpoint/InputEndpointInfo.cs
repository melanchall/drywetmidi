using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class InputEndpointInfo : NativeHandle
    {
        public InputEndpointInfo()
            : base()
        {
        }

        public InputEndpointInfo(IntPtr handle)
            : base(handle)
        {
        }

        protected override bool ReleaseHandle()
        {
            lock (Lock)
            {
#if TEST
                TestCheckpoints?.SetCheckpointReached(InputEndpointCheckpointsNames.ReleaseInfoHandleEntered);
#endif

                InputEndpointApi.Api_DeleteDeviceInfo(handle);

#if TEST
                TestCheckpoints?.SetCheckpointReached(InputEndpointCheckpointsNames.InfoDeletedInReleaseInfoHandle);
#endif

                return true;
            }
        }
    }
}
