using System;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class InputDeviceApi64 : InputDeviceApi
    {
        #region Constants

        private const string LibraryName = LibraryName64;

        #endregion

        #region Extern functions

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetInputDevicesCount();

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IN_GETINFORESULT GetInputDeviceInfo(int deviceIndex, out IntPtr info);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetInputDeviceHashCode(IntPtr info);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool AreInputDevicesEqual(IntPtr info1, IntPtr info2);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IN_GETPROPERTYRESULT GetInputDeviceName(IntPtr info, out IntPtr value);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IN_GETPROPERTYRESULT GetInputDeviceManufacturer(IntPtr info, out IntPtr value);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IN_GETPROPERTYRESULT GetInputDeviceProduct(IntPtr info, out IntPtr value);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IN_GETPROPERTYRESULT GetInputDeviceDriverVersion(IntPtr info, out int value);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IN_OPENRESULT OpenInputDevice_Win(IntPtr info, IntPtr sessionHandle, Callback_Win callback, int sysExBufferSize, out IntPtr handle);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IN_OPENRESULT OpenInputDevice_Mac(IntPtr info, IntPtr sessionHandle, Callback_Mac callback, out IntPtr handle);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IN_CLOSERESULT CloseInputDevice(IntPtr handle);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IN_RENEWSYSEXBUFFERRESULT RenewInputDeviceSysExBuffer(IntPtr handle, int size);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IN_CONNECTRESULT ConnectToInputDevice(IntPtr handle);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IN_DISCONNECTRESULT DisconnectFromInputDevice(IntPtr handle);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IN_GETEVENTDATARESULT GetEventDataFromInputDevice(IntPtr packetList, int packetIndex, out IntPtr data, out int length, out int packetsCount);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IN_GETSYSEXDATARESULT GetInputDeviceSysExBufferData(IntPtr header, out IntPtr data, out int size);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool IsInputDevicePropertySupported(InputDeviceProperty property);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IN_GETPROPERTYRESULT GetInputDeviceUniqueId(IntPtr info, out int value);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IN_GETPROPERTYRESULT GetInputDeviceDriverOwner(IntPtr info, out IntPtr value);

        #endregion

        #region Methods

        public override int Api_GetDevicesCount()
        {
            return GetInputDevicesCount();
        }

        public override IN_GETINFORESULT Api_GetDeviceInfo(int deviceIndex, out IntPtr info)
        {
            return GetInputDeviceInfo(deviceIndex, out info);
        }

        public override int Api_GetDeviceHashCode(IntPtr info)
        {
            return GetInputDeviceHashCode(info);
        }

        public override bool Api_AreDevicesEqual(IntPtr info1, IntPtr info2)
        {
            return AreInputDevicesEqual(info1, info2);
        }

        public override IN_OPENRESULT Api_OpenDevice_Win(IntPtr info, IntPtr sessionHandle, Callback_Win callback, int sysExBufferSize, out IntPtr handle)
        {
            return OpenInputDevice_Win(info, sessionHandle, callback, sysExBufferSize, out handle);
        }

        public override IN_OPENRESULT Api_OpenDevice_Mac(IntPtr info, IntPtr sessionHandle, Callback_Mac callback, out IntPtr handle)
        {
            return OpenInputDevice_Mac(info, sessionHandle, callback, out handle);
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

        public override IN_GETEVENTDATARESULT Api_GetEventData(IntPtr packetList, int packetIndex, out IntPtr data, out int length, out int packetsCount)
        {
            return GetEventDataFromInputDevice(packetList, packetIndex, out data, out length, out packetsCount);
        }

        public override IN_GETSYSEXDATARESULT Api_GetSysExBufferData(IntPtr header, out IntPtr data, out int size)
        {
            return GetInputDeviceSysExBufferData(header, out data, out size);
        }

        public override bool Api_IsPropertySupported(InputDeviceProperty property)
        {
            return IsInputDevicePropertySupported(property);
        }

        public override IN_GETPROPERTYRESULT Api_GetDeviceName(IntPtr info, out string name)
        {
            IntPtr namePointer;
            var result = GetInputDeviceName(info, out namePointer);
            name = GetStringFromPointer(namePointer);
            return result;
        }

        public override IN_GETPROPERTYRESULT Api_GetDeviceManufacturer(IntPtr info, out string manufacturer)
        {
            IntPtr manufacturerPointer;
            var result = GetInputDeviceManufacturer(info, out manufacturerPointer);
            manufacturer = GetStringFromPointer(manufacturerPointer);
            return result;
        }

        public override IN_GETPROPERTYRESULT Api_GetDeviceProduct(IntPtr info, out string product)
        {
            IntPtr productPointer;
            var result = GetInputDeviceProduct(info, out productPointer);
            product = GetStringFromPointer(productPointer);
            return result;
        }

        public override IN_GETPROPERTYRESULT Api_GetDeviceDriverVersion(IntPtr info, out int driverVersion)
        {
            return GetInputDeviceDriverVersion(info, out driverVersion);
        }

        public override IN_GETPROPERTYRESULT Api_GetDeviceUniqueId(IntPtr info, out int uniqueId)
        {
            return GetInputDeviceUniqueId(info, out uniqueId);
        }

        public override IN_GETPROPERTYRESULT Api_GetDeviceDriverOwner(IntPtr info, out string driverOwner)
        {
            IntPtr driverOwnerPointer;
            var result = GetInputDeviceDriverOwner(info, out driverOwnerPointer);
            driverOwner = GetStringFromPointer(driverOwnerPointer);
            return result;
        }

        #endregion
    }
}
