namespace Melanchall.DryWetMidi.Multimedia
{
    internal static class MidiDevicesSessionApiProvider
    {
        #region Fields

        private static MidiDevicesSessionApi _api;

        #endregion

        #region Properties

        public static MidiDevicesSessionApi Api
        {
            get
            {
                if (_api == null)
                    _api = new MidiDevicesSessionApi64();

                return _api;
            }
        }

        #endregion
    }
}
