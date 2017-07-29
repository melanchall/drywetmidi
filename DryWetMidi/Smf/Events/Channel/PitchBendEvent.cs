using System;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a Pitch Bend Change message.
    /// </summary>
    /// <remarks>
    /// This message is sent to indicate a change in the pitch bender (wheel or lever, typically).
    /// The pitch bender is measured by a fourteen bit value. Center (no pitch change) is 0x2000.
    /// </remarks>
    public sealed class PitchBendEvent : ChannelEvent, IEquatable<PitchBendEvent>
    {
        #region Constants

        private const int ParametersCount = 2;
        private const int PitchValueLsbParameterIndex = 0;
        private const int PitchValueMsbParameterIndex = 1;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PitchBendEvent"/>.
        /// </summary>
        public PitchBendEvent()
            : base(ParametersCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PitchBendEvent"/> with the specified
        /// pitch value.
        /// </summary>
        /// <param name="pitchValue">Pitch value.</param>
        public PitchBendEvent(ushort pitchValue)
            : this()
        {
            PitchValue = pitchValue;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets pitch value.
        /// </summary>
        public ushort PitchValue
        {
            get
            {
                return DataTypesUtilities.Combine(this[PitchValueLsbParameterIndex],
                                                  this[PitchValueMsbParameterIndex]);
            }
            set
            {
                this[PitchValueLsbParameterIndex] = value.GetHead();
                this[PitchValueMsbParameterIndex] = value.GetTail();
            }
        }

        #endregion

        #region IEquatable<PitchBendEvent>

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="pitchBendEvent">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
        public bool Equals(PitchBendEvent pitchBendEvent)
        {
            return Equals(pitchBendEvent, true);
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
            return Equals(midiEvent as PitchBendEvent, respectDeltaTime);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Pitch Bend [{Channel}] ({PitchValue})";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as PitchBendEvent);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return CalculateHashCode(EventStatusBytes.Channel.PitchBend);
        }

        #endregion
    }
}
