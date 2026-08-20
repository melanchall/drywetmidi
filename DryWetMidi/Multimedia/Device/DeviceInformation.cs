using System.Collections.Concurrent;
using System.Linq;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// // TODO
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

        public string Id { get; }

        public string Name { get; }

        public string? Manufacturer { get; }

        public string? Model { get; }

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

        public static bool operator ==(DeviceInformation? deviceInformation1, DeviceInformation? deviceInformation2)
        {
            if (ReferenceEquals(deviceInformation1, deviceInformation2))
                return true;

            if (ReferenceEquals(null, deviceInformation1) || ReferenceEquals(null, deviceInformation2))
                return false;

            return deviceInformation1.Id == deviceInformation2.Id;
        }

        public static bool operator !=(DeviceInformation? deviceInformation1, DeviceInformation? deviceInformation2)
        {
            return !(deviceInformation1 == deviceInformation2);
        }

        #endregion

        #region Overrides

        public override int GetHashCode() =>
            Id.GetHashCode();

        public override bool Equals(object? obj) =>
            obj is DeviceInformation device &&
            Id.Equals(device.Id);

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
