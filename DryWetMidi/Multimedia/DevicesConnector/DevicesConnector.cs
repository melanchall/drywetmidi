using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Provides a way to connect an input MIDI device to an output MIDI devices to redirect all
    /// incoming events from the input device to the output ones. More info in the
    /// <see href="xref:a_dev_connector">Devices connector</see> article.
    /// </summary>
    public sealed class DevicesConnector
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DevicesConnector"/> with the specified
        /// input and output MIDI devices.
        /// </summary>
        /// <remarks>
        /// <paramref name="inputDevice"/> will not be actually connected to <paramref name="outputDevices"/> after
        /// an instance of <see cref="DevicesConnector"/> is created. You must call <see cref="Connect"/> method
        /// to establish connection between devices.
        /// </remarks>
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
        public DevicesConnector(IInputDevice inputDevice, params IOutputDevice[] outputDevices)
        {
            ThrowIfArgument.IsNull(nameof(inputDevice), inputDevice);
            ThrowIfArgument.IsNull(nameof(outputDevices), outputDevices);
            ThrowIfArgument.ContainsNull(nameof(outputDevices), outputDevices);

            InputDevice = inputDevice;
            OutputDevices = outputDevices;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets an input MIDI device to connect to <see cref="OutputDevices"/>.
        /// </summary>
        public IInputDevice InputDevice { get; }

        /// <summary>
        /// Gets output MIDI devices to connect <see cref="InputDevice"/> to.
        /// </summary>
        public IReadOnlyCollection<IOutputDevice> OutputDevices { get; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="InputDevice"/> currently connected
        /// to <see cref="OutputDevices"/> or not (i.e. <see cref="Connect"/> method has been called).
        /// </summary>
        public bool AreDevicesConnected { get; private set; }

        /// <summary>
        /// Gets or sets a callback to process events coming from <see cref="InputDevice"/> before
        /// they will be sent to <see cref="OutputDevices"/>. The default value is <c>null</c> which
        /// means no processing will be applied.
        /// </summary>
        public DevicesConnectorEventCallback EventCallback { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Connects <see cref="InputDevice"/> to <see cref="OutputDevices"/> so all events coming from
        /// the input device will be redirected to the output ones.
        /// </summary>
        public void Connect()
        {
            if (AreDevicesConnected)
                return;

            InputDevice.EventReceived += OnEventReceived;
            AreDevicesConnected = true;
        }

        /// <summary>
        /// Disconnects <see cref="InputDevice"/> from <see cref="OutputDevices"/> so events coming from
        /// the input device will not be redirected to the output ones.
        /// </summary>
        public void Disconnect()
        {
            AreDevicesConnected = false;
            InputDevice.EventReceived -= OnEventReceived;
        }

        private void OnEventReceived(object sender, MidiEventReceivedEventArgs e)
        {
            if (!AreDevicesConnected)
                return;

            var inputMidiEvent = e.Event;
            var eventCallback = EventCallback;

            var midiEvent = eventCallback == null ? inputMidiEvent : eventCallback(inputMidiEvent);
            if (midiEvent == null)
                return;

            foreach (var outputDevice in OutputDevices)
            {
                if (AreDevicesConnected)
                    outputDevice.SendEvent(e.Event);
            }
        }

        #endregion
    }
}
