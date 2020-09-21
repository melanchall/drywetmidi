using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Melanchall.DryWetMidi.Devices
{
    internal static class MidiOutWinApi
    {
        #region Types

        [StructLayout(LayoutKind.Sequential)]
        public struct MIDIOUTCAPS
        {
            public ushort wMid;
            public ushort wPid;
            public uint vDriverVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szPname;
            public ushort wTechnology;
            public ushort wVoices;
            public ushort wNotes;
            public ushort wChannelMask;
            public uint dwSupport;
        }

        [Flags]
        public enum MIDICAPS : uint
        {
            MIDICAPS_VOLUME = 1,
            MIDICAPS_LRVOLUME = 2,
            MIDICAPS_CACHE = 4,
            MIDICAPS_STREAM = 8
        }

        #endregion

        #region Methods

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Ansi, EntryPoint = "midiOutGetDevCapsA", ExactSpelling = true)]
        public static extern uint midiOutGetDevCaps(IntPtr uDeviceID, ref MIDIOUTCAPS lpMidiOutCaps, uint cbMidiOutCaps);

        [DllImport("winmm.dll", CharSet = CharSet.Ansi, EntryPoint = "midiOutGetErrorTextA", ExactSpelling = true)]
        public static extern uint midiOutGetErrorText(uint mmrError, StringBuilder pszText, uint cchText);

        [DllImport("winmm.dll", ExactSpelling = true)]
        public static extern uint midiOutGetNumDevs();

        [DllImport("winmm.dll", ExactSpelling = true)]
        public static extern uint midiOutOpen(out IntPtr lphmo, int uDeviceID, MidiWinApi.MidiMessageCallback dwCallback, IntPtr dwInstance, uint dwFlags);

        [DllImport("winmm.dll", ExactSpelling = true)]
        public static extern uint midiOutClose(IntPtr hmo);

        [DllImport("winmm.dll", ExactSpelling = true)]
        public static extern uint midiOutShortMsg(IntPtr hMidiOut, uint dwMsg);

        [DllImport("winmm.dll", ExactSpelling = true)]
        public static extern uint midiOutGetVolume(IntPtr hmo, ref uint lpdwVolume);

        [DllImport("winmm.dll", ExactSpelling = true)]
        public static extern uint midiOutSetVolume(IntPtr hmo, uint dwVolume);

        [DllImport("winmm.dll", ExactSpelling = true)]
        public static extern uint midiOutPrepareHeader(IntPtr hmo, IntPtr lpMidiOutHdr, int cbMidiOutHdr);

        [DllImport("winmm.dll", ExactSpelling = true)]
        public static extern uint midiOutUnprepareHeader(IntPtr hmo, IntPtr lpMidiOutHdr, int cbMidiOutHdr);

        [DllImport("winmm.dll", ExactSpelling = true)]
        public static extern uint midiOutLongMsg(IntPtr hmo, IntPtr lpMidiOutHdr, int cbMidiOutHdr);

        #endregion
    }
}
