using System;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Devices
{
    internal static class MidiWinApi
    {
        #region Types

        [StructLayout(LayoutKind.Sequential)]
        internal struct MIDIHDR
        {
            public IntPtr lpData;
            public uint dwBufferLength;
            public uint dwBytesRecorded;
            public IntPtr dwUser;
            public uint dwFlags;
            public IntPtr lpNext;
            public IntPtr reserved;
            public uint dwOffset;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public IntPtr[] dwReserved;
        }

        public delegate void MidiMessageCallback(IntPtr hMidi, MidiMessage wMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2);

        #endregion

        #region Constants

        public const uint MaxErrorLength = 256;
        public const uint CallbackFunction = 196608;

        #endregion
    }
}
