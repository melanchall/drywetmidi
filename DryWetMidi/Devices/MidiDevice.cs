using System;
using System.Text;

namespace Melanchall.DryWetMidi.Devices
{
    /// <summary>
    /// Represents a MIDI device.
    /// </summary>
    public abstract class MidiDevice : IDisposable
    {
        #region Events

        /// <summary>
        /// Occurs when an error occurred on device (for example, during MIDI events parsing).
        /// </summary>
        public event EventHandler<ErrorOccurredEventArgs> ErrorOccurred;

        #endregion

        #region Fields

        /// <summary>
        /// Device handle.
        /// </summary>
        protected IntPtr _windowsHandle = IntPtr.Zero;

        protected IntPtr _info = IntPtr.Zero;
        protected IntPtr _handle = IntPtr.Zero;

        /// <summary>
        /// Flag to detect redundant disposing.
        /// </summary>
        protected bool _disposed = false;

        #endregion

        #region Constructor

        internal MidiDevice(IntPtr info)
        {
            _info = info;
            SetBasicDeviceInformation();
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
        public string Name { get; protected set; }

        /// <summary>
        /// Gets the manufacturer of MIDI device driver.
        /// </summary>
        public string Manufacturer { get; protected set; }

        /// <summary>
        /// Gets the product identifier of MIDI device.
        /// </summary>
        public string Product { get; protected set; }

        // TODO: parse
        /// <summary>
        /// Gets the version of MIDI device driver.
        /// </summary>
        public uint DriverVersion { get; protected set; }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the basic information about MIDI device, such as name and driver details.
        /// </summary>
        protected abstract void SetBasicDeviceInformation();

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
        /// Processes MMRESULT which is return value of winmm functions.
        /// </summary>
        /// <param name="mmResult">MMRESULT which is return value of winmm function.</param>
        /// <exception cref="MidiDeviceException"><paramref name="mmResult"/> represents error code.</exception>
        protected void ProcessMmResult(uint mmResult)
        {
            if (mmResult == MidiWinApi.MMSYSERR_NOERROR)
                return;

            var stringBuilder = new StringBuilder((int)MidiWinApi.MaxErrorLength);
            var getErrorTextResult = GetErrorText(mmResult, stringBuilder, MidiWinApi.MaxErrorLength + 1);
            if (getErrorTextResult != MidiWinApi.MMSYSERR_NOERROR)
                throw new MidiDeviceException("Error occured during operation on device.");

            var errorText = stringBuilder.ToString();
            throw new MidiDeviceException(errorText);
        }

        /// <summary>
        /// Raises <see cref="ErrorOccurred"/> event.
        /// </summary>
        /// <param name="exception">An exception that represents error occurred.</param>
        protected void OnError(Exception exception)
        {
            ErrorOccurred?.Invoke(this, new ErrorOccurredEventArgs(exception));
        }

        /// <summary>
        /// Gets error description for the specified MMRESULT which is return value of winmm function.
        /// </summary>
        /// <param name="mmrError">MMRESULT which is return value of winmm function.</param>
        /// <param name="pszText"><see cref="StringBuilder"/> to write error description to.</param>
        /// <param name="cchText">Size of <paramref name="pszText"/> buffer.</param>
        /// <returns>Return value of winmm function which gets error description.</returns>
        protected abstract uint GetErrorText(uint mmrError, StringBuilder pszText, uint cchText);

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
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the MIDI device class and optionally releases
        /// the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to
        /// release only unmanaged resources.</param>
        protected abstract void Dispose(bool disposing);

        #endregion
    }
}
