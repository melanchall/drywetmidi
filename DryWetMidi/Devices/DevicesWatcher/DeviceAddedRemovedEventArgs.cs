using System;

namespace Melanchall.DryWetMidi.Devices
{
    public sealed class DeviceAddedRemovedEventArgs : EventArgs
    {
        #region Constructor

        public DeviceAddedRemovedEventArgs(MidiDevice device)
        {
            Device = device;
        }

        #endregion

        #region Properties

        public MidiDevice Device { get; }

        #endregion
    }
}
