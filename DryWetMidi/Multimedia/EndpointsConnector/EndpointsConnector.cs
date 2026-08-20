using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Provides a way to connect an input MIDI endpoint to output MIDI endpoints to redirect all
    /// incoming events from the input endpoint to the output endpoints. More info in the
    /// <see href="xref:a_dev_connector">Endpoints connector</see> article.
    /// </summary>
    public sealed class EndpointsConnector
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="EndpointsConnector"/> with the specified
        /// input and output MIDI endpoints.
        /// </summary>
        /// <remarks>
        /// <paramref name="inputEndpoint"/> will not be actually connected to <paramref name="outputEndpoints"/> after
        /// an instance of <see cref="EndpointsConnector"/> is created. You must call <see cref="Connect"/> method
        /// to establish connection between endpoints.
        /// </remarks>
        /// <param name="inputEndpoint">Input MIDI endpoint to connect to <paramref name="outputEndpoints"/>.</param>
        /// <param name="outputEndpoints">Output MIDI endpoints to connect <paramref name="inputEndpoint"/> to.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="inputEndpoint"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="outputEndpoints"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="outputEndpoints"/> contains <c>null</c>.</exception>
        public EndpointsConnector(IInputEndpoint inputEndpoint, params IOutputEndpoint[] outputEndpoints)
        {
            ThrowIfArgument.IsNull(nameof(inputEndpoint), inputEndpoint);
            ThrowIfArgument.IsNull(nameof(outputEndpoints), outputEndpoints);
            ThrowIfArgument.ContainsNull(nameof(outputEndpoints), outputEndpoints);

            InputEndpoint = inputEndpoint;
            OutputEndpoints = outputEndpoints;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets an input MIDI endpoint to connect to <see cref="OutputEndpoints"/>.
        /// </summary>
        public IInputEndpoint InputEndpoint { get; }

        /// <summary>
        /// Gets output MIDI endpoints to connect <see cref="InputEndpoint"/> to.
        /// </summary>
        public IReadOnlyCollection<IOutputEndpoint> OutputEndpoints { get; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="InputEndpoint"/> currently connected
        /// to <see cref="OutputEndpoints"/> or not (i.e. <see cref="Connect"/> method has been called).
        /// </summary>
        public bool AreEndpointsConnected { get; private set; }

        /// <summary>
        /// Gets or sets a callback to process events coming from <see cref="InputEndpoint"/> before
        /// they will be sent to <see cref="OutputEndpoints"/>. The default value is <c>null</c> which
        /// means no processing will be applied.
        /// </summary>
        public EndpointsConnectorEventCallback? EventCallback { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Connects <see cref="InputEndpoint"/> to <see cref="OutputEndpoints"/> so all events coming from
        /// the input endpoint will be redirected to the output endpoints.
        /// </summary>
        public void Connect()
        {
            if (AreEndpointsConnected)
                return;

            InputEndpoint.EventReceived += OnEventReceived;
            AreEndpointsConnected = true;
        }

        /// <summary>
        /// Disconnects <see cref="InputEndpoint"/> from <see cref="OutputEndpoints"/> so events coming from
        /// the input endpoint will not be redirected to the output endpoints.
        /// </summary>
        public void Disconnect()
        {
            AreEndpointsConnected = false;
            InputEndpoint.EventReceived -= OnEventReceived;
        }

        private void OnEventReceived(object? sender, MidiEventReceivedEventArgs e)
        {
            if (!AreEndpointsConnected)
                return;

            var inputMidiEvent = e.Event;
            var eventCallback = EventCallback;

            var midiEvent = eventCallback == null ? inputMidiEvent : eventCallback(inputMidiEvent);
            if (midiEvent == null)
                return;

            foreach (var outputEndpoint in OutputEndpoints)
            {
                if (AreEndpointsConnected)
                    outputEndpoint.SendEvent(e.Event);
            }
        }

        #endregion
    }
}
