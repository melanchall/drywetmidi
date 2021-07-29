using System;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Devices
{
    internal sealed class VirtualDeviceApi64 : VirtualDeviceApi
    {
        #region Constants

        private const string LibraryName = LibraryName64;
        private const string TeLibraryName = "teVirtualMIDI";

        #endregion

        #region Extern functions

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern API_TYPE GetApiType();

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern VIRTUAL_OPENRESULT OpenVirtualDevice_Apple(IntPtr name, IntPtr sessionHandle, Callback_Apple callback, out IntPtr info);

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern VIRTUAL_CLOSERESULT CloseVirtualDevice(IntPtr info);

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern VIRTUAL_SENDBACKRESULT SendDataBackFromVirtualDevice(IntPtr pktlist, IntPtr readProcRefCon);

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern IntPtr GetInputDeviceInfoFromVirtualDevice(IntPtr info);

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern IntPtr GetOutputDeviceInfoFromVirtualDevice(IntPtr info);

        [DllImport(TeLibraryName, ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr virtualMIDICreatePortEx2(string portName, Callback_Te callback, IntPtr dwCallbackInstance, uint maxSysexLength, uint flags);

        [DllImport(TeLibraryName, ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool virtualMIDISendData(IntPtr midiPort, IntPtr midiDataBytes, uint length);

        [DllImport(TeLibraryName, ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern void virtualMIDIClosePort(IntPtr instance);

        #endregion

        #region Methods

        public override API_TYPE Api_GetApiType()
        {
            return GetApiType();
        }

        public override VIRTUAL_OPENRESULT Api_OpenDevice_Apple(string name, IntPtr sessionHandle, Callback_Apple callback, out IntPtr info)
        {
            var namePointer = Marshal.StringToHGlobalAnsi(name);
            return OpenVirtualDevice_Apple(namePointer, sessionHandle, callback, out info);
        }

        public override VIRTUAL_OPENRESULT Api_OpenDevice_Te(string name, IntPtr sessionHandle, Callback_Te callback, out IntPtr info)
        {
            // TODO
            info = virtualMIDICreatePortEx2(name, callback, IntPtr.Zero, 4096, 1);
            return VIRTUAL_OPENRESULT.VIRTUAL_OPENRESULT_OK;
        }

        public override VIRTUAL_CLOSERESULT Api_CloseDevice(IntPtr info)
        {
            var apiType = Api_GetApiType();
            switch (apiType)
            {
                case API_TYPE.API_TYPE_APPLE:
                    return CloseVirtualDevice(info);
                default:
                    virtualMIDIClosePort(info);
                    return VIRTUAL_CLOSERESULT.VIRTUAL_CLOSERESULT_OK;
            }
        }

        public override VIRTUAL_SENDBACKRESULT Api_SendDataBack(IntPtr pktlist, IntPtr readProcRefCon)
        {
            return SendDataBackFromVirtualDevice(pktlist, readProcRefCon);
        }

        public override VIRTUAL_SENDBACKRESULT Api_SendDataBack_Te(IntPtr midiPort, IntPtr midiDataBytes, uint length)
        {
            return virtualMIDISendData(midiPort, midiDataBytes, length)
                ? VIRTUAL_SENDBACKRESULT.VIRTUAL_SENDBACKRESULT_OK
                : VIRTUAL_SENDBACKRESULT.VIRTUAL_SENDBACKRESULT_UNKNOWNERROR_TE;
        }

        public override IntPtr Api_GetInputDeviceInfo(IntPtr info)
        {
            return GetInputDeviceInfoFromVirtualDevice(info);
        }

        public override IntPtr Api_GetOutputDeviceInfo(IntPtr info)
        {
            return GetOutputDeviceInfoFromVirtualDevice(info);
        }

        #endregion
    }
}
