using System;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a Note On message.
    /// </summary>
    /// <remarks>
    /// This message is sent when a note is depressed (start).
    /// </remarks>
    public sealed class NoteOnEvent : NoteEvent, IEquatable<NoteOnEvent>
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteOnEvent"/>.
        /// </summary>
        public NoteOnEvent()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteOnEvent"/> with the specified
        /// note number and velocity.
        /// </summary>
        /// <param name="noteNumber">Note number.</param>
        /// <param name="velocity">Velocity.</param>
        public NoteOnEvent(SevenBitNumber noteNumber, SevenBitNumber velocity)
            : base(noteNumber, velocity)
        {
        }

        #endregion

        #region IEquatable<NoteOnEvent>

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="noteOnEvent">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
        public bool Equals(NoteOnEvent noteOnEvent)
        {
            return Equals(noteOnEvent, true);
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
            return Equals(midiEvent as NoteOnEvent, respectDeltaTime);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Note On [{Channel}] ({NoteNumber}, {Velocity})";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as NoteOnEvent);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return CalculateHashCode(EventStatusBytes.Channel.NoteOn);
        }

        #endregion
    }
}
