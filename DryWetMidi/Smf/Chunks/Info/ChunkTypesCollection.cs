using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Collection of <see cref="ChunkType"/> objects which provide identity information of a chunk.
    /// </summary>
    public sealed class ChunkTypesCollection : IEnumerable<ChunkType>
    {
        #region Fields

        private readonly Dictionary<Type, string> _ids = new Dictionary<Type, string>();
        private readonly Dictionary<string, Type> _types = new Dictionary<string, Type>();

        #endregion

        #region Methods

        /// <summary>
        /// Adds chunk type along with the corresponding ID.
        /// </summary>
        /// <param name="type">Type of chunk.</param>
        /// <param name="id">ID of chunk.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is null. -or- <paramref name="id"/>
        /// is null.</exception>
        /// <exception cref="ArgumentException">Chunk type specified by <paramref name="type"/> and
        /// <paramref name="id"/> already exists in the <see cref="ChunksCollection"/>.</exception>
        public void Add(Type type, string id)
        {
            _ids.Add(type, id);
            _types.Add(id, type);
        }

        /// <summary>
        /// Gets the chunk type associated with the specified ID.
        /// </summary>
        /// <param name="id">ID of the chunk type to get.</param>
        /// <param name="type">When this method returns, contains the chunk type associated with
        /// the specified ID, if ID is found; otherwise, null. This parameter is passed
        /// uninitialized.</param>
        /// <returns>true if the <see cref="ChunkTypesCollection"/> contains a chunk type with the
        /// specified ID; otherwise, false.</returns>
        public bool TryGetType(string id, out Type type)
        {
            return _types.TryGetValue(id, out type);
        }

        /// <summary>
        /// Gets the ID associated with the specified chunk type.
        /// </summary>
        /// <param name="type">Chunk type to get ID for.</param>
        /// <param name="id">When this method returns, contains the ID associated with the specified
        /// chunk type, if the type is found; otherwise, null. This parameter is passed uninitialized.</param>
        /// <returns>true if the <see cref="ChunkTypesCollection"/> contains an ID for the
        /// specified chunk type; otherwise, false.</returns>
        public bool TryGetId(Type type, out string id)
        {
            return _ids.TryGetValue(type, out id);
        }

        #endregion

        #region IEnumerable<ChunkType>

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<ChunkType> GetEnumerator()
        {
            return _ids.Select(kv => new ChunkType(kv.Key, kv.Value))
                               .GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
