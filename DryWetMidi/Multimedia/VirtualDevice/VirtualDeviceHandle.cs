using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Configuration;
using System;
using System.Threading;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class VirtualDeviceHandle : NativeHandle
    {
        private readonly MidiConfigurationHandle? _configuration;
        private int _configurationReferenceAdded;

        public VirtualDeviceHandle()
            : base()
        {
        }

        public VirtualDeviceHandle(IntPtr handle, MidiConfigurationHandle configuration)
            : base(handle)
        {
            _configuration = configuration;
            _configurationReferenceAdded = 1;
        }

        public void ReleaseConfigurationReference()
        {
            if (Interlocked.Exchange(ref _configurationReferenceAdded, 0) != 0)
                _configuration!.DangerousRelease();
        }

        protected override bool ReleaseHandle()
        {
#if TEST
            TestCheckpoints?.SetCheckpointReached(VirtualDeviceCheckpointsNames.ReleaseHandleEntered);
#endif

            var closeResult = VirtualDeviceApi.VIRTUAL_CLOSERESULT.VIRTUAL_CLOSERESULT_FAILED;
            var errorCode = 0;
            var closed = false;

            try
            {
                closeResult = VirtualDeviceApi.Api_CloseDevice(handle, out errorCode);
                closed = closeResult == VirtualDeviceApi.VIRTUAL_CLOSERESULT.VIRTUAL_CLOSERESULT_OK;

                if (!closed && CommonApi.Api_GetOsType() == CommonApi.OsType.Windows)
                    VirtualDeviceApi.Api_AbandonDevice_Win(handle);
            }
            finally
            {
                ReleaseConfigurationReference();
            }

#if TEST
            TestCheckpoints?.SetCheckpointReached(VirtualDeviceCheckpointsNames.CloseDeviceExecutedInReleaseHandle);

            if (closed)
                TestCheckpoints?.SetCheckpointReached(VirtualDeviceCheckpointsNames.CloseDeviceSuccessInReleaseHandle);
            else
                TestCheckpoints?.SetErrorReached($"Failed to close virtual device: {closeResult} ({errorCode}).");
#endif

            return closed;
        }
    }
}
