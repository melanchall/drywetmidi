using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal static class VirtualDeviceApiProvider
    {
        #region Constants

        private static readonly bool Is64Bit = IntPtr.Size == 8;

        #endregion

        #region Fields

        private static VirtualDeviceApi _api;

        #endregion

        #region Properties

        public static VirtualDeviceApi Api
        {
            get
            {
                if (_api == null)
                    _api = Is64Bit ? (VirtualDeviceApi)new VirtualDeviceApi64() : new VirtualDeviceApi32();

                return _api;
            }
        }

        #endregion
    }
}
