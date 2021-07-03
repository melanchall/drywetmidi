using System;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Devices
{
    internal sealed class MidiDevicesSessionApi64 : MidiDevicesSessionApi
    {
        #region Constants

        private const string LibraryName = "Melanchall_DryWetMidi_Native64";

        #endregion

        #region Extern functions

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern SESSION_OPENRESULT OpenSession(IntPtr name, out IntPtr handle);

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern SESSION_CLOSERESULT CloseSession(IntPtr handle);

        #endregion

        #region Methods

        public override SESSION_OPENRESULT Api_OpenSession(IntPtr name, out IntPtr handle)
        {
            return OpenSession(name, out handle);
        }

        public override SESSION_CLOSERESULT Api_CloseSession(IntPtr handle)
        {
            return CloseSession(handle);
        }

        #endregion
    }
}
