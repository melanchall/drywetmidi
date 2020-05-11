using System;

namespace Melanchall.DryWetMidi.Interaction
{
    public interface INotifyTimeChanged
    {
        #region Events

        event EventHandler<TimeChangedEventArgs> TimeChanged;

        #endregion
    }
}
