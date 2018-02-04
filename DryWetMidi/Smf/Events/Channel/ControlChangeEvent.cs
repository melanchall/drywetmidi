using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a Control Change message.
    /// </summary>
    /// <remarks>
    /// This message is sent when a controller value changes. Controllers include devices
    /// such as pedals and levers.
    /// </remarks>
    public sealed class ControlChangeEvent : ChannelEvent
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

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Control Change [{Channel}] ({ControlNumber}, {ControlValue})";
        }

        #endregion
    }
}
