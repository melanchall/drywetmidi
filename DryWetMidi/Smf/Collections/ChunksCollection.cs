using Melanchall.DryWetMidi.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Collection of <see cref="MidiChunk"/> objects.
    /// </summary>
    public sealed class ChunksCollection : IEnumerable<MidiChunk>
    {
        #region Fields

        private readonly List<MidiChunk> _chunks = new List<MidiChunk>();

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
        public MidiChunk this[int index]
        {
            get
            {
                if (index < 0 || index >= _chunks.Count)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "Index is out of range.");

                return _chunks[index];
            }
            set
            {
                ThrowIf.ArgumentIsNull(nameof(value), value);

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
        /// Adds a chunk to the end of the collection.
        /// </summary>
        /// <param name="chunk">The chunk to be added to the end of the collection.</param>
        /// <remarks>
        /// Note that header chunks cannot be added into the collection since it may cause inconsistence in the file structure.
        /// Header chunk with appropriate information will be written to a file automatically on
        /// <see cref="MidiFile.Write(string, bool, MidiFileFormat, WritingSettings)"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="chunk"/> is null.</exception>
        public void Add(MidiChunk chunk)
        {
            ThrowIf.ArgumentIsNull(nameof(chunk), chunk);

            _chunks.Add(chunk);
        }

        /// <summary>
        /// Adds chunks the end of the collection.
        /// </summary>
        /// <param name="chunks">Chunks to add to the collection.</param>
        /// <remarks>
        /// Note that header chunks cannot be added into the collection since it may cause inconsistence in the file structure.
        /// Header chunk with appropriate information will be written to a file automatically on
        /// <see cref="MidiFile.Write(string, bool, MidiFileFormat, WritingSettings)"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="chunks"/> is null.</exception>
        public void AddRange(IEnumerable<MidiChunk> chunks)
        {
            ThrowIf.ArgumentIsNull(nameof(chunks), chunks);

            _chunks.AddRange(chunks.Where(c => c != null));
        }

        /// <summary>
        /// Inserts a chunk into the collection at the specified index.
        /// </summary>
        /// <remarks>
        /// Note that header chunks cannot be inserted into the collection since it may cause inconsistence in the file structure.
        /// Header chunk with appropriate information will be written to a file automatically on
        /// <see cref="MidiFile.Write(string, bool, MidiFileFormat, WritingSettings)"/>.
        /// </remarks>
        /// <param name="index">The zero-based index at which the chunk should be inserted.</param>
        /// <param name="chunk">The chunk to insert.</param>
        /// <exception cref="ArgumentNullException"><paramref name="chunk"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0. -or-
        /// <paramref name="index"/> is greater than <see cref="Count"/>.</exception>
        public void Insert(int index, MidiChunk chunk)
        {
            ThrowIf.ArgumentIsNull(nameof(chunk), chunk);

            if (index < 0 || index >= _chunks.Count)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Index is out of range.");

            _chunks.Insert(index, chunk);
        }

        /// <summary>
        /// Inserts a set of chunks into the collection at the specified index.
        /// </summary>
        /// <remarks>
        /// Note that header chunks cannot be inserted into the collection since it may cause inconsistence in the file structure.
        /// Header chunk with appropriate information will be written to a file automatically on
        /// <see cref="MidiFile.Write(string, bool, MidiFileFormat, WritingSettings)"/>.
        /// </remarks>
        /// <param name="index">The zero-based index at which the chunk should be inserted.</param>
        /// <param name="chunks">The chunk to insert.</param>
        /// <exception cref="ArgumentNullException"><paramref name="chunks"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0. -or-
        /// <paramref name="index"/> is greater than <see cref="Count"/>.</exception>
        public void InsertRange(int index, IEnumerable<MidiChunk> chunks)
        {
            ThrowIf.ArgumentIsNull(nameof(chunks), chunks);

            if (index < 0 || index >= _chunks.Count)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Index is out of range.");

            _chunks.InsertRange(index, chunks.Where(c => c != null));
        }

        /// <summary>
        /// Removes the first occurrence of a specific chunk from the collection.
        /// </summary>
        /// <param name="chunk">The chunk to remove from the collection. The value cannot be null.</param>
        /// <returns>true if chunk is successfully removed; otherwise, false. This method also returns
        /// false if chunk was not found in the collection.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="chunk"/> is null.</exception>
        public bool Remove(MidiChunk chunk)
        {
            ThrowIf.ArgumentIsNull(nameof(chunk), chunk);

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

        /// <summary>
        /// Removes all the chunks that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="match">The <see cref="Predicate{T}"/> delegate that defines the conditions
        /// of the chunks to remove.</param>
        /// <returns>The number of chunks removed from the <see cref="ChunksCollection"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="match"/> is null.</exception>
        public int RemoveAll(Predicate<MidiChunk> match)
        {
            ThrowIf.ArgumentIsNull(nameof(match), match);

            return _chunks.RemoveAll(match);
        }

        /// <summary>
        /// Searches for the specified chunk and returns the zero-based index of the first
        /// occurrence within the entire <see cref="ChunksCollection"/>.
        /// </summary>
        /// <param name="chunk">The chunk to locate in the <see cref="ChunksCollection"/>.</param>
        /// <returns>The zero-based index of the first occurrence of chunk within the entire
        /// <see cref="ChunksCollection"/>, if found; otherwise, –1.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="chunk"/> is null.</exception>
        public int IndexOf(MidiChunk chunk)
        {
            ThrowIf.ArgumentIsNull(nameof(chunk), chunk);

            return _chunks.IndexOf(chunk);
        }

        /// <summary>
        /// Removes all chunks from the <see cref="ChunksCollection"/>.
        /// </summary>
        public void Clear()
        {
            _chunks.Clear();
        }

        #endregion

        #region IEnumerable<Chunk>

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="ChunksCollection"/>.
        /// </summary>
        /// <returns>An enumerator for the <see cref="ChunksCollection"/>.</returns>
        public IEnumerator<MidiChunk> GetEnumerator()
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
