namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a Note Off message.
    /// </summary>
    /// <remarks>
    /// This message is sent when a note is released (ended).
    /// </remarks>
    public sealed class NoteOffEvent : ChannelEvent
    {
        #region Constants

        private const int ParametersCount = 2;
        private const int NoteNumberParameterIndex = 0;
        private const int VelocityParameterIndex = 1;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteOffEvent"/>.
        /// </summary>
        public NoteOffEvent()
            : base(ParametersCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteOffEvent"/> with the specified
        /// note number and velocity.
        /// </summary>
        /// <param name="noteNumber">Note number.</param>
        /// <param name="velocity">Velocity.</param>
        public NoteOffEvent(SevenBitNumber noteNumber, SevenBitNumber velocity)
            : this()
        {
            NoteNumber = noteNumber;
            Velocity = velocity;
        }

        #endregion

        #region Properties

        public SevenBitNumber NoteNumber
        {
            get { return _parameters[NoteNumberParameterIndex]; }
            set { _parameters[NoteNumberParameterIndex] = value; }
        }

        public SevenBitNumber Velocity
        {
            get { return _parameters[VelocityParameterIndex]; }
            set { _parameters[VelocityParameterIndex] = value; }
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"Note Off (channel = {Channel}, note number = {NoteNumber}, velocity = {Velocity})";
        }

        #endregion
    }
}
