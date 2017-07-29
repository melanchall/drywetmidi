using System;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a Device Name meta event.
    /// </summary>
    /// <remarks>
    /// This optional event is used to identify the hardware device used to produce
    /// sounds for this track.
    /// </remarks>
    public sealed class DeviceNameEvent : BaseTextEvent, IEquatable<DeviceNameEvent>
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceNameEvent"/>.
        /// </summary>
        public DeviceNameEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceNameEvent"/> with the
        /// specified device name.
        /// </summary>
        /// <param name="deviceName">Name of the device.</param>
        public DeviceNameEvent(string deviceName)
            : base(deviceName)
        {
        }

        #endregion

        #region IEquatable<DeviceNameEvent>

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="deviceNameEvent">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
        public bool Equals(DeviceNameEvent deviceNameEvent)
        {
            return Equals(deviceNameEvent, true);
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
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="midiEvent">The event to compare with the current one.</param>
        /// <param name="respectDeltaTime">If true the delta-times will be taken into an account
        /// while comparing events; if false - delta-times will be ignored.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public override bool Equals(MidiEvent midiEvent, bool respectDeltaTime)
        {
            return Equals(midiEvent as DeviceNameEvent, respectDeltaTime);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Device Name ({Text})";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as DeviceNameEvent);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }
}
