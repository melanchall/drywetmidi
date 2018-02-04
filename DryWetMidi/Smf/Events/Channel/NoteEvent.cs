using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Base class that represents a Note On or a Note Off message.
    /// </summary>
    public abstract class NoteEvent : ChannelEvent
    {
        #region Constants

        private const int ParametersCount = 2;
        private const int NoteNumberParameterIndex = 0;
        private const int VelocityParameterIndex = 1;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteEvent"/>.
        /// </summary>
        public NoteEvent()
            : base(ParametersCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteEvent"/> with the specified
        /// note number and velocity.
        /// </summary>
        /// <param name="noteNumber">Note number.</param>
        /// <param name="velocity">Velocity.</param>
        public NoteEvent(SevenBitNumber noteNumber, SevenBitNumber velocity)
            : this()
        {
            NoteNumber = noteNumber;
            Velocity = velocity;
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
        /// Gets or sets velocity.
        /// </summary>
        public SevenBitNumber Velocity
        {
            get { return this[VelocityParameterIndex]; }
            set { this[VelocityParameterIndex] = value; }
        }

        #endregion
    }
}
