namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a Note On message.
    /// </summary>
    /// <remarks>
    /// This message is sent when a note is depressed (start).
    /// </remarks>
    public sealed class NoteOnEvent : ChannelEvent
    {
        #region Constants

        private const int ParametersCount = 2;
        private const int NoteNumberParameterIndex = 0;
        private const int VelocityParameterIndex = 1;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteOnEvent"/>.
        /// </summary>
        public NoteOnEvent()
            : base(ParametersCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteOnEvent"/> with the specified
        /// note number and velocity.
        /// </summary>
        /// <param name="noteNumber">Note number.</param>
        /// <param name="velocity">Velocity.</param>
        public NoteOnEvent(SevenBitNumber noteNumber, SevenBitNumber velocity)
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
            return $"Note On (channel = {Channel}, note number = {NoteNumber}, velocity = {Velocity})";
        }

        #endregion
    }
}
