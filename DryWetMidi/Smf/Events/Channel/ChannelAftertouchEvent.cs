using System;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a Channel Pressure (Aftertouch) message.
    /// </summary>
    /// <remarks>
    /// This message is most often sent by pressing down on the key after it "bottoms out".
    /// This message is different from polyphonic after-touch. Use this message to send the
    /// single greatest pressure value (of all the current depressed keys).
    /// </remarks>
    public sealed class ChannelAftertouchEvent : ChannelEvent, IEquatable<ChannelAftertouchEvent>
    {
        #region Constants

        private const int ParametersCount = 1;
        private const int AftertouchValueParameterIndex = 0;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelAftertouchEvent"/>.
        /// </summary>
        public ChannelAftertouchEvent()
            : base(ParametersCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelAftertouchEvent"/> with the specified
        /// aftertouch (pressure) value.
        /// </summary>
        /// <param name="aftertouchValue">Aftertouch (pressure) value.</param>
        public ChannelAftertouchEvent(SevenBitNumber aftertouchValue)
            : this()
        {
            AftertouchValue = aftertouchValue;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets aftertouch (pressure) value.
        /// </summary>
        public SevenBitNumber AftertouchValue
        {
            get { return this[AftertouchValueParameterIndex]; }
            set { this[AftertouchValueParameterIndex] = value; }
        }

        #endregion

        #region IEquatable<ChannelAftertouchEvent>

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="channelAftertouchEvent">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
        public bool Equals(ChannelAftertouchEvent channelAftertouchEvent)
        {
            return Equals(channelAftertouchEvent, true);
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
            return Equals(midiEvent as ChannelAftertouchEvent, respectDeltaTime);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Channel Aftertouch [{Channel}] ({AftertouchValue})";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as ChannelAftertouchEvent);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return CalculateHashCode(EventStatusBytes.Channel.ChannelAftertouch);
        }

        #endregion
    }
}
