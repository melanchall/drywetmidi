using System;
using System.Runtime.InteropServices;
using Melanchall.DryWetMidi.Configuration;
using Melanchall.DryWetMidi.Common;

#if NET7_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace Melanchall.DryWetMidi.Multimedia
{
    internal static partial class InputEndpointApi
    {
        #region Nested enums

        public enum IN_GETCOUNTRESULT
        {
            IN_GETCOUNTRESULT_OK = 0
        }

        public enum IN_GETALLINFORESULT
        {
            IN_GETALLINFORESULT_OK = 0,

            IN_GETALLINFORESULT_BADDEVICEID = 1,
            IN_GETALLINFORESULT_INVALIDSTRUCTURE = 2,
            IN_GETALLINFORESULT_NODRIVER = 3,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.NoMemory)]
            IN_GETALLINFORESULT_NOMEMORY = 4,
            IN_GETALLINFORESULT_UNKNOWNWMSERROR = 5,

            IN_GETALLINFORESULT_UNKNOWNERROR = 1000,
            IN_GETALLINFORESULT_UNKNOWNERRORONGETINFO = 1001,
        }

        public enum IN_OPENRESULT
        {
            IN_OPENRESULT_OK = 0,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.InUse)]
            IN_OPENRESULT_ALLOCATED = 1,
            IN_OPENRESULT_BADDEVICEID = 2,
            IN_OPENRESULT_INVALIDFLAG = 3,
            IN_OPENRESULT_INVALIDSTRUCTURE = 4,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.NoMemory)]
            IN_OPENRESULT_NOMEMORY = 5,
            IN_OPENRESULT_FAILEDPREPARESYSEXBUFFERS = 6,
            IN_OPENRESULT_INVALIDCLIENT = 101,
            IN_OPENRESULT_INVALIDPORT = 102,
            IN_OPENRESULT_WRONGTHREAD = 103,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.NotPermitted)]
            IN_OPENRESULT_NOTPERMITTED = 104,
            IN_OPENRESULT_UNKNOWNERROR = 10000
        }

        public enum IN_CLOSERESULT
        {
            IN_CLOSERESULT_OK = 0,
            IN_CLOSERESULT_RESET_INVALIDHANDLE = 1,
            IN_CLOSERESULT_RESET_UNKNOWNERROR = 3000,
            IN_CLOSERESULT_CLOSE_STILLPLAYING = 2,
            IN_CLOSERESULT_CLOSE_INVALIDHANDLE = 3,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.NoMemory)]
            IN_CLOSERESULT_CLOSE_NOMEMORY = 4,
            IN_CLOSERESULT_CLOSE_UNKNOWNERROR = 2000,
            IN_CLOSERESULT_UNPREPAREBUFFER_STILLPLAYING = 5,
            IN_CLOSERESULT_UNPREPAREBUFFER_INVALIDSTRUCTURE = 6,
            IN_CLOSERESULT_UNPREPAREBUFFER_INVALIDHANDLE = 7,
            IN_CLOSERESULT_UNPREPAREBUFFER_UNKNOWNERROR = 1000
        }

        public enum IN_RENEWSYSEXBUFFERRESULT
        {
            IN_RENEWSYSEXBUFFERRESULT_OK = 0,
            IN_RENEWSYSEXBUFFERRESULT_INVALIDHEADER = 1,
            IN_RENEWSYSEXBUFFERRESULT_BUFFERNOTDONE = 2,
            IN_RENEWSYSEXBUFFERRESULT_STILLPLAYING = 3,
            IN_RENEWSYSEXBUFFERRESULT_UNPREPARED = 4,
            IN_RENEWSYSEXBUFFERRESULT_INVALIDHANDLE = 5,
            IN_RENEWSYSEXBUFFERRESULT_INVALIDSTRUCTURE = 6,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.NoMemory)]
            IN_RENEWSYSEXBUFFERRESULT_NOMEMORY = 7,
            IN_RENEWSYSEXBUFFERRESULT_UNKNOWNERROR = 8,
            IN_RENEWSYSEXBUFFERRESULT_CLOSING = 9,
        }

        public enum IN_CONNECTRESULT
        {
            IN_CONNECTRESULT_OK = 0,
            IN_CONNECTRESULT_INVALIDHANDLE = 1,
            IN_CONNECTRESULT_UNKNOWNERROR = 101,
            IN_CONNECTRESULT_INVALIDPORT = 102,
            IN_CONNECTRESULT_WRONGTHREAD = 103,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.NotPermitted)]
            IN_CONNECTRESULT_NOTPERMITTED = 104,
            IN_CONNECTRESULT_UNKNOWNENDPOINT = 105,
            IN_CONNECTRESULT_WRONGENDPOINT = 106
        }

        public enum IN_DISCONNECTRESULT
        {
            IN_DISCONNECTRESULT_OK = 0,
            IN_DISCONNECTRESULT_INVALIDHANDLE = 1,
            IN_DISCONNECTRESULT_UNKNOWNERROR = 101,
            IN_DISCONNECTRESULT_INVALIDPORT = 102,
            IN_DISCONNECTRESULT_WRONGTHREAD = 103,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.NotPermitted)]
            IN_DISCONNECTRESULT_NOTPERMITTED = 104,
            IN_DISCONNECTRESULT_UNKNOWNENDPOINT = 105,
            IN_DISCONNECTRESULT_WRONGENDPOINT = 106,
            IN_DISCONNECTRESULT_NOCONNECTION = 107
        }

        public enum IN_GETEVENTDATARESULT
        {
            IN_GETEVENTDATARESULT_OK = 0
        }

        public enum IN_GETSYSEXDATARESULT
        {
            IN_GETSYSEXDATARESULT_OK = 0
        }

        public enum IN_GETPROPERTYRESULT
        {
            IN_GETPROPERTYRESULT_OK = 0,

            IN_GETPROPERTYRESULT_PROPERTYUNAVAILABLE = 101,
            IN_GETPROPERTYRESULT_FAILEDGETVALUE = 102,
            IN_GETPROPERTYRESULT_FAILEDFILLVALUEBUFFER = 103,
        }

        #endregion

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Callback_Win(IntPtr hMidi, NativeApi.MidiMessage wMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Callback_Mac(IntPtr pktlist, IntPtr readProcRefCon, IntPtr srcConnRefCon);

        #endregion

        #region Extern functions

#if NET7_0_OR_GREATER
        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial void CloneInputEndpointInfo(IntPtr source, out IntPtr info);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial IN_GETCOUNTRESULT GetInputEndpointsCount(out int count);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial IN_GETALLINFORESULT GetInputEndpointsInfo(MidiConfigurationHandle configuration, MidiDevicesSessionHandle sessionHandle, out IntPtr devicesInfo, out int devicesCount, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial void FreeInputEndpointsInfo(IntPtr array, int size);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial IN_GETPROPERTYRESULT GetInputEndpointName(IntPtr info, out IntPtr value, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial IN_GETPROPERTYRESULT GetInputEndpointId_Win(IntPtr info, out IntPtr value, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial IN_GETPROPERTYRESULT GetInputEndpointId_Mac(IntPtr info, out int value, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial IN_OPENRESULT OpenInputEndpoint_Win(IntPtr info, MidiDevicesSessionHandle sessionHandle, Callback_Win callback, int sysExBufferSize, int sysExBufferCount, out IntPtr handle, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial IN_OPENRESULT OpenInputEndpoint_Mac(IntPtr info, MidiDevicesSessionHandle sessionHandle, Callback_Mac callback, out IntPtr handle, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial IN_CLOSERESULT CloseInputEndpoint(IntPtr handle, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial IN_RENEWSYSEXBUFFERRESULT RenewInputEndpointSysExBuffer(IntPtr handle, IntPtr header, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial IN_CONNECTRESULT ConnectToInputEndpoint(IntPtr handle, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial IN_DISCONNECTRESULT DisconnectFromInputEndpoint(IntPtr handle, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial IN_GETEVENTDATARESULT GetEventDataFromInputEndpoint(IntPtr packetList, int packetIndex, out IntPtr data, out int length, out int packetsCount);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial IN_GETSYSEXDATARESULT GetInputEndpointSysExBufferData(IntPtr header, out IntPtr data, out int size);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial void DeleteInputEndpointInfo(IntPtr info);
#else
        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern void CloneInputEndpointInfo(IntPtr source, out IntPtr info);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IN_GETCOUNTRESULT GetInputEndpointsCount(out int count);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IN_GETALLINFORESULT GetInputEndpointsInfo(MidiConfigurationHandle configuration, MidiDevicesSessionHandle sessionHandle, out IntPtr devicesInfo, out int devicesCount, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FreeInputEndpointsInfo(IntPtr array, int size);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IN_GETPROPERTYRESULT GetInputEndpointName(IntPtr info, out IntPtr value, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IN_GETPROPERTYRESULT GetInputEndpointId_Win(IntPtr info, out IntPtr value, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IN_GETPROPERTYRESULT GetInputEndpointId_Mac(IntPtr info, out int value, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IN_OPENRESULT OpenInputEndpoint_Win(IntPtr info, MidiDevicesSessionHandle sessionHandle, Callback_Win callback, int sysExBufferSize, int sysExBufferCount, out IntPtr handle, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IN_OPENRESULT OpenInputEndpoint_Mac(IntPtr info, MidiDevicesSessionHandle sessionHandle, Callback_Mac callback, out IntPtr handle, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IN_CLOSERESULT CloseInputEndpoint(IntPtr handle, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IN_RENEWSYSEXBUFFERRESULT RenewInputEndpointSysExBuffer(IntPtr handle, IntPtr header, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IN_CONNECTRESULT ConnectToInputEndpoint(IntPtr handle, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IN_DISCONNECTRESULT DisconnectFromInputEndpoint(IntPtr handle, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IN_GETEVENTDATARESULT GetEventDataFromInputEndpoint(IntPtr packetList, int packetIndex, out IntPtr data, out int length, out int packetsCount);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IN_GETSYSEXDATARESULT GetInputEndpointSysExBufferData(IntPtr header, out IntPtr data, out int size);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DeleteInputEndpointInfo(IntPtr info);
#endif

        #endregion

        #region Methods

        public static void Api_CloneInputEndpointInfo(IntPtr source, out IntPtr info)
        {
            CloneInputEndpointInfo(source, out info);
        }

        public static IN_GETCOUNTRESULT Api_GetEndpointsCount(out int count)
        {
            return GetInputEndpointsCount(out count);
        }

        public static IN_GETALLINFORESULT Api_GetEndpointsInfo(MidiConfigurationHandle configuration, MidiDevicesSessionHandle sessionHandle, out IntPtr endpointsInfo, out int endpointsCount, out int errorCode)
        {
            return GetInputEndpointsInfo(configuration, sessionHandle, out endpointsInfo, out endpointsCount, out errorCode);
        }

        public static void Api_FreeEndpointsInfo(IntPtr array, int size)
        {
            FreeInputEndpointsInfo(array, size);
        }

        public static IN_OPENRESULT Api_OpenEndpoint_Win(IntPtr info, MidiDevicesSessionHandle sessionHandle, Callback_Win callback, int sysExBufferSize, int sysExBufferCount, out IntPtr handle, out int errorCode)
        {
            return OpenInputEndpoint_Win(info, sessionHandle, callback, sysExBufferSize, sysExBufferCount, out handle, out errorCode);
        }

        public static IN_OPENRESULT Api_OpenEndpoint_Mac(IntPtr info, MidiDevicesSessionHandle sessionHandle, Callback_Mac callback, out IntPtr handle, out int errorCode)
        {
            return OpenInputEndpoint_Mac(info, sessionHandle, callback, out handle, out errorCode);
        }

        public static IN_CLOSERESULT Api_CloseEndpoint(IntPtr handle, out int errorCode)
        {
            return CloseInputEndpoint(handle, out errorCode);
        }

        public static IN_RENEWSYSEXBUFFERRESULT Api_RenewInputEndpointSysExBuffer(IntPtr handle, IntPtr header, out int errorCode)
        {
            return RenewInputEndpointSysExBuffer(handle, header, out errorCode);
        }

        public static IN_CONNECTRESULT Api_Connect(IntPtr handle, out int errorCode)
        {
            return ConnectToInputEndpoint(handle, out errorCode);
        }

        public static IN_DISCONNECTRESULT Api_Disconnect(IntPtr handle, out int errorCode)
        {
            return DisconnectFromInputEndpoint(handle, out errorCode);
        }

        public static IN_GETEVENTDATARESULT Api_GetEventData(IntPtr packetList, int packetIndex, out IntPtr data, out int length, out int packetsCount)
        {
            return GetEventDataFromInputEndpoint(packetList, packetIndex, out data, out length, out packetsCount);
        }

        public static IN_GETSYSEXDATARESULT Api_GetSysExBufferData(IntPtr header, out IntPtr data, out int size)
        {
            return GetInputEndpointSysExBufferData(header, out data, out size);
        }

        public static IN_GETPROPERTYRESULT Api_GetEndpointName(IntPtr info, out string name, out int errorCode)
        {
            name = string.Empty;

            var result = GetInputEndpointName(info, out var namePointer, out errorCode);
            if (result != IN_GETPROPERTYRESULT.IN_GETPROPERTYRESULT_OK)
                return result;

            name = NativeApi.GetStringFromPointer(namePointer);
            NativeApi.FreeStringPointer(namePointer);

            return result;
        }

        public static IN_GETPROPERTYRESULT Api_GetEndpointId(IntPtr info, out string id, out int errorCode)
        {
            errorCode = 0;
            id = string.Empty;

            IN_GETPROPERTYRESULT result = default;

            var apiType = CommonApi.Api_GetApiType();

            if (apiType == CommonApi.API_TYPE.API_TYPE_WIN)
            {
                result = GetInputEndpointId_Win(info, out var idPointer, out errorCode);
                if (result == IN_GETPROPERTYRESULT.IN_GETPROPERTYRESULT_OK)
                    id = NativeApi.GetStringFromPointer(idPointer);
            }
            else if (apiType == CommonApi.API_TYPE.API_TYPE_MAC)
            {
                result = GetInputEndpointId_Mac(info, out var idValue, out errorCode);
                if (result == IN_GETPROPERTYRESULT.IN_GETPROPERTYRESULT_OK)
                    id = idValue.ToString();
            }
            else
            {
                // TODO
            }

            return result;
        }

        public static void Api_DeleteEndpointInfo(IntPtr info)
        {
            DeleteInputEndpointInfo(info);
        }

        #endregion
    }
}
