using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal static class CommonApi
    {
        #region Nested enums

        public enum API_TYPE
        {
            API_TYPE_WIN = 0,
            API_TYPE_MAC = 1
        }

        #endregion

        #region Extern functions

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern API_TYPE GetApiType();

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool CanCompareDevices();

        #endregion

        #region Methods

        public static API_TYPE Api_GetApiType()
        {
            return GetApiType();
        }

        public static bool Api_CanCompareDevices()
        {
            return CanCompareDevices();
        }

        #endregion
    }
}
