using System;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a Note Off message.
    /// </summary>
    /// <remarks>
    /// This message is sent when a note is released (ended).
    /// </remarks>
    public sealed class NoteOffEvent : NoteEvent, IEquatable<NoteOffEvent>
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteOffEvent"/>.
        /// </summary>
        public NoteOffEvent()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteOffEvent"/> with the specified
        /// note number and velocity.
        /// </summary>
        /// <param name="noteNumber">Note number.</param>
        /// <param name="velocity">Velocity.</param>
        public NoteOffEvent(SevenBitNumber noteNumber, SevenBitNumber velocity)
            : base(noteNumber, velocity)
        {
        }

        #endregion

        #region IEquatable<NoteOffEvent>

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="noteOffEvent">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
        public bool Equals(NoteOffEvent noteOffEvent)
        {
            return Equals(noteOffEvent, true);
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
            return Equals(midiEvent as NoteOffEvent, respectDeltaTime);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Note Off [{Channel}] ({NoteNumber}, {Velocity})";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as NoteOffEvent);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return CalculateHashCode(EventStatusBytes.Channel.NoteOff);
        }

        #endregion
    }
}
