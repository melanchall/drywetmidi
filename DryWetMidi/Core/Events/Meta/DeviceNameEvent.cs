namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents a Device Name meta event.
    /// </summary>
    /// <remarks>
    /// This optional event is used to identify the hardware device used to produce
    /// sounds for this track.
    /// </remarks>
    public sealed class DeviceNameEvent : BaseTextEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceNameEvent"/>.
        /// </summary>
        public DeviceNameEvent()
            : base(MidiEventType.DeviceName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceNameEvent"/> with the
        /// specified device name.
        /// </summary>
        /// <param name="deviceName">Name of the device.</param>
        public DeviceNameEvent(string deviceName)
            : base(MidiEventType.DeviceName, deviceName)
        {
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Clones event by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the event.</returns>
        protected override MidiEvent CloneEvent()
        {
            return new DeviceNameEvent(Text);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Device Name ({Text})";
        }

        #endregion
    }
}
