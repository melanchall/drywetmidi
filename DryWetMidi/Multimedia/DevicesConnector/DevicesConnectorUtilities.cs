using System;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Provides methods to connect MIDI devices. More info in the
    /// <see href="xref:a_dev_connector">Devices connector</see> article.
    /// </summary>
    public static class DevicesConnectorUtilities
    {
        #region Methods

        /// <summary>
        /// Connects an input device to the specified output devices.
        /// </summary>
        /// <param name="inputDevice">Input MIDI device to connect to <paramref name="outputDevices"/>.</param>
        /// <param name="outputDevices">Output MIDI devices to connect <paramref name="inputDevice"/> to.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="inputDevice"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="outputDevices"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="outputDevices"/> contains <c>null</c>.</exception>
        public static DevicesConnector Connect(this IInputDevice inputDevice, params IOutputDevice[] outputDevices)
        {
            ThrowIfArgument.IsNull(nameof(inputDevice), inputDevice);
            ThrowIfArgument.IsNull(nameof(outputDevices), outputDevices);
            ThrowIfArgument.ContainsNull(nameof(outputDevices), outputDevices);

            var devicesConnector = new DevicesConnector(inputDevice, outputDevices);
            devicesConnector.Connect();
            return devicesConnector;
        }

        #endregion
    }
}
