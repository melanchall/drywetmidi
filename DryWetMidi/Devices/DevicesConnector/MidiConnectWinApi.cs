using System;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Devices
{
    internal static class MidiConnectWinApi
    {
        #region Methods

        [DllImport("winmm.dll")]
        public static extern MMRESULT midiConnect(IntPtr hMidi, IntPtr hmo, IntPtr pReserved);

        [DllImport("winmm.dll")]
        public static extern MMRESULT midiDisconnect(IntPtr hMidi, IntPtr hmo, IntPtr pReserved);

        #endregion
    }
}
