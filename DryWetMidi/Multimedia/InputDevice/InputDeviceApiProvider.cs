using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal static class InputDeviceApiProvider
    {
        #region Constants

        private static readonly bool Is64Bit = IntPtr.Size == 8;

        #endregion

        #region Fields

        private static InputDeviceApi _api;

        #endregion

        #region Properties

        public static InputDeviceApi Api
        {
            get
            {
                if (_api == null)
                    _api = Is64Bit ? (InputDeviceApi)new InputDeviceApi64() : new InputDeviceApi32();

                return _api;
            }
        }

        #endregion
    }
}
