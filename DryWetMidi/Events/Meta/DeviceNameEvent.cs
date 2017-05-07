namespace Melanchall.DryWetMidi
{
    public sealed class DeviceNameEvent : BaseTextEvent
    {
        #region Constructor

        public DeviceNameEvent()
        {
        }

        public DeviceNameEvent(string deviceName)
            : base(deviceName)
        {
        }

        #endregion

        #region Methods

        public bool Equals(DeviceNameEvent deviceNameEvent)
        {
            return Equals(deviceNameEvent, true);
        }

        public bool Equals(DeviceNameEvent deviceNameEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, deviceNameEvent))
                return false;

            if (ReferenceEquals(this, deviceNameEvent))
                return true;

            return base.Equals(deviceNameEvent, respectDeltaTime);
        }

        #endregion

        #region Overrides

        protected override MidiEvent CloneEvent()
        {
            return new DeviceNameEvent(Text);
        }

        public override string ToString()
        {
            return $"Device Name (device name = {Text})";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DeviceNameEvent);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }
}
