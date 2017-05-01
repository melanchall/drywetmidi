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
