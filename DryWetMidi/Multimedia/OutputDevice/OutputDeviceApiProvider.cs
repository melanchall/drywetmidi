namespace Melanchall.DryWetMidi.Multimedia
{
    internal static class OutputDeviceApiProvider
    {
        #region Fields

        private static OutputDeviceApi _api;

        #endregion

        #region Properties

        public static OutputDeviceApi Api
        {
            get
            {
                if (_api == null)
                    _api = new OutputDeviceApi64();

                return _api;
            }
        }

        #endregion
    }
}
