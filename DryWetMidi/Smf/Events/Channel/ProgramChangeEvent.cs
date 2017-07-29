using System;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a Program Change message.
    /// </summary>
    /// <remarks>
    /// This message sent when the patch number changes.
    /// </remarks>
    public sealed class ProgramChangeEvent : ChannelEvent, IEquatable<ProgramChangeEvent>
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

        /// <summary>
        /// Gets or sets program (patch) number.
        /// </summary>
        public SevenBitNumber ProgramNumber
        {
            get { return this[ProgramNumberParameterIndex]; }
            set { this[ProgramNumberParameterIndex] = value; }
        }

        #endregion

        #region IEquatable<ProgramChangeEvent>

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="programChangeEvent">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
        public bool Equals(ProgramChangeEvent programChangeEvent)
        {
            return Equals(programChangeEvent, true);
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
            return Equals(midiEvent as ProgramChangeEvent, respectDeltaTime);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Program Change [{Channel}] ({ProgramNumber})";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as ProgramChangeEvent);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return CalculateHashCode(EventStatusBytes.Channel.ProgramChange);
        }

        #endregion
    }
}
