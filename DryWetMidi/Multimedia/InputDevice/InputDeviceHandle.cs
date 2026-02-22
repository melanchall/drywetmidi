using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class InputDeviceHandle : NativeHandle
    {
        public InputDeviceHandle()
            : base()
        {
        }

        public InputDeviceHandle(IntPtr handle)
            : base(handle)
        {
        }

        protected override bool ReleaseHandle()
        {
#if TEST
            TestCheckpoints?.SetCheckpointReached(InputDeviceCheckpointsNames.HandleFinalizerEntered);
#endif

            var disconnectResult = InputDeviceApi.Api_Disconnect(handle, out _);
            if (disconnectResult != InputDeviceApi.IN_DISCONNECTRESULT.IN_DISCONNECTRESULT_OK)
                return false;

#if TEST
            TestCheckpoints?.SetCheckpointReached(InputDeviceCheckpointsNames.DeviceDisconnectedInHandleFinalizer);
#endif

            var closeResult = InputDeviceApi.Api_CloseDevice(handle, out _);
            if (closeResult != InputDeviceApi.IN_CLOSERESULT.IN_CLOSERESULT_OK)
                return false;

#if TEST
            TestCheckpoints?.SetCheckpointReached(InputDeviceCheckpointsNames.DeviceClosedInHandleFinalizer);
#endif

            return true;
        }
    }
}
