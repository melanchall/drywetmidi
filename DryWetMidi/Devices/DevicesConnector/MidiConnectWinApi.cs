using System;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Devices
{
    internal static class MidiConnectWinApi
    {
        #region Methods

        [DllImport("winmm.dll")]
        public static extern uint midiConnect(IntPtr hMidi, IntPtr hmo, IntPtr pReserved);

        [DllImport("winmm.dll")]
        public static extern uint midiDisconnect(IntPtr hMidi, IntPtr hmo, IntPtr pReserved);

        #endregion
    }
}
