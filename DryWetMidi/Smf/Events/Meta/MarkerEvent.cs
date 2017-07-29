using System;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a Marker meta event.
    /// </summary>
    /// <remarks>
    /// The MIDI marker meta message marks a point in time for a MIDI sequence.
    /// </remarks>
    public sealed class MarkerEvent : BaseTextEvent, IEquatable<MarkerEvent>
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkerEvent"/>.
        /// </summary>
        public MarkerEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkerEvent"/> with the
        /// specified text of marker.
        /// </summary>
        /// <param name="text">Text of the marker.</param>
        public MarkerEvent(string text)
            : base(text)
        {
        }

        #endregion

        #region IEquatable<MarkerEvent>

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="markerEvent">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
        public bool Equals(MarkerEvent markerEvent)
        {
            return Equals(markerEvent, true);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Clones event by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the event.</returns>
        protected override MidiEvent CloneEvent()
        {
            return new MarkerEvent(Text);
        }

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="midiEvent">The event to compare with the current one.</param>
        /// <param name="respectDeltaTime">If true the delta-times will be taken into an account
        /// while comparing events; if false - delta-times will be ignored.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public override bool Equals(MidiEvent midiEvent, bool respectDeltaTime)
        {
            return Equals(midiEvent as MarkerEvent, respectDeltaTime);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Marker ({Text})";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as MarkerEvent);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }
}
