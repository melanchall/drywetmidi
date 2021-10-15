using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal static class OutputDeviceApiProvider
    {
        #region Constants

        private static readonly bool Is64Bit = IntPtr.Size == 8;

        #endregion

        #region Fields

        private static OutputDeviceApi _api;

        #endregion

        #region Properties

        public static OutputDeviceApi Api
        {
            get
            {
                if (_api == null)
                    _api = Is64Bit ? (OutputDeviceApi)new OutputDeviceApi64() : new OutputDeviceApi32();

                return _api;
            }
        }

        #endregion
    }
}
