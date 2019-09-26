using System;

namespace Melanchall.DryWetMidi.Devices
{
    public interface ITickGenerator : IDisposable
    {
        #region Events

        event EventHandler TickGenerated;

        #endregion

        #region Methods

        void TryStart();

        #endregion
    }
}
