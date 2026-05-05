using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Configuration;
using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Represents a MIDI device.
    /// </summary>
    public abstract class MidiDevice : IDisposable
    {
        #region Nested enums

        internal enum CreationContext
        {
            User,
            VirtualDevice,
            RemovedDevice,
            AddedDevice
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when an error occurred on device (for example, during MIDI events parsing).
        /// </summary>
        public event EventHandler<ErrorOccurredEventArgs> ErrorOccurred;

        #endregion

        #region Constants

        private static readonly Dictionary<CreationContext, string> ContextsDescriptions = new Dictionary<CreationContext, string>
        {
            [CreationContext.User] = string.Empty,
            [CreationContext.VirtualDevice] = "subdevice of a virtual device",
            [CreationContext.AddedDevice] = "from 'Device added' notification",
            [CreationContext.RemovedDevice] = "from 'Device removed' notification",
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

        internal MidiDevice(CreationContext context)
        {
            Context = context;
        }

        #endregion

        #region Finalizer

        /// <summary>
        /// Finalizes the current instance of the MIDI device class.
        /// </summary>
        ~MidiDevice()
        {
            Dispose(false);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether a device is enabled (i.e. operable) or not.
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
        /// Gets the name of the current MIDI device.
        /// </summary>
        public abstract string Name { get; }

        public ParentDevice ParentDevice
        {
            get
            {
                if (!LibraryConfiguration.IsParentDeviceApiAvailable())
                    throw new PlatformNotSupportedException("Parent device API is not supported on the current operating system.");

                var result = DevicesCommonApi.Api_GetParentDeviceInfo((Info ?? Handle).DangerousGetHandle(), out var id, out var name, out var manufacturer, out var model);
                if (!result)
                    return null;

                return ParentDevice.Get(id, name, manufacturer, model);
            }
        }

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

        internal void EnsureDeviceIsNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("Device is disposed.");
        }

        internal void EnsureDeviceIsNotRemoved()
        {
            if (Context == CreationContext.RemovedDevice)
                throw new InvalidOperationException("Operation can't be performed on removed device.");
        }

        internal void OnError(Exception exception)
        {
            ErrorOccurred?.Invoke(this, new ErrorOccurredEventArgs(exception));
        }

        internal virtual void OnEnabledChanged(bool enabled)
        {
        }

        // TODO: check all calls, looks like some not needed
        internal static void EnsureSessionIsCreated()
        {
            MidiDevicesSession.GetSessionHandle();
        }

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
        /// Releases all resources used by the MIDI device class instance.
        /// </summary>
        public void Dispose()
        {
            if (Context == CreationContext.VirtualDevice)
                throw new InvalidOperationException("Disposing of a subdevice of a virtual device is prohibited.");

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the MIDI device class and optionally releases
        /// the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to
        /// release only unmanaged resources.</param>
        internal abstract void Dispose(bool disposing);

        #endregion
    }
}
