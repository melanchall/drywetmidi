using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Represents a musical rest.
    /// </summary>
    public sealed class Rest : ILengthedObject
    {
        #region Constructor

        internal Rest(long time, long length, FourBitNumber? channel, SevenBitNumber? noteNumber)
        {
            Time = time;
            Length = length;
            Channel = channel;
            NoteNumber = noteNumber;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets start time of an object.
        /// </summary>
        public long Time { get; }

        /// <summary>
        /// Gets length of an object.
        /// </summary>
        public long Length { get; }

        /// <summary>
        /// Gets a channel the rest was constructed for.
        /// </summary>
        public FourBitNumber? Channel { get; }

        /// <summary>
        /// Gets a note number the rest was constructed for.
        /// </summary>
        public SevenBitNumber? NoteNumber { get; }

        #endregion

        #region Operators

        /// <summary>
        /// Determines if two <see cref="Rest"/> objects are equal.
        /// </summary>
        /// <param name="rest1">The first <see cref="Rest"/> to compare.</param>
        /// <param name="rest2">The second <see cref="Rest"/> to compare.</param>
        /// <returns>true if the rests are equal, false otherwise.</returns>
        public static bool operator ==(Rest rest1, Rest rest2)
        {
            if (ReferenceEquals(rest1, rest2))
                return true;

            if (ReferenceEquals(null, rest1) || ReferenceEquals(null, rest2))
                return false;

            return rest1.Time == rest2.Time &&
                   rest1.Length == rest2.Length &&
                   rest1.Channel == rest2.Channel &&
                   rest1.NoteNumber == rest2.NoteNumber;
        }

        /// <summary>
        /// Determines if two <see cref="Rest"/> objects are not equal.
        /// </summary>
        /// <param name="rest1">The first <see cref="Rest"/> to compare.</param>
        /// <param name="rest2">The second <see cref="Rest"/> to compare.</param>
        /// <returns>false if the rests are equal, true otherwise.</returns>
        public static bool operator !=(Rest rest1, Rest rest2)
        {
            return !(rest1 == rest2);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Rest (channel = {Channel}, note number = {NoteNumber})";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return this == (obj as Rest);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return Time.GetHashCode() ^
                   Length.GetHashCode() ^
                   Channel.GetValueOrDefault().GetHashCode() ^
                   NoteNumber.GetValueOrDefault().GetHashCode();
        }

        #endregion
    }
}
