using System;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Devices
{
    internal sealed class InputDeviceApi64 : InputDeviceApi
    {
        #region Constants

        private const string LibraryName = "Melanchall_DryWetMidi_Native64";

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
        private static extern uint GetInputDeviceDriverVersion(IntPtr info);

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern IN_OPENRESULT OpenInputDevice_Winmm(IntPtr info, Callback_Winmm callback, int sysExBufferSize, out IntPtr handle);

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern IN_CLOSERESULT CloseInputDevice(IntPtr handle);

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern IN_RENEWSYSEXBUFFERRESULT RenewInputDeviceSysExBuffer(IntPtr handle, int size);

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern IN_CONNECTRESULT ConnectToInputDevice(IntPtr handle);

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern IN_DISCONNECTRESULT DisconnectFromInputDevice(IntPtr handle);

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

        public override uint Api_GetDeviceDriverVersion(IntPtr info)
        {
            return GetInputDeviceDriverVersion(info);
        }

        public override IN_OPENRESULT Api_OpenDevice_Winmm(IntPtr info, Callback_Winmm callback, int sysExBufferSize, out IntPtr handle)
        {
            return OpenInputDevice_Winmm(info, callback, sysExBufferSize, out handle);
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

        #endregion
    }
}
