using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Configuration;
using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Represents a MIDI endpoint.
    /// </summary>
    /// <remarks>
    /// <os-specific-api/>
    /// </remarks>
    public abstract class MidiEndpoint : IDisposable
    {
        #region Nested enums

        internal enum CreationContext
        {
            User,
            VirtualDevice,
            RemovedEndpoint,
            AddedEndpoint
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when an error occurred on endpoint (for example, during MIDI events parsing).
        /// </summary>
        public event EventHandler<ErrorOccurredEventArgs> ErrorOccurred;

        #endregion

        #region Constants

        private static readonly Dictionary<CreationContext, string> ContextsDescriptions = new Dictionary<CreationContext, string>
        {
            [CreationContext.User] = string.Empty,
            [CreationContext.VirtualDevice] = "endpoint of a virtual device",
            [CreationContext.AddedEndpoint] = "from 'Endpoint added' notification",
            [CreationContext.RemovedEndpoint] = "from 'Endpoint removed' notification",
        };

        #endregion

        #region Fields

        /// <summary>
        /// Flag to detect redundant disposing.
        /// </summary>
        protected bool _disposed = false;
        protected bool _enabled = true;

#if TEST
        private TestCheckpoints _testCheckpoints;
#endif

        #endregion

        #region Constructor

        internal MidiEndpoint(CreationContext context)
        {
            Context = context;
        }

        #endregion

        #region Finalizer

        /// <summary>
        /// Finalizes the current instance of the MIDI endpoint class.
        /// </summary>
        ~MidiEndpoint()
        {
            Dispose(false);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether an endpoint is enabled (i.e. operable) or not.
        /// </summary>
        public bool IsEnabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled == value)
                    return;

                _enabled = value;
                OnEnabledChanged(value);
            }
        }

        /// <summary>
        /// Gets the name of the current MIDI endpoint.
        /// </summary>
        public abstract string Name { get; }

        public abstract string Id { get; }

        internal CreationContext Context { get; }

        internal NativeHandle Handle { get; set; }

        internal NativeHandle Info { get; set; }

#if TEST
        internal TestCheckpoints TestCheckpoints
        {
            get { return _testCheckpoints; }
            set
            {
                _testCheckpoints = value;

                if (Handle != null)
                    Handle.TestCheckpoints = value;

                if (Info != null)
                    Info.TestCheckpoints = value;
            }
        }
#endif

        #endregion

        #region Methods

        public DeviceInformation GetDeviceInformation()
        {
            NativeApiUtilities.EnsureOsIsSupported();

            var result = DeviceApi.Api_GetDeviceInformation((Info ?? Handle).DangerousGetHandle(), MidiConfiguration.GetConfigurationHandle(), out var id, out var name, out var manufacturer, out var model, out var deviceDriver, out var errorCode);
            NativeApiUtilities.HandleEndpointNativeApiResult(result, errorCode);

            return DeviceInformation.Get(id, name, manufacturer, model, deviceDriver);
        }

        internal void EnsureEndpointIsNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("Endpoint is disposed.");
        }

        internal void EnsureEndpointIsNotRemoved()
        {
            if (Context == CreationContext.RemovedEndpoint)
                throw new InvalidOperationException("Operation can't be performed on removed endpoint.");
        }

        internal void OnError(Exception exception)
        {
            ErrorOccurred?.Invoke(this, new ErrorOccurredEventArgs(exception));
        }

        internal virtual void OnEnabledChanged(bool enabled)
        {
        }

        internal static MidiDevicesSessionHandle EnsureSessionIsCreated() =>
            MidiDevicesSession.GetSessionHandle();

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return ContextsDescriptions[Context];
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Releases all resources used by the MIDI endpoint class instance.
        /// </summary>
        public void Dispose()
        {
            if (Context == CreationContext.VirtualDevice)
                throw new InvalidOperationException("Disposing of an endpoint of a virtual device is prohibited.");

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the MIDI endpoint class and optionally releases
        /// the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to
        /// release only unmanaged resources.</param>
        internal abstract void Dispose(bool disposing);

        #endregion
    }
}
