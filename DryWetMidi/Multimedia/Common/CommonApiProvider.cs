namespace Melanchall.DryWetMidi.Multimedia
{
    internal static class CommonApiProvider
    {
        #region Fields

        private static CommonApi _api;

        #endregion

        #region Properties

        public static CommonApi Api
        {
            get
            {
                if (_api == null)
                    _api = new CommonApi64();

                return _api;
            }
        }

        #endregion
    }
}
