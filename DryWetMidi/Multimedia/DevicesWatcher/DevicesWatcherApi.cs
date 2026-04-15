using System;
using System.Runtime.InteropServices;
using static Melanchall.DryWetMidi.Multimedia.MidiDevicesSessionApi;

#if NET7_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace Melanchall.DryWetMidi.Multimedia
{
    internal static partial class DevicesWatcherApi
    {
        #region Extern functions

#if NET7_0_OR_GREATER
        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial void EnableDevicesWatcher(MidiDevicesSessionHandle sessionHandle);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial void DisableDevicesWatcher(MidiDevicesSessionHandle sessionHandle);
#else
        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern void EnableDevicesWatcher(MidiDevicesSessionHandle sessionHandle);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DisableDevicesWatcher(MidiDevicesSessionHandle sessionHandle);
#endif

        #endregion

        #region Methods

        public static void Api_EnableDevicesWatcher(MidiDevicesSessionHandle sessionHandle)
        {
            EnableDevicesWatcher(sessionHandle);
        }

        public static void Api_DisableDevicesWatcher(MidiDevicesSessionHandle sessionHandle)
        {
            DisableDevicesWatcher(sessionHandle);
        }

        #endregion
    }
}
