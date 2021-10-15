using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal static class CommonApiProvider
    {
        #region Constants

        private static readonly bool Is64Bit = IntPtr.Size == 8;

        #endregion

        #region Fields

        private static CommonApi _api;

        #endregion

        #region Properties

        public static CommonApi Api
        {
            get
            {
                if (_api == null)
                    _api = Is64Bit ? (CommonApi)new CommonApi64() : new CommonApi32();

                return _api;
            }
        }

        #endregion
    }
}
