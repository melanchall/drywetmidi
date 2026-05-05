using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Configuration
{
    internal sealed class MidiConfigurationHandle : NativeHandle
    {
        public MidiConfigurationHandle()
            : base()
        {
        }

        public MidiConfigurationHandle(IntPtr handle)
            : base(handle)
        {
        }

        protected override bool ReleaseHandle()
        {
#if TEST
            TestCheckpoints?.SetCheckpointReached(MidiConfigurationCheckpointNames.ReleaseHandleEntered);
#endif

            var result = MidiConfigurationApi.Api_CleanupConfiguration(handle);

#if TEST
            if (result != MidiConfigurationApi.CONFIGURATION_CLEANUPRESULT.CONFIGURATION_CLEANUPRESULT_OK)
                return false;

            TestCheckpoints?.SetCheckpointReached(MidiConfigurationCheckpointNames.CleanupConfigurationInReleaseHandle);
#endif

            return true;
        }
    }
}
