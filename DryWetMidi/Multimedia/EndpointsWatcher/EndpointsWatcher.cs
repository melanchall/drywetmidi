using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Configuration;
using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Provides a way to watch endpoints adding/removing in the system. More info in the
    /// <see href="xref:a_dev_watcher">Endpoints watcher</see> article.
    /// </summary>
    /// <remarks>
    /// <os-specific-api/>
    /// <advanced-windows-api/>
    /// </remarks>
    public sealed class EndpointsWatcher
    {
        #region Events

        /// <summary>
        /// Occurs when a MIDI endpoint has been added to the system.
        /// </summary>
        public event EventHandler<EndpointAddedRemovedEventArgs>? EndpointAdded;

        /// <summary>
        /// Occurs when a MIDI endpoint has been removed from the system.
        /// </summary>
        public event EventHandler<EndpointAddedRemovedEventArgs>? EndpointRemoved;

        #endregion

        #region Fields

        private static readonly Lazy<EndpointsWatcher> _instance =
            new(() => new EndpointsWatcher());

        #endregion

        #region Constructor

        private EndpointsWatcher()
        {
            MidiDevicesSession.GetSessionHandle();

            MidiDevicesSession.InputEndpointAdded += OnInputEndpointAdded;
            MidiDevicesSession.InputEndpointRemoved += OnInputEndpointRemoved;
            MidiDevicesSession.OutputEndpointAdded += OnOutputEndpointAdded;
            MidiDevicesSession.OutputEndpointRemoved += OnOutputEndpointRemoved;

            AppDomain.CurrentDomain.DomainUnload += OnDomainUnloadOrExit;
            AppDomain.CurrentDomain.ProcessExit += OnDomainUnloadOrExit;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the instance of <see cref="EndpointsWatcher"/>.
        /// </summary>
        /// <exception cref="FeatureNotAvailableException">Endpoints watcher API is not available.</exception>
        public static EndpointsWatcher Instance
        {
            get
            {
                if (!LibraryConfiguration.IsEndpointsWatcherApiAvailable())
                    throw new FeatureNotAvailableException("Endpoints watcher API is not available.");

                return _instance.Value;
            }
        }

        #endregion

        #region Methods


        // TODO: need it?
        private void OnDomainUnloadOrExit(object? sender, EventArgs e)
        {
            // TODO: remove all event handlers of the instance?

            MidiDevicesSession.InputEndpointAdded -= OnInputEndpointAdded;
            MidiDevicesSession.InputEndpointRemoved -= OnInputEndpointRemoved;
            MidiDevicesSession.OutputEndpointAdded -= OnOutputEndpointAdded;
            MidiDevicesSession.OutputEndpointRemoved -= OnOutputEndpointRemoved;
        }

        private void OnInputEndpointAdded(object? sender, IntPtr info)
        {
            var endpointAddded = EndpointAdded;
            if (endpointAddded != null)
                endpointAddded.Invoke(this, new EndpointAddedRemovedEventArgs(new InputEndpoint(info, MidiEndpoint.CreationContext.AddedEndpoint)));
            else
                InputEndpointApi.Api_DeleteEndpointInfo(info);
        }

        private void OnInputEndpointRemoved(object? sender, IntPtr info)
        {
            var endpointRemoved = EndpointRemoved;
            if (endpointRemoved != null)
                endpointRemoved.Invoke(this, new EndpointAddedRemovedEventArgs(new InputEndpoint(info, MidiEndpoint.CreationContext.RemovedEndpoint)));
            else
                InputEndpointApi.Api_DeleteEndpointInfo(info);
        }

        private void OnOutputEndpointAdded(object? sender, IntPtr info)
        {
            var endpointAdded = EndpointAdded;
            if (endpointAdded != null)
                endpointAdded.Invoke(this, new EndpointAddedRemovedEventArgs(new OutputEndpoint(info, MidiEndpoint.CreationContext.AddedEndpoint)));
            else
                OutputEndpointApi.Api_DeleteEndpointInfo(info);
        }

        private void OnOutputEndpointRemoved(object? sender, IntPtr info)
        {
            var endpointRemoved = EndpointRemoved;
            if (endpointRemoved != null)
                endpointRemoved.Invoke(this, new EndpointAddedRemovedEventArgs(new OutputEndpoint(info, MidiEndpoint.CreationContext.RemovedEndpoint)));
            else
                OutputEndpointApi.Api_DeleteEndpointInfo(info);
        }

        #endregion
    }
}
