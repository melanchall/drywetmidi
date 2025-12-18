namespace Melanchall.DryWetMidi.Multimedia
{
    internal static class TickGeneratorSessionApiProvider
    {
        #region Fields

        private static TickGeneratorSessionApi _api;

        #endregion

        #region Properties

        public static TickGeneratorSessionApi Api
        {
            get
            {
                if (_api == null)
                    _api = new TickGeneratorSessionApi64();

                return _api;
            }
        }

        #endregion
    }
}
