using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a Pitch Bend Change message.
    /// </summary>
    /// <remarks>
    /// This message is sent to indicate a change in the pitch bender (wheel or lever, typically).
    /// The pitch bender is measured by a fourteen bit value. Center (no pitch change) is 0x2000.
    /// </remarks>
    public sealed class PitchBendEvent : ChannelEvent
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
                return DataTypesUtilities.Combine(this[PitchValueMsbParameterIndex],
                                                  this[PitchValueLsbParameterIndex]);
            }
            set
            {
                this[PitchValueLsbParameterIndex] = value.GetTail();
                this[PitchValueMsbParameterIndex] = value.GetHead();
            }
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Pitch Bend [{Channel}] ({PitchValue})";
        }

        #endregion
    }
}
