using System;
using System.Runtime.InteropServices;
using System.Text;

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

        public delegate MMRESULT ErrorTextGetter(MMRESULT mmrError, StringBuilder pszText, uint cchText);

        #endregion

        #region Constants

        public const uint MaxErrorLength = 256;
        public const uint CallbackFunction = 196608;

        #endregion

        #region Methods

        public static void ProcessMmResult(Func<MMRESULT> method, ErrorTextGetter errorTextGetter)
        {
            var mmResult = method();
            if (mmResult == MMRESULT.MMSYSERR_NOERROR)
                return;

            var stringBuilder = new StringBuilder((int)MaxErrorLength);
            var getErrorTextResult = errorTextGetter(mmResult, stringBuilder, MaxErrorLength + 1);
            if (getErrorTextResult != MMRESULT.MMSYSERR_NOERROR)
                throw new MidiDeviceException("Error occured during operation on device.");

            var errorText = stringBuilder.ToString();
            throw new MidiDeviceException(errorText);
        }

        #endregion
    }
}
