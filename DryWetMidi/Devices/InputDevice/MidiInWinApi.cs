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

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern MMRESULT midiInGetDevCaps(UIntPtr uDeviceID, ref MIDIINCAPS caps, uint cbMidiInCaps);

        [DllImport("winmm.dll")]
        public static extern MMRESULT midiInGetErrorText(MMRESULT wError, StringBuilder lpText, uint cchText);

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern int midiInGetNumDevs();

        [DllImport("winmm.dll")]
        public static extern MMRESULT midiInOpen(out IntPtr lphMidiIn, uint uDeviceID, MidiWinApi.MidiMessageCallback dwCallback, IntPtr dwInstance, uint dwFlags);

        [DllImport("winmm.dll")]
        public static extern MMRESULT midiInClose(IntPtr hMidiIn);

        [DllImport("winmm.dll")]
        public static extern MMRESULT midiInStart(IntPtr hMidiIn);

        [DllImport("winmm.dll")]
        public static extern MMRESULT midiInStop(IntPtr hMidiIn);

        [DllImport("winmm.dll")]
        public static extern MMRESULT midiInReset(IntPtr hMidiIn);

        [DllImport("winmm.dll")]
        public static extern MMRESULT midiInPrepareHeader(IntPtr hMidiIn, ref MidiWinApi.MIDIHDR lpMidiInHdr, int cbMidiInHdr);

        [DllImport("winmm.dll")]
        public static extern MMRESULT midiInUnprepareHeader(IntPtr hMidiIn, ref MidiWinApi.MIDIHDR lpMidiInHdr, int cbMidiInHdr);

        [DllImport("winmm.dll")]
        public static extern MMRESULT midiInAddBuffer(IntPtr hMidiIn, ref MidiWinApi.MIDIHDR lpMidiInHdr, int cbMidiInHdr);

        #endregion
    }
}
