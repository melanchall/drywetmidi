using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Core
{
    // TODO: test
    /// <summary>
    /// Comparer to compare <see cref="MidiFile"/> objects for equality.
    /// </summary>
    public sealed class MidiFileEqualityComparer : IEqualityComparer<MidiFile>
    {
        #region Fields

        private readonly MidiFileEqualityCheckSettings _settings;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MidiFileEqualityComparer"/>.
        /// </summary>
        public MidiFileEqualityComparer()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MidiFileEqualityComparer"/> with the
        /// specified settings according to which <see cref="MidiFile"/> objects should
        /// be compared for equality.
        /// </summary>
        /// <param name="settings">Settings according to which <see cref="MidiFile"/> objects should
        /// be compared for equality.</param>
        public MidiFileEqualityComparer(MidiFileEqualityCheckSettings settings)
        {
            _settings = settings ?? new MidiFileEqualityCheckSettings();
        }

        #endregion

        #region IEqualityComparer<MidiFile>

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first file to compare.</param>
        /// <param name="y">The second file to compare.</param>
        /// <returns><c>true</c> if the specified objects are equal; otherwise, <c>false</c>.</returns>
        public bool Equals(MidiFile x, MidiFile y)
        {
            string message;
            return MidiFile.Equals(x, y, _settings, out message);
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <param name="obj">The <see cref="MidiFile"/> for which a hash code is to be returned.</param>
        /// <returns>A hash code for the specified object.</returns>
        public int GetHashCode(MidiFile obj)
        {
            return obj.Chunks.Count.GetHashCode();
        }

        #endregion
    }
}
