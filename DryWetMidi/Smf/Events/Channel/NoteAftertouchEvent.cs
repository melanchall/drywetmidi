using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a Polyphonic Key Pressure (Aftertouch) message.
    /// </summary>
    /// <remarks>
    /// This message is most often sent by pressing down on the key after it "bottoms out".
    /// </remarks>
    public sealed class NoteAftertouchEvent : ChannelEvent
    {
        #region Constants

        private const int ParametersCount = 2;
        private const int NoteNumberParameterIndex = 0;
        private const int AftertouchValueParameterIndex = 1;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteAftertouchEvent"/>.
        /// </summary>
        public NoteAftertouchEvent()
            : base(ParametersCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteAftertouchEvent"/> with the specified
        /// note number and aftertouch (pressure) value.
        /// </summary>
        /// <param name="noteNumber">Note number.</param>
        /// <param name="aftertouchValue">Aftertouch (pressure) value.</param>
        public NoteAftertouchEvent(SevenBitNumber noteNumber, SevenBitNumber aftertouchValue)
            : this()
        {
            NoteNumber = noteNumber;
            AftertouchValue = aftertouchValue;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets note number.
        /// </summary>
        public SevenBitNumber NoteNumber
        {
            get { return this[NoteNumberParameterIndex]; }
            set { this[NoteNumberParameterIndex] = value; }
        }

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
            return $"Note Aftertouch [{Channel}] ({NoteNumber}, {AftertouchValue})";
        }

        #endregion
    }
}
