using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Core
{
    // TODO: test
    /// <summary>
    /// Comparer to compare <see cref="MidiEvent"/> objects for equality.
    /// </summary>
    public sealed class MidiEventEqualityComparer : IEqualityComparer<MidiEvent>
    {
        #region Fields

        private readonly MidiEventEqualityCheckSettings _settings;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MidiEventEqualityComparer"/>.
        /// </summary>
        public MidiEventEqualityComparer()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MidiEventEqualityComparer"/> with the
        /// specified settings according to which <see cref="MidiEvent"/> objects should
        /// be compared for equality.
        /// </summary>
        /// <param name="settings">Settings according to which <see cref="MidiEvent"/> objects should
        /// be compared for equality.</param>
        public MidiEventEqualityComparer(MidiEventEqualityCheckSettings settings)
        {
            _settings = settings ?? new MidiEventEqualityCheckSettings();
        }

        #endregion

        #region IEqualityComparer<MidiEvent>

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first event to compare.</param>
        /// <param name="y">The second event to compare.</param>
        /// <returns><c>true</c> if the specified objects are equal; otherwise, <c>false</c>.</returns>
        public bool Equals(MidiEvent x, MidiEvent y)
        {
            string message;
            return MidiEvent.Equals(x, y, _settings, out message);
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <param name="obj">The <see cref="MidiEvent"/> for which a hash code is to be returned.</param>
        /// <returns>A hash code for the specified object.</returns>
        public int GetHashCode(MidiEvent obj)
        {
            return obj.EventType.GetHashCode();
        }

        #endregion
    }
}
