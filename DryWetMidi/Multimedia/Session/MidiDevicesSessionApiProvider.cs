using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal static class MidiDevicesSessionApiProvider
    {
        #region Constants

        private static readonly bool Is64Bit = IntPtr.Size == 8;

        #endregion

        #region Fields

        private static MidiDevicesSessionApi _api;

        #endregion

        #region Properties

        public static MidiDevicesSessionApi Api
        {
            get
            {
                if (_api == null)
                    _api = Is64Bit ? (MidiDevicesSessionApi)new MidiDevicesSessionApi64() : new MidiDevicesSessionApi32();

                return _api;
            }
        }

        #endregion
    }
}
