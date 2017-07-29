using System;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a Control Change message.
    /// </summary>
    /// <remarks>
    /// This message is sent when a controller value changes. Controllers include devices
    /// such as pedals and levers.
    /// </remarks>
    public sealed class ControlChangeEvent : ChannelEvent, IEquatable<ControlChangeEvent>
    {
        #region Constants

        private const int ParametersCount = 2;
        private const int ControlNumberParameterIndex = 0;
        private const int ControlValueParameterIndex = 1;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlChangeEvent"/>.
        /// </summary>
        public ControlChangeEvent()
            : base(ParametersCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlChangeEvent"/> with the specified
        /// controller number and controller value.
        /// </summary>
        /// <param name="controlNumber">Controller number.</param>
        /// <param name="controlValue">Controller value.</param>
        public ControlChangeEvent(SevenBitNumber controlNumber, SevenBitNumber controlValue)
            : this()
        {
            ControlNumber = controlNumber;
            ControlValue = controlValue;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets controller number.
        /// </summary>
        public SevenBitNumber ControlNumber
        {
            get { return this[ControlNumberParameterIndex]; }
            set { this[ControlNumberParameterIndex] = value; }
        }

        /// <summary>
        /// Gets or sets controller value.
        /// </summary>
        public SevenBitNumber ControlValue
        {
            get { return this[ControlValueParameterIndex]; }
            set { this[ControlValueParameterIndex] = value; }
        }

        #endregion

        #region IEquatable<ControlChangeEvent>

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="controlChangeEvent">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
        public bool Equals(ControlChangeEvent controlChangeEvent)
        {
            return Equals(controlChangeEvent, true);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="midiEvent">The event to compare with the current one.</param>
        /// <param name="respectDeltaTime">If true the delta-times will be taken into an account
        /// while comparing events; if false - delta-times will be ignored.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public override bool Equals(MidiEvent midiEvent, bool respectDeltaTime)
        {
            return Equals(midiEvent as ControlChangeEvent, respectDeltaTime);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Control Change [{Channel}] ({ControlNumber}, {ControlValue})";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as ControlChangeEvent);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return CalculateHashCode(EventStatusBytes.Channel.ControlChange);
        }

        #endregion
    }
}
