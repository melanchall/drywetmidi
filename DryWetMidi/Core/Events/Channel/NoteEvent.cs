using System.ComponentModel;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
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
        protected NoteEvent(MidiEventType eventType)
            : base(eventType, ParametersCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteEvent"/> with the specified
        /// note number and velocity.
        /// </summary>
        /// <param name="eventType">The type of event.</param>
        /// <param name="noteNumber">Note number.</param>
        /// <param name="velocity">Velocity.</param>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="eventType"/> specified an invalid value.</exception>
        protected NoteEvent(MidiEventType eventType, SevenBitNumber noteNumber, SevenBitNumber velocity)
            : this(eventType)
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
