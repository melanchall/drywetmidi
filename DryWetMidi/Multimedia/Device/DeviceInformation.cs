using System.Collections.Concurrent;
using System.Linq;

namespace Melanchall.DryWetMidi.Multimedia
{
    public sealed class DeviceInformation
    {
        #region Constants

        private static readonly ConcurrentDictionary<object, DeviceInformation> DevicesCache = new();

        #endregion

        #region Constructor

        private DeviceInformation(string id, string name, string manufacturer, string model)
        {
            Id = id;
            Name = name;
            Manufacturer = manufacturer;
            Model = model;
        }

        #endregion

        #region Properties

        public string Id { get; }

        public string Name { get; }

        public string Manufacturer { get; }

        public string Model { get; }

        #endregion

        #region Methods

        internal static DeviceInformation Get(string id, string name, string manufacturer, string model)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            return DevicesCache.GetOrAdd(id, _ => new DeviceInformation(id, name, manufacturer, model));
        }

        #endregion

        #region Overrides

        public override int GetHashCode() =>
            Id.GetHashCode();

        public override bool Equals(object obj) =>
            obj is DeviceInformation device &&
            Id.Equals(device.Id);

        public override string ToString()
        {
            var manufacturerPart = string.IsNullOrEmpty(Manufacturer) ? string.Empty : $"manufacturer = {Manufacturer}";
            var modelPart = string.IsNullOrEmpty(Model) ? string.Empty : $"model = {Model}";

            var additionalInfoString = string.Join(
                ", ",
                new[] { manufacturerPart, modelPart }.Where(s => !string.IsNullOrEmpty(s)));

            return $"{Name} (ID = {Id}{(string.IsNullOrEmpty(additionalInfoString) ? string.Empty : $", {additionalInfoString}")})";
        }

        #endregion
    }
}
