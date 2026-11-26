using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents a chunk's identity described by its type and corresponding ID.
    /// </summary>
    public sealed class ChunkType
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ChunkType"/> with the specified type and ID.
        /// </summary>
        /// <param name="type">Type of a chunk.</param>
        /// <param name="id">4-character ID of a chunk.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="id"/> is <c>null</c> or contains white-spaces only.</exception>
        public ChunkType(Type type, string id)
        {
            ThrowIfArgument.IsNull(nameof(type), type);
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(id), id, "Chunk ID");

            Type = type;
            Id = id;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the type of chunks described by this instance of the <see cref="ChunkType"/>.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets the ID of chunks described by this instance of the <see cref="ChunkType"/>.
        /// </summary>
        public string Id { get; }

        #endregion

        #region Operators

        /// <summary>
        /// Determines if two <see cref="ChunkType"/> objects are equal.
        /// </summary>
        /// <param name="chunkType1">The first <see cref="ChunkType"/> to compare.</param>
        /// <param name="chunkType2">The second <see cref="ChunkType"/> to compare.</param>
        /// <returns><c>true</c> if the objects are equal, <c>false</c> otherwise.</returns>
        public static bool operator ==(ChunkType? chunkType1, ChunkType? chunkType2)
        {
            if (ReferenceEquals(chunkType1, chunkType2))
                return true;

            if (ReferenceEquals(null, chunkType1) || ReferenceEquals(null, chunkType2))
                return false;

            return chunkType1.Type == chunkType2.Type &&
                   chunkType1.Id == chunkType2.Id;
        }

        /// <summary>
        /// Determines if two <see cref="ChunkType"/> objects are not equal.
        /// </summary>
        /// <param name="chunkType1">The first <see cref="ChunkType"/> to compare.</param>
        /// <param name="chunkType2">The second <see cref="ChunkType"/> to compare.</param>
        /// <returns><c>false</c> if the objects are equal, <c>true</c> otherwise.</returns>
        public static bool operator !=(ChunkType? chunkType1, ChunkType? chunkType2)
        {
            return !(chunkType1 == chunkType2);
        }

        #endregion

        #region Overrides

        /// <inheritdoc/>
        override public string ToString()
        {
            return $"{Id} -> {Type.Name}";
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return this == (obj as ChunkType);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                var result = 17;
                result = result * 23 + Type.GetHashCode();
                result = result * 23 + Id.GetHashCode();
                return result;
            }
        }

        #endregion
    }
}
