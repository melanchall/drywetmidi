namespace Melanchall.DryWetMidi.Multimedia
{
    internal static class InputDeviceApiProvider
    {
        #region Fields

        private static InputDeviceApi _api;

        #endregion

        #region Properties

        public static InputDeviceApi Api
        {
            get
            {
                if (_api == null)
                    _api = new InputDeviceApi64();

                return _api;
            }
        }

        #endregion
    }
}
