using System.Collections.Concurrent;
using System.Linq;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Provides information about a MIDI device.
    /// </summary>
    /// <remarks>
    /// <os-specific-api/>
    /// </remarks>
    public sealed class DeviceInformation
    {
        #region Constants

        private static readonly ConcurrentDictionary<object, DeviceInformation> DevicesCache = new();

        #endregion

        #region Constructor

        private DeviceInformation(string id, string name, string? manufacturer, string? model, string? driverVersion)
        {
            Id = id;
            Name = name;
            Manufacturer = manufacturer;
            Model = model;
            DriverVersion = driverVersion;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the identifier of the device.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the name of the device.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the device manufacturer, if available.
        /// </summary>
        public string? Manufacturer { get; }

        /// <summary>
        /// Gets the device model, if available.
        /// </summary>
        public string? Model { get; }

        /// <summary>
        /// Gets the device driver version, if available.
        /// </summary>
        public string? DriverVersion { get; }

        #endregion

        #region Methods

        internal static DeviceInformation? Get(string id, string name, string? manufacturer, string? model, string? driverVersion)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            return DevicesCache.GetOrAdd(id, _ => new DeviceInformation(id, name, manufacturer, model, driverVersion));
        }

        #endregion

        #region Operators

        /// <summary>
        /// Determines whether two device information objects identify the same device.
        /// </summary>
        /// <param name="deviceInformation1">The first device information object.</param>
        /// <param name="deviceInformation2">The second device information object.</param>
        /// <returns><c>true</c> if both objects are <c>null</c> or have the same device identifier; otherwise, <c>false</c>.</returns>
        public static bool operator ==(DeviceInformation? deviceInformation1, DeviceInformation? deviceInformation2)
        {
            if (ReferenceEquals(deviceInformation1, deviceInformation2))
                return true;

            if (ReferenceEquals(null, deviceInformation1) || ReferenceEquals(null, deviceInformation2))
                return false;

            return deviceInformation1.Id == deviceInformation2.Id;
        }

        /// <summary>
        /// Determines whether two device information objects identify different devices.
        /// </summary>
        /// <param name="deviceInformation1">The first device information object.</param>
        /// <param name="deviceInformation2">The second device information object.</param>
        /// <returns><c>true</c> if the objects have different device identifiers; otherwise, <c>false</c>.</returns>
        public static bool operator !=(DeviceInformation? deviceInformation1, DeviceInformation? deviceInformation2)
        {
            return !(deviceInformation1 == deviceInformation2);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a hash code for this device information object.
        /// </summary>
        /// <returns>A hash code based on the device identifier.</returns>
        public override int GetHashCode() =>
            Id.GetHashCode();

        /// <summary>
        /// Determines whether this object and another object identify the same device.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns><c>true</c> if <paramref name="obj"/> is a <see cref="DeviceInformation"/> with the same device identifier; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj) =>
            obj is DeviceInformation device &&
            Id.Equals(device.Id);

        /// <summary>
        /// Returns a string that describes this device information object.
        /// </summary>
        /// <returns>A string containing the device name, identifier, and any available additional information.</returns>
        public override string ToString()
        {
            var manufacturerPart = string.IsNullOrEmpty(Manufacturer) ? string.Empty : $"manufacturer = {Manufacturer}";
            var modelPart = string.IsNullOrEmpty(Model) ? string.Empty : $"model = {Model}";
            var driverVersionPart = string.IsNullOrEmpty(DriverVersion) ? string.Empty : $"driver version = {DriverVersion}";

            var additionalInfoString = string.Join(
                ", ",
                new[] { manufacturerPart, modelPart, driverVersionPart }.Where(s => !string.IsNullOrEmpty(s)));

            return $"{Name} (ID = {Id}{(string.IsNullOrEmpty(additionalInfoString) ? string.Empty : $", {additionalInfoString}")})";
        }

        #endregion
    }
}
