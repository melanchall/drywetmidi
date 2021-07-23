using System;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Devices
{
    internal sealed class InputDeviceApi32 : InputDeviceApi
    {
        #region Constants

        private const string LibraryName = "Melanchall_DryWetMidi_Native32";

        #endregion

        #region Extern functions

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern API_TYPE GetApiType();

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern int GetInputDevicesCount();

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern IN_GETINFORESULT GetInputDeviceInfo(int deviceIndex, out IntPtr info);

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern IntPtr GetInputDeviceName(IntPtr info);

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern IntPtr GetInputDeviceManufacturer(IntPtr info);

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern IntPtr GetInputDeviceProduct(IntPtr info);

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern int GetInputDeviceDriverVersion(IntPtr info);

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern IN_OPENRESULT OpenInputDevice_Winmm(IntPtr info, IntPtr sessionHandle, Callback_Winmm callback, int sysExBufferSize, out IntPtr handle);

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern IN_OPENRESULT OpenInputDevice_Apple(IntPtr info, IntPtr sessionHandle, Callback_Apple callback, out IntPtr handle);

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern IN_CLOSERESULT CloseInputDevice(IntPtr handle);

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern IN_RENEWSYSEXBUFFERRESULT RenewInputDeviceSysExBuffer(IntPtr handle, int size);

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern IN_CONNECTRESULT ConnectToInputDevice(IntPtr handle);

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern IN_DISCONNECTRESULT DisconnectFromInputDevice(IntPtr handle);

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern IN_GETEVENTDATARESULT GetEventDataFromInputDevice(IntPtr packetList, int packetIndex, out IntPtr data, out int length);

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern IN_GETSYSEXDATARESULT GetInputDeviceSysExBufferData(IntPtr header, out IntPtr data, out int size);

        #endregion

        #region Methods

        public override API_TYPE Api_GetApiType()
        {
            return GetApiType();
        }

        public override int Api_GetDevicesCount()
        {
            return GetInputDevicesCount();
        }

        public override IN_GETINFORESULT Api_GetDeviceInfo(int deviceIndex, out IntPtr info)
        {
            return GetInputDeviceInfo(deviceIndex, out info);
        }

        public override string Api_GetDeviceName(IntPtr info)
        {
            var namePointer = GetInputDeviceName(info);
            return namePointer != IntPtr.Zero ? Marshal.PtrToStringAnsi(namePointer) : string.Empty;
        }

        public override string Api_GetDeviceManufacturer(IntPtr info)
        {
            var manufacturerPointer = GetInputDeviceManufacturer(info);
            return manufacturerPointer != IntPtr.Zero ? Marshal.PtrToStringAnsi(manufacturerPointer) : string.Empty;
        }

        public override string Api_GetDeviceProduct(IntPtr info)
        {
            var productPointer = GetInputDeviceProduct(info);
            return productPointer != IntPtr.Zero ? Marshal.PtrToStringAnsi(productPointer) : string.Empty;
        }

        public override int Api_GetDeviceDriverVersion(IntPtr info)
        {
            return GetInputDeviceDriverVersion(info);
        }

        public override IN_OPENRESULT Api_OpenDevice_Winmm(IntPtr info, IntPtr sessionHandle, Callback_Winmm callback, int sysExBufferSize, out IntPtr handle)
        {
            return OpenInputDevice_Winmm(info, sessionHandle, callback, sysExBufferSize, out handle);
        }

        public override IN_OPENRESULT Api_OpenDevice_Apple(IntPtr info, IntPtr sessionHandle, Callback_Apple callback, out IntPtr handle)
        {
            return OpenInputDevice_Apple(info, sessionHandle, callback, out handle);
        }

        public override IN_CLOSERESULT Api_CloseDevice(IntPtr handle)
        {
            return CloseInputDevice(handle);
        }

        public override IN_RENEWSYSEXBUFFERRESULT Api_RenewSysExBuffer(IntPtr handle, int size)
        {
            return RenewInputDeviceSysExBuffer(handle, size);
        }

        public override IN_CONNECTRESULT Api_Connect(IntPtr handle)
        {
            return ConnectToInputDevice(handle);
        }

        public override IN_DISCONNECTRESULT Api_Disconnect(IntPtr handle)
        {
            return DisconnectFromInputDevice(handle);
        }

        public override IN_GETEVENTDATARESULT Api_GetEventData(IntPtr packetList, int packetIndex, out IntPtr data, out int length)
        {
            return GetEventDataFromInputDevice(packetList, packetIndex, out data, out length);
        }

        public override IN_GETSYSEXDATARESULT Api_GetSysExBufferData(IntPtr header, out IntPtr data, out int size)
        {
            return GetInputDeviceSysExBufferData(header, out data, out size);
        }

        #endregion
    }
}
