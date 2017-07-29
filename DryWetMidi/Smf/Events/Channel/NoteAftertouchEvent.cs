using System;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a Polyphonic Key Pressure (Aftertouch) message.
    /// </summary>
    /// <remarks>
    /// This message is most often sent by pressing down on the key after it "bottoms out".
    /// </remarks>
    public sealed class NoteAftertouchEvent : ChannelEvent, IEquatable<NoteAftertouchEvent>
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

        #region IEquatable<NoteAftertouchEvent>

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="noteAftertouchEvent">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
        public bool Equals(NoteAftertouchEvent noteAftertouchEvent)
        {
            return Equals(noteAftertouchEvent, true);
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
            return Equals(midiEvent as NoteAftertouchEvent, respectDeltaTime);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Note Aftertouch [{Channel}] ({NoteNumber}, {AftertouchValue})";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as NoteAftertouchEvent);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return CalculateHashCode(EventStatusBytes.Channel.NoteAftertouch);
        }

        #endregion
    }
}
