using System;

namespace Melanchall.DryWetMidi.Smf
{
    public sealed class EscapeSysExEvent : SysExEvent
    {
        #region Constructor

        public EscapeSysExEvent()
        {
        }

        public EscapeSysExEvent(byte[] data)
        {
            Data = data;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="escapeSysExEvent">The event to compare with the current one.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(EscapeSysExEvent escapeSysExEvent)
        {
            return Equals(escapeSysExEvent, true);
        }

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="escapeSysExEvent">The event to compare with the current one.</param>
        /// <param name="respectDeltaTime">If true the <see cref="MidiEvent.DeltaTime"/> will be taken into an account
        /// while comparing events; if false - delta-times will be ignored.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(EscapeSysExEvent escapeSysExEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, escapeSysExEvent))
                return false;

            if (ReferenceEquals(this, escapeSysExEvent))
                return true;

            return base.Equals(escapeSysExEvent, respectDeltaTime);
        }

        #endregion

        #region Overrides

        internal override void Read(MidiReader reader, ReadingSettings settings, int size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Non-negative size have to be specified in order to read Escape SysEx event.");

            Data = reader.ReadBytes(size);
            Completed = true;
        }

        public override string ToString()
        {
            return "Escape SysEx";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as EscapeSysExEvent);
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
