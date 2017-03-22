using System;
using System.Collections;
using System.Collections.Generic;

namespace Melanchall.DryMidi
{
    /// <summary>
    /// Collection of <see cref="Chunk"/> objects.
    /// </summary>
    public sealed class ChunksCollection : IEnumerable<Chunk>
    {
        #region Fields

        private readonly List<Chunk> _chunks = new List<Chunk>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the chunk at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the chunk to get or set.</param>
        /// <returns>The chunk at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than 0; or <paramref name="index"/> is equal to or greater than
        /// <see cref="Count"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">value is null</exception>
        public Chunk this[int index]
        {
            get
            {
                if (index < 0 || index >= _chunks.Count)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "Index is out of range.");

                return _chunks[index];
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                if (index < 0 || index >= _chunks.Count)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "Index is out of range.");

                _chunks[index] = value;
            }
        }

        /// <summary>
        /// Gets the number of chunks contained in the collection.
        /// </summary>
        public int Count => _chunks.Count;

        #endregion

        #region Methods

        /// <summary>
        /// Adds a chunk to the end of collection.
        /// </summary>
        /// <remarks>
        /// Note that header chunks cannot be added into the collection since it may cause inconsistence in the file structure.
        /// Header chunk with appropriate information will be written to a file automatically on <see cref="MidiFile.Save(string, bool, WritingSettings)"/>
        /// or <see cref="MidiFile.Write(System.IO.Stream, WritingSettings)"/>.
        /// </remarks>
        /// <param name="chunk">The chunk to be added to the end of the collection.</param>
        /// <exception cref="ArgumentNullException"><paramref name="chunk"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="chunk"/> is an instance of <see cref="HeaderChunk"/>.</exception>
        public void Add(Chunk chunk)
        {
            if (chunk == null)
                throw new ArgumentNullException(nameof(chunk));

            if (chunk is HeaderChunk)
                throw new ArgumentException("Header chunk cannot be added to chunks collection.", nameof(chunk));

            _chunks.Add(chunk);
        }

        /// <summary>
        /// Inserts a chunk into the collection at the specified index.
        /// </summary>
        /// <remarks>
        /// Note that header chunks cannot be inserted into the collection since it may cause inconsistence in the file structure.
        /// Header chunk with appropriate information will be written to a file automatically on <see cref="MidiFile.Save(string, bool, WritingSettings)"/>
        /// or <see cref="MidiFile.Write(System.IO.Stream, WritingSettings)"/>.
        /// </remarks>
        /// <param name="index">The zero-based index at which the chunk should be inserted.</param>
        /// <param name="chunk">The chunk to be added to the end of the collection.</param>
        /// <exception cref="ArgumentNullException"><paramref name="chunk"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="chunk"/> is an instance of <see cref="HeaderChunk"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0; or
        /// <paramref name="index"/> is greater than <see cref="Count"/>.</exception>
        public void Insert(int index, Chunk chunk)
        {
            if (chunk == null)
                throw new ArgumentNullException(nameof(chunk));

            if (chunk is HeaderChunk)
                throw new ArgumentException("Header chunk cannot be inserted to chunks collection.", nameof(chunk));

            if (index < 0 || index >= _chunks.Count)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Index is out of range.");

            _chunks.Insert(index, chunk);
        }


        /// <summary>
        /// Removes the first occurrence of a specific chunk from the collection.
        /// </summary>
        /// <param name="chunk">The chunk to remove from the collection. The value cannot be null.</param>
        /// <returns>true if chunk is successfully removed; otherwise, false. This method also returns
        /// false if chunk was not found in the collection.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="chunk"/> is null.</exception>
        public bool Remove(Chunk chunk)
        {
            if (chunk == null)
                throw new ArgumentNullException(nameof(chunk));

            return _chunks.Remove(chunk);
        }

        /// <summary>
        /// Removes the chunk at the specified index of the collection.
        /// </summary>
        /// <param name="index">The zero-based index of the chunk to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0; or <paramref name="index"/>
        /// is equal to or greater than <see cref="Count"/>.</exception>
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _chunks.Count)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Index is out of range.");

            _chunks.RemoveAt(index);
        }

        #endregion

        #region IEnumerable<Message>

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="ChunksCollection"/>.
        /// </summary>
        /// <returns>An enumerator for the <see cref="ChunksCollection"/>.</returns>
        public IEnumerator<Chunk> GetEnumerator()
        {
            return _chunks.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="ChunksCollection"/>.
        /// </summary>
        /// <returns>An enumerator for the <see cref="ChunksCollection"/>.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _chunks.GetEnumerator();
        }

        #endregion
    }
}
