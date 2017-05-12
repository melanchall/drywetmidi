using System;

namespace Melanchall.DryWetMidi
{
    /// <summary>
    /// Represents a MIDI file system exclusive event.
    /// </summary>
    /// <remarks>
    /// System exclusive events are used to specify a MIDI system exclusive message, either as one unit or in packets,
    /// or as an "escape" to specify any arbitrary bytes to be transmitted.
    /// </remarks>
    public abstract class SysExEvent : MidiEvent
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether this sytem exclusive event is completed or not.
        /// </summary>
        public bool Completed { get; internal set; } = true;

        /// <summary>
        /// Gets or sets the event's data.
        /// </summary>
        public byte[] Data { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="sysExEvent">The event to compare with the current one.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(SysExEvent sysExEvent)
        {
            return Equals(sysExEvent, true);
        }

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="sysExEvent">The event to compare with the current one.</param>
        /// <param name="respectDeltaTime">If true the <see cref="MidiEvent.DeltaTime"/> will be taken into an account
        /// while comparing events; if false - delta-times will be ignored.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(SysExEvent sysExEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, sysExEvent))
                return false;

            if (ReferenceEquals(this, sysExEvent))
                return true;

            return base.Equals(sysExEvent, respectDeltaTime) &&
                   Completed == sysExEvent.Completed &&
                   ArrayUtilities.Equals(Data, sysExEvent.Data);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Writes content of a MIDI event.
        /// </summary>
        /// <param name="writer">Writer to write the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        internal sealed override void Write(MidiWriter writer, WritingSettings settings)
        {
            var data = Data;
            if (data != null)
                writer.WriteBytes(data);
        }

        /// <summary>
        /// Gets the size of the content of a MIDI event.
        /// </summary>
        /// <returns>Size of the event's content.</returns>
        internal sealed override int GetSize()
        {
            return Data?.Length ?? 0;
        }

        /// <summary>
        /// Clones event by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the event.</returns>
        protected sealed override MidiEvent CloneEvent()
        {
            var eventType = GetType();
            var sysExEvent = (SysExEvent)Activator.CreateInstance(eventType);

            sysExEvent.Completed = Completed;
            sysExEvent.Data = Data?.Clone() as byte[];

            return sysExEvent;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as SysExEvent);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode() ^ Completed.GetHashCode() ^ (Data?.GetHashCode() ?? 0);
        }

        #endregion
    }
}
