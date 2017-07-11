using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class TupletDefinition
    {
        #region Constants

        public static readonly TupletDefinition Triplet = new TupletDefinition(3, 2);
        public static readonly TupletDefinition Duplet = new TupletDefinition(2, 3);

        #endregion

        #region Constructor

        public TupletDefinition(int notesCount, int spaceSize)
        {
            if (notesCount < 1)
                throw new ArgumentOutOfRangeException(nameof(notesCount), notesCount, "Notes count is less than 1.");

            if (spaceSize < 1)
                throw new ArgumentOutOfRangeException(nameof(spaceSize), spaceSize, "Space size is less than 1.");

            NotesCount = notesCount;
            SpaceSize = spaceSize;
        }

        #endregion

        #region Properties

        public int NotesCount { get; }

        public int SpaceSize { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="tupletDefinition">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(TupletDefinition tupletDefinition)
        {
            if (ReferenceEquals(null, tupletDefinition))
                return false;

            if (ReferenceEquals(this, tupletDefinition))
                return true;

            return NotesCount == tupletDefinition.NotesCount &&
                   SpaceSize == tupletDefinition.SpaceSize;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{NotesCount}:{SpaceSize}";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as TupletDefinition);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return NotesCount.GetHashCode() ^ SpaceSize.GetHashCode();
        }

        #endregion
    }
}
