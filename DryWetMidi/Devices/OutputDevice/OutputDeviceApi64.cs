using System;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Devices
{
    internal sealed class OutputDeviceApi64 : OutputDeviceApi
    {
        #region Constants

        private const string LibraryName = LibraryName64;

        #endregion

        #region Extern functions

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern API_TYPE GetApiType();

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern int GetOutputDevicesCount();

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern OUT_GETINFORESULT GetOutputDeviceInfo(int deviceIndex, out IntPtr info);

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern IntPtr GetOutputDeviceName(IntPtr info);

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern IntPtr GetOutputDeviceManufacturer(IntPtr info);

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern IntPtr GetOutputDeviceProduct(IntPtr info);

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern int GetOutputDeviceDriverVersion(IntPtr info);

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern OUT_OPENRESULT OpenOutputDevice_Winmm(IntPtr info, IntPtr sessionHandle, Callback_Winmm callback, out IntPtr handle);

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern OUT_OPENRESULT OpenOutputDevice_Apple(IntPtr info, IntPtr sessionHandle, out IntPtr handle);

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern OUT_CLOSERESULT CloseOutputDevice(IntPtr handle);

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern OUT_SENDSHORTRESULT SendShortEventToOutputDevice(IntPtr handle, int message);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_SENDSYSEXRESULT SendSysExEventToOutputDevice_Apple(IntPtr handle, byte[] data, ushort dataSize);

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern OUT_SENDSYSEXRESULT SendSysExEventToOutputDevice_Winmm(IntPtr handle, IntPtr data, int size);

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern OUT_GETSYSEXDATARESULT GetOutputDeviceSysExBufferData(IntPtr handle, IntPtr header, out IntPtr data, out int size);

        #endregion

        #region Methods

        public override API_TYPE Api_GetApiType()
        {
            return GetApiType();
        }

        public override int Api_GetDevicesCount()
        {
            return GetOutputDevicesCount();
        }

        public override OUT_GETINFORESULT Api_GetDeviceInfo(int deviceIndex, out IntPtr info)
        {
            return GetOutputDeviceInfo(deviceIndex, out info);
        }

        public override string Api_GetDeviceName(IntPtr info)
        {
            var namePointer = GetOutputDeviceName(info);
            return namePointer != IntPtr.Zero ? Marshal.PtrToStringAnsi(namePointer) : string.Empty;
        }

        public override string Api_GetDeviceManufacturer(IntPtr info)
        {
            var manufacturerPointer = GetOutputDeviceManufacturer(info);
            return manufacturerPointer != IntPtr.Zero ? Marshal.PtrToStringAnsi(manufacturerPointer) : string.Empty;
        }

        public override string Api_GetDeviceProduct(IntPtr info)
        {
            var productPointer = GetOutputDeviceProduct(info);
            return productPointer != IntPtr.Zero ? Marshal.PtrToStringAnsi(productPointer) : string.Empty;
        }

        public override int Api_GetDeviceDriverVersion(IntPtr info)
        {
            return GetOutputDeviceDriverVersion(info);
        }

        public override OUT_OPENRESULT Api_OpenDevice_Winmm(IntPtr info, IntPtr sessionHandle, Callback_Winmm callback, out IntPtr handle)
        {
            return OpenOutputDevice_Winmm(info, sessionHandle, callback, out handle);
        }

        public override OUT_OPENRESULT Api_OpenDevice_Apple(IntPtr info, IntPtr sessionHandle, out IntPtr handle)
        {
            return OpenOutputDevice_Apple(info, sessionHandle, out handle);
        }

        public override OUT_CLOSERESULT Api_CloseDevice(IntPtr handle)
        {
            return CloseOutputDevice(handle);
        }

        public override OUT_SENDSHORTRESULT Api_SendShortEvent(IntPtr handle, int message)
        {
            return SendShortEventToOutputDevice(handle, message);
        }

        public override OUT_SENDSYSEXRESULT Api_SendSysExEvent_Apple(IntPtr handle, byte[] data, ushort dataSize)
        {
            return SendSysExEventToOutputDevice_Apple(handle, data, dataSize);
        }

        public override OUT_SENDSYSEXRESULT Api_SendSysExEvent_Winmm(IntPtr handle, IntPtr data, int size)
        {
            return SendSysExEventToOutputDevice_Winmm(handle, data, size);
        }

        public override OUT_GETSYSEXDATARESULT Api_GetSysExBufferData(IntPtr handle, IntPtr header, out IntPtr data, out int size)
        {
            return GetOutputDeviceSysExBufferData(handle, header, out data, out size);
        }

        #endregion
    }
}
