namespace Melanchall.DryWetMidi.Multimedia
{
    internal static class TickGeneratorApiProvider
    {
        #region Fields

        private static TickGeneratorApi _api;

        #endregion

        #region Properties

        public static TickGeneratorApi Api
        {
            get
            {
                if (_api == null)
                    _api = new TickGeneratorApi64();

                return _api;
            }
        }

        #endregion
    }
}
