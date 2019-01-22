using System;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Devices
{
    /// <summary>
    /// Provides methods to connect MIDI devices.
    /// </summary>
    public static class DevicesConnectorUtilities
    {
        #region Methods

        /// <summary>
        /// Connects an input device to the specified output device.
        /// </summary>
        /// <param name="inputDevice">Input MIDI device to connect to <paramref name="outputDevice"/>.</param>
        /// <param name="outputDevice">Output MIDI device to connect <paramref name="inputDevice"/> to.</param>
        /// <exception cref="ArgumentNullException"><paramref name="inputDevice"/> is null. -or-
        /// <paramref name="outputDevice"/> is null.</exception>
        /// <exception cref="MidiDeviceException"><see cref="InputDevice"/> is already connected
        /// to <see cref="OutputDevice"/>.</exception>
        public static DevicesConnector Connect(this InputDevice inputDevice, OutputDevice outputDevice)
        {
            ThrowIfArgument.IsNull(nameof(inputDevice), inputDevice);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            var devicesConnector = new DevicesConnector(inputDevice, outputDevice);
            devicesConnector.Connect();
            return devicesConnector;
        }

        #endregion
    }
}
