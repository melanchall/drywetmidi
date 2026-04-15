using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class InputDeviceInfo : NativeHandle
    {
        public InputDeviceInfo()
            : base()
        {
        }

        public InputDeviceInfo(IntPtr handle)
            : base(handle)
        {
        }

        protected override bool ReleaseHandle()
        {
            lock (Lock)
            {
#if TEST
                TestCheckpoints?.SetCheckpointReached(InputDeviceCheckpointsNames.ReleaseInfoHandleEntered);
#endif

                InputDeviceApi.Api_DeleteDeviceInfo(handle);

#if TEST
                TestCheckpoints?.SetCheckpointReached(InputDeviceCheckpointsNames.InfoDeletedInReleaseInfoHandle);
#endif

                return true;
            }
        }
    }
}
