using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Melanchall.DryWetMidi.Devices
{
    internal static class MidiInWinApi
    {
        #region Types

        [StructLayout(LayoutKind.Sequential)]
        internal struct MIDIINCAPS
        {
            public ushort wMid;
            public ushort wPid;
            public uint vDriverVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szPname;
            public uint dwSupport;
        }

        #endregion

        #region Methods

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Ansi, EntryPoint = "midiInGetDevCapsA", ExactSpelling = true)]
        public static extern uint midiInGetDevCaps(IntPtr uDeviceID, ref MIDIINCAPS caps, uint cbMidiInCaps);

        [DllImport("winmm.dll", CharSet = CharSet.Ansi, EntryPoint = "midiInGetErrorTextA", ExactSpelling = true)]
        public static extern uint midiInGetErrorText(uint wError, StringBuilder lpText, uint cchText);

        [DllImport("winmm.dll", ExactSpelling = true)]
        public static extern uint midiInGetNumDevs();

        [DllImport("winmm.dll", ExactSpelling = true)]
        public static extern uint midiInOpen(out IntPtr lphMidiIn, int uDeviceID, MidiWinApi.MidiMessageCallback dwCallback, IntPtr dwInstance, uint dwFlags);

        [DllImport("winmm.dll", ExactSpelling = true)]
        public static extern uint midiInClose(IntPtr hMidiIn);

        [DllImport("winmm.dll", ExactSpelling = true)]
        public static extern uint midiInStart(IntPtr hMidiIn);

        [DllImport("winmm.dll", ExactSpelling = true)]
        public static extern uint midiInStop(IntPtr hMidiIn);

        [DllImport("winmm.dll", ExactSpelling = true)]
        public static extern uint midiInReset(IntPtr hMidiIn);

        [DllImport("winmm.dll", ExactSpelling = true)]
        public static extern uint midiInPrepareHeader(IntPtr hMidiIn, IntPtr lpMidiInHdr, int cbMidiInHdr);

        [DllImport("winmm.dll", ExactSpelling = true)]
        public static extern uint midiInUnprepareHeader(IntPtr hMidiIn, IntPtr lpMidiInHdr, int cbMidiInHdr);

        [DllImport("winmm.dll", ExactSpelling = true)]
        public static extern uint midiInAddBuffer(IntPtr hMidiIn, IntPtr lpMidiInHdr, int cbMidiInHdr);

        #endregion
    }
}
