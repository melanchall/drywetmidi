using System;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class OutputDeviceApi32 : OutputDeviceApi
    {
        #region Constants

        private const string LibraryName = LibraryName32;

        #endregion

        #region Extern functions

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetOutputDevicesCount();

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETINFORESULT GetOutputDeviceInfo(int deviceIndex, out IntPtr info);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetOutputDeviceHashCode(IntPtr info);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool AreOutputDevicesEqual(IntPtr info1, IntPtr info2);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETPROPERTYRESULT GetOutputDeviceName(IntPtr info, out IntPtr value);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETPROPERTYRESULT GetOutputDeviceManufacturer(IntPtr info, out IntPtr value);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETPROPERTYRESULT GetOutputDeviceProduct(IntPtr info, out IntPtr value);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETPROPERTYRESULT GetOutputDeviceDriverVersion(IntPtr info, out int value);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_OPENRESULT OpenOutputDevice_Win(IntPtr info, IntPtr sessionHandle, Callback_Win callback, out IntPtr handle);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_OPENRESULT OpenOutputDevice_Mac(IntPtr info, IntPtr sessionHandle, out IntPtr handle);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_CLOSERESULT CloseOutputDevice(IntPtr handle);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_SENDSHORTRESULT SendShortEventToOutputDevice(IntPtr handle, int message);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_SENDSYSEXRESULT SendSysExEventToOutputDevice_Mac(IntPtr handle, byte[] data, ushort dataSize);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_SENDSYSEXRESULT SendSysExEventToOutputDevice_Win(IntPtr handle, IntPtr data, int size);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETSYSEXDATARESULT GetOutputDeviceSysExBufferData(IntPtr handle, IntPtr header, out IntPtr data, out int size);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool IsOutputDevicePropertySupported(OutputDeviceProperty property);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETPROPERTYRESULT GetOutputDeviceTechnology(IntPtr info, out OutputDeviceTechnology value);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETPROPERTYRESULT GetOutputDeviceUniqueId(IntPtr info, out int value);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETPROPERTYRESULT GetOutputDeviceVoicesNumber(IntPtr info, out int value);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETPROPERTYRESULT GetOutputDeviceNotesNumber(IntPtr info, out int value);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETPROPERTYRESULT GetOutputDeviceChannelsMask(IntPtr info, out int value);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETPROPERTYRESULT GetOutputDeviceOptions(IntPtr info, out OutputDeviceOption value);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETPROPERTYRESULT GetOutputDeviceDriverOwner(IntPtr info, out IntPtr value);

        #endregion

        #region Methods

        public override int Api_GetDevicesCount()
        {
            return GetOutputDevicesCount();
        }

        public override OUT_GETINFORESULT Api_GetDeviceInfo(int deviceIndex, out IntPtr info)
        {
            return GetOutputDeviceInfo(deviceIndex, out info);
        }

        public override int Api_GetDeviceHashCode(IntPtr info)
        {
            return GetOutputDeviceHashCode(info);
        }

        public override bool Api_AreDevicesEqual(IntPtr info1, IntPtr info2)
        {
            return AreOutputDevicesEqual(info1, info2);
        }

        public override OUT_OPENRESULT Api_OpenDevice_Win(IntPtr info, IntPtr sessionHandle, Callback_Win callback, out IntPtr handle)
        {
            return OpenOutputDevice_Win(info, sessionHandle, callback, out handle);
        }

        public override OUT_OPENRESULT Api_OpenDevice_Mac(IntPtr info, IntPtr sessionHandle, out IntPtr handle)
        {
            return OpenOutputDevice_Mac(info, sessionHandle, out handle);
        }

        public override OUT_CLOSERESULT Api_CloseDevice(IntPtr handle)
        {
            return CloseOutputDevice(handle);
        }

        public override OUT_SENDSHORTRESULT Api_SendShortEvent(IntPtr handle, int message)
        {
            return SendShortEventToOutputDevice(handle, message);
        }

        public override OUT_SENDSYSEXRESULT Api_SendSysExEvent_Mac(IntPtr handle, byte[] data, ushort dataSize)
        {
            return SendSysExEventToOutputDevice_Mac(handle, data, dataSize);
        }

        public override OUT_SENDSYSEXRESULT Api_SendSysExEvent_Win(IntPtr handle, IntPtr data, int size)
        {
            return SendSysExEventToOutputDevice_Win(handle, data, size);
        }

        public override OUT_GETSYSEXDATARESULT Api_GetSysExBufferData(IntPtr handle, IntPtr header, out IntPtr data, out int size)
        {
            return GetOutputDeviceSysExBufferData(handle, header, out data, out size);
        }

        public override bool Api_IsPropertySupported(OutputDeviceProperty property)
        {
            return IsOutputDevicePropertySupported(property);
        }

        public override OUT_GETPROPERTYRESULT Api_GetDeviceName(IntPtr info, out string name)
        {
            IntPtr namePointer;
            var result = GetOutputDeviceName(info, out namePointer);
            name = GetStringFromPointer(namePointer);
            return result;
        }

        public override OUT_GETPROPERTYRESULT Api_GetDeviceManufacturer(IntPtr info, out string manufacturer)
        {
            IntPtr manufacturerPointer;
            var result = GetOutputDeviceManufacturer(info, out manufacturerPointer);
            manufacturer = GetStringFromPointer(manufacturerPointer);
            return result;
        }

        public override OUT_GETPROPERTYRESULT Api_GetDeviceProduct(IntPtr info, out string product)
        {
            IntPtr productPointer;
            var result = GetOutputDeviceProduct(info, out productPointer);
            product = GetStringFromPointer(productPointer);
            return result;
        }

        public override OUT_GETPROPERTYRESULT Api_GetDeviceDriverVersion(IntPtr info, out int driverVersion)
        {
            return GetOutputDeviceDriverVersion(info, out driverVersion);
        }

        public override OUT_GETPROPERTYRESULT Api_GetDeviceTechnology(IntPtr info, out OutputDeviceTechnology deviceType)
        {
            return GetOutputDeviceTechnology(info, out deviceType);
        }

        public override OUT_GETPROPERTYRESULT Api_GetDeviceUniqueId(IntPtr info, out int uniqueId)
        {
            return GetOutputDeviceUniqueId(info, out uniqueId);
        }

        public override OUT_GETPROPERTYRESULT Api_GetDeviceVoicesNumber(IntPtr info, out int voicesNumber)
        {
            return GetOutputDeviceVoicesNumber(info, out voicesNumber);
        }

        public override OUT_GETPROPERTYRESULT Api_GetDeviceNotesNumber(IntPtr info, out int notesNumber)
        {
            return GetOutputDeviceNotesNumber(info, out notesNumber);
        }

        public override OUT_GETPROPERTYRESULT Api_GetDeviceChannelsMask(IntPtr info, out int channelsMask)
        {
            return GetOutputDeviceChannelsMask(info, out channelsMask);
        }

        public override OUT_GETPROPERTYRESULT Api_GetDeviceOptions(IntPtr info, out OutputDeviceOption option)
        {
            return GetOutputDeviceOptions(info, out option);
        }

        public override OUT_GETPROPERTYRESULT Api_GetDeviceDriverOwner(IntPtr info, out string driverOwner)
        {
            IntPtr driverOwnerPointer;
            var result = GetOutputDeviceDriverOwner(info, out driverOwnerPointer);
            driverOwner = GetStringFromPointer(driverOwnerPointer);
            return result;
        }

        #endregion
    }
}
