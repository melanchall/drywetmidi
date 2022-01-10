using System;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class MidiDevicesSessionApi64 : MidiDevicesSessionApi
    {
        #region Constants

        private const string LibraryName = LibraryName64;

        #endregion

        #region Extern functions

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern SESSION_OPENRESULT OpenSession_Mac(IntPtr name, InputDeviceCallback inputDeviceCallback, OutputDeviceCallback outputDeviceCallback, out IntPtr handle);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern SESSION_OPENRESULT OpenSession_Win(IntPtr name, out IntPtr handle);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern SESSION_CLOSERESULT CloseSession(IntPtr handle);

        #endregion

        #region Methods

        public override SESSION_OPENRESULT Api_OpenSession_Mac(
            IntPtr name,
            InputDeviceCallback inputDeviceCallback,
            OutputDeviceCallback outputDeviceCallback,
            out IntPtr handle)
        {
            return OpenSession_Mac(name, inputDeviceCallback, outputDeviceCallback, out handle);
        }

        public override SESSION_OPENRESULT Api_OpenSession_Win(IntPtr name, out IntPtr handle)
        {
            return OpenSession_Win(name, out handle);
        }

        public override SESSION_CLOSERESULT Api_CloseSession(IntPtr handle)
        {
            return CloseSession(handle);
        }

        #endregion
    }
}
