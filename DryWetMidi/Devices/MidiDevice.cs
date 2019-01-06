using System;
using System.IO;
using System.Text;

namespace Melanchall.DryWetMidi.Devices
{
    public abstract class MidiDevice : IDisposable
    {
        #region Events

        public event EventHandler<ErrorOccurredEventArgs> ErrorOccurred;

        #endregion

        #region Fields

        protected readonly uint _id;
        protected IntPtr _handle = IntPtr.Zero;
        protected bool _disposed = false;

        #endregion

        #region Constructor

        internal MidiDevice(uint id)
        {
            _id = id;
        }

        #endregion

        #region Finalizer

        ~MidiDevice()
        {
            Dispose(false);
        }

        #endregion

        #region Properties

        public string Name { get; private set; }

        public Manufacturer DriverManufacturer { get; private set; }

        public ushort ProductIdentifier { get; private set; }

        public Version DriverVersion { get; private set; }

        #endregion

        #region Methods

        protected void SetBasicDeviceInformation(ushort manufacturerIdentifier, ushort productIdentifier, uint driverVersion, string name)
        {
            Name = name;
            DriverManufacturer = Enum.IsDefined(typeof(Manufacturer), manufacturerIdentifier)
                ? (Manufacturer)manufacturerIdentifier
                : Manufacturer.Unknown;
            ProductIdentifier = productIdentifier;

            var majorVersion = driverVersion >> 8;
            var minorVersion = driverVersion & 0xFF;
            DriverVersion = new Version((int)majorVersion, (int)minorVersion);
        }

        protected void EnsureDeviceIsNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("Device is disposed.");
        }

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

        protected void OnError(Exception exception)
        {
            ErrorOccurred?.Invoke(this, new ErrorOccurredEventArgs(exception));
        }

        protected abstract uint GetErrorText(uint mmrError, StringBuilder pszText, uint cchText);

        protected static void WriteBytesToStream(MemoryStream memoryStream, params byte[] bytes)
        {
            memoryStream.Seek(0, SeekOrigin.Begin);
            memoryStream.Write(bytes, 0, bytes.Length);
            memoryStream.Seek(0, SeekOrigin.Begin);
        }

        internal abstract IntPtr GetHandle();

        #endregion

        #region Overrides

        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);

        #endregion
    }
}
