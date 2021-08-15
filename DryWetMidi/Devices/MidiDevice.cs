using System;

namespace Melanchall.DryWetMidi.Devices
{
    /// <summary>
    /// Represents a MIDI device.
    /// </summary>
    public abstract class MidiDevice : IDisposable
    {
        #region Nested enums

        internal enum DeviceOwner
        {
            User,
            VirtualDevice
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when an error occurred on device (for example, during MIDI events parsing).
        /// </summary>
        public event EventHandler<ErrorOccurredEventArgs> ErrorOccurred;

        #endregion

        #region Fields

        protected IntPtr _info = IntPtr.Zero;
        protected IntPtr _handle = IntPtr.Zero;

        /// <summary>
        /// Flag to detect redundant disposing.
        /// </summary>
        protected bool _disposed = false;

        #endregion

        #region Constructor

        internal MidiDevice()
            : this(IntPtr.Zero)
        {
        }

        internal MidiDevice(IntPtr info)
            : this(info, DeviceOwner.User)
        {
        }

        internal MidiDevice(IntPtr info, DeviceOwner owner)
        {
            _info = info;
            Owner = owner;
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
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets the name of MIDI device.
        /// </summary>
        public abstract string Name { get; }

        internal DeviceOwner Owner { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Checks that current instance of MIDI device class is not disposed and throws
        /// <see cref="ObjectDisposedException"/> if not.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Current instance of MIDI device class is disposed.</exception>
        protected void EnsureDeviceIsNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("Device is disposed.");
        }

        /// <summary>
        /// Raises <see cref="ErrorOccurred"/> event.
        /// </summary>
        /// <param name="exception">An exception that represents error occurred.</param>
        protected void OnError(Exception exception)
        {
            ErrorOccurred?.Invoke(this, new ErrorOccurredEventArgs(exception));
        }

        protected static void EnsureSessionIsCreated()
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
            return Name;
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Releases all resources used by the MIDI device class instance.
        /// </summary>
        public void Dispose()
        {
            if (Owner != DeviceOwner.User)
                throw new InvalidOperationException($"Disposing of a device owned by {Owner} is prohibited.");

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
