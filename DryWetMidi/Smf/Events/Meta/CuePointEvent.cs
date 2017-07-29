using System;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a Cue Point meta event.
    /// </summary>
    /// <remarks>
    /// The MIDI cue point meta message denotes a cue in a MIDI file, usually to signify
    /// the beginning of an action. It can describe something that happens within a film,
    /// video or stage production at that point in the musical score. E.g. 'Car crashes',
    /// 'Door opens', etc.
    /// </remarks>
    public sealed class CuePointEvent : BaseTextEvent, IEquatable<CuePointEvent>
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CuePointEvent"/>.
        /// </summary>
        public CuePointEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CuePointEvent"/> with the
        /// specified text of cue.
        /// </summary>
        /// <param name="text">Text of the cue.</param>
        public CuePointEvent(string text)
            : base(text)
        {
        }

        #endregion

        #region IEquatable<CuePointEvent>

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="cuePointEvent">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
        public bool Equals(CuePointEvent cuePointEvent)
        {
            return Equals(cuePointEvent, true);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Clones event by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the event.</returns>
        protected override MidiEvent CloneEvent()
        {
            return new CuePointEvent(Text);
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
            return Equals(midiEvent as CuePointEvent, respectDeltaTime);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Cue Point ({Text})";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as CuePointEvent);
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
