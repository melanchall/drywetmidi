using System;

namespace Melanchall.DryMidi
{
    public sealed class DeviceNameEvent : MetaEvent
    {
        #region Constructor

        public DeviceNameEvent()
        {
        }

        public DeviceNameEvent(string deviceName)
            : this()
        {
            DeviceName = deviceName;
        }

        #endregion

        #region Properties

        public string DeviceName { get; set; }

        #endregion

        #region Methods

        public bool Equals(DeviceNameEvent deviceNameEvent)
        {
            if (ReferenceEquals(null, deviceNameEvent))
                return false;

            if (ReferenceEquals(this, deviceNameEvent))
                return true;

            return base.Equals(deviceNameEvent) && DeviceName == deviceNameEvent.DeviceName;
        }

        #endregion

        #region Overrides

        protected override void ReadContentData(MidiReader reader, ReadingSettings settings, int size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Non-negative size have to be specified in order to read Device Name event.");

            DeviceName = reader.ReadString(size);
        }

        protected override void WriteContentData(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteString(DeviceName);
        }

        protected override int GetContentDataSize()
        {
            return DeviceName?.Length ?? 0;
        }

        protected override MidiEvent CloneEvent()
        {
            return new DeviceNameEvent(DeviceName);
        }

        public override string ToString()
        {
            return $"Device Name (device name = {DeviceName})";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DeviceNameEvent);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ (DeviceName?.GetHashCode() ?? 0);
        }

        #endregion
    }
}
