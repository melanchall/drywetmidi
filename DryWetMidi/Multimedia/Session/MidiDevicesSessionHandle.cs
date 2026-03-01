using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class MidiDevicesSessionHandle : NativeHandle
    {
        public MidiDevicesSessionHandle()
            : base()
        {
        }

        public MidiDevicesSessionHandle(IntPtr handle)
            : base(handle)
        {
        }

        protected override bool ReleaseHandle()
        {
#if TEST
            TestCheckpoints?.SetCheckpointReached(MidiDevicesSessionCheckpointNames.ReleaseHandleEntered);
#endif

            var result = MidiDevicesSessionApi.Api_CloseSession(handle);

#if TEST
            if (result != MidiDevicesSessionApi.SESSION_CLOSERESULT.SESSION_CLOSERESULT_OK)
                return false;

            TestCheckpoints?.SetCheckpointReached(MidiDevicesSessionCheckpointNames.CloseSessionInReleaseHandle);
#endif

            return true;
        }
    }
}
