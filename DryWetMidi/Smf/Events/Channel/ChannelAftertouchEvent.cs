using Melanchall.DryWetMidi.Common;

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
    public sealed class ChannelAftertouchEvent : ChannelEvent
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

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Channel Aftertouch [{Channel}] ({AftertouchValue})";
        }

        #endregion
    }
}
