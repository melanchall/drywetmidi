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

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern MMRESULT midiOutGetDevCaps(UIntPtr uDeviceID, ref MIDIOUTCAPS lpMidiOutCaps, uint cbMidiOutCaps);

        [DllImport("winmm.dll")]
        public static extern MMRESULT midiOutGetErrorText(MMRESULT mmrError, StringBuilder pszText, uint cchText);

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern int midiOutGetNumDevs();

        [DllImport("winmm.dll")]
        public static extern MMRESULT midiOutOpen(out IntPtr lphmo, uint uDeviceID, MidiWinApi.MidiMessageCallback dwCallback, IntPtr dwInstance, uint dwFlags);

        [DllImport("winmm.dll")]
        public static extern MMRESULT midiOutClose(IntPtr hmo);

        [DllImport("winmm.dll")]
        public static extern MMRESULT midiOutShortMsg(IntPtr hMidiOut, uint dwMsg);

        [DllImport("winmm.dll")]
        public static extern MMRESULT midiOutGetVolume(IntPtr hmo, ref uint lpdwVolume);

        [DllImport("winmm.dll")]
        public static extern MMRESULT midiOutSetVolume(IntPtr hmo, uint dwVolume);

        #endregion
    }
}
