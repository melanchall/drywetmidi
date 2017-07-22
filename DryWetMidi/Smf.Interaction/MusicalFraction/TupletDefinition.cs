using Melanchall.DryWetMidi.Common;

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
            ThrowIfArgument.IsNonpositive(nameof(notesCount), notesCount, "Notes count is less than 1.");
            ThrowIfArgument.IsNonpositive(nameof(spaceSize), spaceSize, "Space size is less than 1.");

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
            return this == tupletDefinition;
        }

        #endregion

        #region Operators

        public static bool operator ==(TupletDefinition tupletDefinition1, TupletDefinition tupletDefinition2)
        {
            if (ReferenceEquals(tupletDefinition1, tupletDefinition2))
                return true;

            if (ReferenceEquals(null, tupletDefinition1) || ReferenceEquals(null, tupletDefinition2))
                return false;

            return tupletDefinition1.NotesCount == tupletDefinition2.NotesCount &&
                   tupletDefinition1.SpaceSize == tupletDefinition2.SpaceSize;
        }

        public static bool operator !=(TupletDefinition tupletDefinition1, TupletDefinition tupletDefinition2)
        {
            return !(tupletDefinition1 == tupletDefinition2);
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
