using System;
using System.IO;
using System.Text;

namespace Melanchall.DryWetMidi.Devices
{
    public abstract class MidiDevice : IDisposable
    {
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

        protected void SetBasicDeviceInformation(ushort manufacturerIdentifier,
                                          ushort productIdentifier,
                                          uint driverVersion,
                                          string name)
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
