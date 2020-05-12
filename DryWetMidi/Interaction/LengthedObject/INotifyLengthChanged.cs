using System;

namespace Melanchall.DryWetMidi.Interaction
{
    public interface INotifyLengthChanged
    {
        #region Events

        event EventHandler<LengthChangedEventArgs> LengthChanged;

        #endregion
    }
}
