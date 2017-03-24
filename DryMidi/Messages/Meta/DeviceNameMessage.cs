using System;

namespace Melanchall.DryMidi
{
    public sealed class DeviceNameMessage : MetaMessage
    {
        #region Constructor

        public DeviceNameMessage()
        {
        }

        public DeviceNameMessage(string deviceName)
            : this()
        {
            DeviceName = deviceName;
        }

        #endregion

        #region Properties

        public string DeviceName { get; set; }

        #endregion

        #region Overrides

        internal override void ReadContent(MidiReader reader, ReadingSettings settings, int size = -1)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Non-negative size have to be specified in order to read Device Name message.");

            DeviceName = reader.ReadString(size);
        }

        internal override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteString(DeviceName);
        }

        internal override int GetContentSize()
        {
            return DeviceName?.Length ?? 0;
        }

        protected override Message CloneMessage()
        {
            return new DeviceNameMessage(DeviceName);
        }

        public override string ToString()
        {
            return $"Device Name message (device name = {DeviceName})";
        }

        #endregion
    }
}
