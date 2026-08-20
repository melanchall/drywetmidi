using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Configuration;
using System;
using System.Runtime.InteropServices;

#if NET7_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace Melanchall.DryWetMidi.Multimedia
{
    internal static partial class DeviceApi
    {
        #region Nested enums

        public enum DEVICE_GETDEVICEINFORESULT
        {
            DEVICE_GETDEVICEINFORESULT_OK = 0,

            DEVICE_GETDEVICEINFORESULT_FAILEDGETENDPOINTINFO = 1,
            DEVICE_GETDEVICEINFORESULT_FAILEDGETPARENTDEVICEINFO = 2,
            DEVICE_GETDEVICEINFORESULT_UNKNOWNWMSERROR = 3,
            DEVICE_GETDEVICEINFORESULT_FAILEDPREPAREDEVICEINFO = 4,
            DEVICE_GETDEVICEINFORESULT_FAILEDGETDEVICEINFO = 5,
            DEVICE_GETDEVICEINFORESULT_FAILEDPREPAREPARENTDEVICEINFO = 6,

            DEVICE_GETDEVICEINFORESULT_FAILEDGETENTITY = 101,
            DEVICE_GETDEVICEINFORESULT_FAILEDGETDEVICE = 102,
            DEVICE_GETDEVICEINFORESULT_FAILEDGETID = 103,
            
            DEVICE_GETDEVICEINFORESULT_NAME_UNAVAILABLE = 104,
            DEVICE_GETDEVICEINFORESULT_NAME_FAILEDGETVALUE = 105,
            DEVICE_GETDEVICEINFORESULT_NAME_FAILEDFILLVALUEBUFFER = 106,
        }

        #endregion

        #region Extern functions

#if NET7_0_OR_GREATER
        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial DEVICE_GETDEVICEINFORESULT GetDeviceInformation(IntPtr info, MidiConfigurationHandle configuration, out IntPtr id, out IntPtr name, out IntPtr manufacturer, out IntPtr model, out IntPtr driverVersion, out int errorCode);
#else
        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern DEVICE_GETDEVICEINFORESULT GetDeviceInformation(IntPtr info, MidiConfigurationHandle configuration, out IntPtr id, out IntPtr name, out IntPtr manufacturer, out IntPtr model, out IntPtr driverVersion, out int errorCode);
#endif

        #endregion

        #region Methods

        public static DEVICE_GETDEVICEINFORESULT Api_GetDeviceInformation(IntPtr info, MidiConfigurationHandle configuration, out string? id, out string? name, out string? manufacturer, out string? model, out string? deviceDriver, out int errorCode)
        {
            id = null;
            name = null;
            manufacturer = null;
            model = null;
            deviceDriver = null;

            var result = GetDeviceInformation(info, configuration, out var idPointer, out var namePointer, out var manufacturerPointer, out var modelPointer, out var driverVersionPointer, out errorCode);
            if (result == DEVICE_GETDEVICEINFORESULT.DEVICE_GETDEVICEINFORESULT_OK)
            {

                id = NativeApi.GetStringFromPointer(idPointer);
                name = NativeApi.GetStringFromPointer(namePointer);
                manufacturer = NativeApi.GetStringFromPointer(manufacturerPointer);
                model = NativeApi.GetStringFromPointer(modelPointer);
                deviceDriver = NativeApi.GetStringFromPointer(driverVersionPointer);
            }

            return result;
        }

        #endregion
    }
}
