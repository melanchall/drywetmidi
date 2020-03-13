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
        /// <param name="inputDevice">Input MIDI device to connect to <paramref name="outputDevices"/>.</param>
        /// <param name="outputDevices">Output MIDI devices to connect <paramref name="inputDevice"/> to.</param>
        /// <exception cref="ArgumentNullException"><paramref name="inputDevice"/> is null. -or-
        /// <paramref name="outputDevices"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="outputDevices"/> contains null.</exception>
        public static DevicesConnector Connect(this InputDevice inputDevice, params IOutputDevice[] outputDevices)
        {
            ThrowIfArgument.IsNull(nameof(inputDevice), inputDevice);
            ThrowIfArgument.IsNull(nameof(outputDevices), outputDevices);

            var devicesConnector = new DevicesConnector(inputDevice, outputDevices);
            devicesConnector.Connect();
            return devicesConnector;
        }

        #endregion
    }
}
