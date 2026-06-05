using System;
using System.Runtime.InteropServices;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Configuration;

#if NET7_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace Melanchall.DryWetMidi.Multimedia
{
    internal static partial class DeviceApi
    {
        #region Nested enums

        public enum DEVCOMMON_GETPARENTDEVICEINFORESULT
        {
            DEVCOMMON_GETPARENTDEVICEINFORESULT_OK = 0,
            DEVCOMMON_GETPARENTDEVICEINFORESULT_NOINFO = 1,
            
            DEVCOMMON_GETPARENTDEVICEINFORESULT_FAILEDTOGETINFO = 100,
            DEVCOMMON_GETPARENTDEVICEINFORESULT_UNKNOWNWMSERROR = 101,

            DEVCOMMON_GETPARENTDEVICEINFORESULT_FAILEDGETID = 1000,
            DEVCOMMON_GETPARENTDEVICEINFORESULT_NAME_FAILEDGETVALUE = 1001,
            DEVCOMMON_GETPARENTDEVICEINFORESULT_NAME_FAILEDFILLVALUEBUFFER = 1002,
            DEVCOMMON_GETPARENTDEVICEINFORESULT_MANUFACTURER_FAILEDGETVALUE = 1003,
            DEVCOMMON_GETPARENTDEVICEINFORESULT_MANUFACTURER_FAILEDFILLVALUEBUFFER = 1004,
            DEVCOMMON_GETPARENTDEVICEINFORESULT_MODEL_FAILEDGETVALUE = 1005,
            DEVCOMMON_GETPARENTDEVICEINFORESULT_MODEL_FAILEDFILLVALUEBUFFER = 1006
        }

        #endregion

        #region Extern functions

#if NET7_0_OR_GREATER
        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial DEVCOMMON_GETPARENTDEVICEINFORESULT GetParentDeviceInfo_Win(IntPtr info, MidiConfigurationHandle configuration, out IntPtr id, out IntPtr name, out IntPtr manufacturer, out IntPtr model, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial DEVCOMMON_GETPARENTDEVICEINFORESULT GetParentDeviceInfo_Mac(IntPtr info, MidiConfigurationHandle configuration, out int id, out IntPtr name, out IntPtr manufacturer, out IntPtr model, out int errorCode);
#else
        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern DEVCOMMON_GETPARENTDEVICEINFORESULT GetParentDeviceInfo_Win(IntPtr info, MidiConfigurationHandle configuration, out IntPtr id, out IntPtr name, out IntPtr manufacturer, out IntPtr model, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern DEVCOMMON_GETPARENTDEVICEINFORESULT GetParentDeviceInfo_Mac(IntPtr info, MidiConfigurationHandle configuration, out int id, out IntPtr name, out IntPtr manufacturer, out IntPtr model, out int errorCode);
#endif

        #endregion

        #region Methods

        public static bool Api_GetParentDeviceInfo(IntPtr info, MidiConfigurationHandle configuration, out string id, out string name, out string manufacturer, out string model)
        {
            id = null;
            name = null;
            manufacturer = null;
            model = null;

            var apiType = CommonApi.Api_GetApiType();
            Func<IntPtr, MidiConfigurationHandle, (DEVCOMMON_GETPARENTDEVICEINFORESULT, string, IntPtr, IntPtr, IntPtr, int)> getInfo = apiType switch
            {
                CommonApi.API_TYPE.API_TYPE_WIN => GetParentDeviceInfo_Win,
                CommonApi.API_TYPE.API_TYPE_MAC => GetParentDeviceInfo_Mac,
                _ => throw new NotSupportedException($"Unsupported API type: {apiType}.")
            };

            var (getInfoResult, idValue, namePointer, manufacturerPointer, modelPointer, errorCode) = getInfo(info, configuration);
            if (getInfoResult != DEVCOMMON_GETPARENTDEVICEINFORESULT.DEVCOMMON_GETPARENTDEVICEINFORESULT_OK &&
                getInfoResult != DEVCOMMON_GETPARENTDEVICEINFORESULT.DEVCOMMON_GETPARENTDEVICEINFORESULT_NOINFO)
                NativeApiUtilities.HandleEndpointNativeApiResult(getInfoResult, errorCode);

            if (getInfoResult != DEVCOMMON_GETPARENTDEVICEINFORESULT.DEVCOMMON_GETPARENTDEVICEINFORESULT_OK)
                return false;

            id = idValue;
            name = NativeApi.GetStringFromPointer(namePointer);
            manufacturer = NativeApi.GetStringFromPointer(manufacturerPointer);
            model = NativeApi.GetStringFromPointer(modelPointer);

            return true;
        }

        private static (DEVCOMMON_GETPARENTDEVICEINFORESULT, string, IntPtr, IntPtr, IntPtr, int) GetParentDeviceInfo_Win(IntPtr info, MidiConfigurationHandle configuration)
        {
            var result = GetParentDeviceInfo_Win(info, configuration, out var idPointer, out var namePointer, out var manufacturerPointer, out var modelPointer, out var errorCode);
            return (result, NativeApi.GetStringFromPointer(idPointer), namePointer, manufacturerPointer, modelPointer, errorCode);
        }

        private static (DEVCOMMON_GETPARENTDEVICEINFORESULT, string, IntPtr, IntPtr, IntPtr, int) GetParentDeviceInfo_Mac(IntPtr info, MidiConfigurationHandle configuration)
        {
            var result = GetParentDeviceInfo_Mac(info, configuration, out var id, out var namePointer, out var manufacturerPointer, out var modelPointer, out var errorCode);
            return (result, id.ToString(), namePointer, manufacturerPointer, modelPointer, errorCode);
        }

        #endregion
    }
}
