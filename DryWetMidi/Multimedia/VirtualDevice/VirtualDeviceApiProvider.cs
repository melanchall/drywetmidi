namespace Melanchall.DryWetMidi.Multimedia
{
    internal static class VirtualDeviceApiProvider
    {
        #region Fields

        private static VirtualDeviceApi _api;

        #endregion

        #region Properties

        public static VirtualDeviceApi Api
        {
            get
            {
                if (_api == null)
                    _api = new VirtualDeviceApi64();

                return _api;
            }
        }

        #endregion
    }
}
