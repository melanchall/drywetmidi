namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a Program Change message.
    /// </summary>
    /// <remarks>
    /// This message sent when the patch number changes.
    /// </remarks>
    public sealed class ProgramChangeEvent : ChannelEvent
    {
        #region Constants

        private const int ParametersCount = 1;
        private const int ProgramNumberParameterIndex = 0;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramChangeEvent"/>.
        /// </summary>
        public ProgramChangeEvent()
            : base(ParametersCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramChangeEvent"/> with the specified
        /// program number.
        /// </summary>
        /// <param name="programNumber">Program number.</param>
        public ProgramChangeEvent(SevenBitNumber programNumber)
            : this()
        {
            ProgramNumber = programNumber;
        }

        #endregion

        #region Properties

        public SevenBitNumber ProgramNumber
        {
            get { return _parameters[ProgramNumberParameterIndex]; }
            set { _parameters[ProgramNumberParameterIndex] = value; }
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"Program Change (channel = {Channel}, program number = {ProgramNumber})";
        }

        #endregion
    }
}
