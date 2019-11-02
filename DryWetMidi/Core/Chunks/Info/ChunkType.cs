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
        public ChunkType(Type type, string id)
        {
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
    }
}
