using System;
using System.Collections;
using System.Collections.Generic;

namespace Melanchall.DryMidi
{
    /// <summary>
    /// Collection of <see cref="Message"/> objects.
    /// </summary>
    public sealed class MessagesCollection : IEnumerable<Message>
    {
        #region Fields

        private readonly List<Message> _messages = new List<Message>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the message at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the message to get or set.</param>
        /// <returns>The message at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException"> <paramref name="index"/> is less than 0;
        /// or <paramref name="index"/> is equal to or greater than <see cref="Count"/>.</exception>
        /// <exception cref="ArgumentNullException">value is null</exception>
        public Message this[int index]
        {
            get
            {
                if (index < 0 || index >= _messages.Count)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "Index is out of range.");

                return _messages[index];
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                if (index < 0 || index >= _messages.Count)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "Index is out of range.");

                _messages[index] = value;
            }
        }

        /// <summary>
        /// Gets the number of messages contained in the collection.
        /// </summary>
        public int Count => _messages.Count;

        #endregion

        #region Methods

        /// <summary>
        /// Adds a message to the end of collection.
        /// </summary>
        /// <remarks>
        /// Note that End Of Track messages cannot be added into the collection since it may cause inconsistence in a
        /// track chunk structure. End Of Track message will be written to the track chunk automatically on
        /// <see cref="TrackChunk.WriteContent(MidiWriter, WritingSettings)"/>.
        /// </remarks>
        /// <param name="message">The message to be added to the end of the collection.</param>
        /// <exception cref="ArgumentNullException"><paramref name="message"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="message"/> is an instance of <see cref="EndOfTrackMessage"/>.</exception>
        public void Add(Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (message is EndOfTrackMessage)
                throw new ArgumentException("End Of Track cannot be added to messages collection.", nameof(message));

            _messages.Add(message);
        }

        /// <summary>
        /// Inserts a message into the collection at the specified index.
        /// </summary>
        /// <remarks>
        /// Note that End Of Track messages cannot be added into the collection since it may cause inconsistence in a
        /// track chunk structure. End Of Track message will be written to the track chunk automatically on
        /// <see cref="TrackChunk.WriteContent(MidiWriter, WritingSettings)"/>.
        /// </remarks>
        /// <param name="index">The zero-based index at which the message should be inserted.</param>
        /// <param name="message">The message to be added to the end of the collection.</param>
        /// <exception cref="ArgumentNullException"><paramref name="message"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="message"/> is an instance of <see cref="EndOfTrackMessage"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0; or
        /// <paramref name="index"/> is greater than <see cref="Count"/>.</exception>
        public void Insert(int index, Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (message is EndOfTrackMessage)
                throw new ArgumentException("End Of Track cannot be inserted to messages collection.", nameof(message));

            if (index < 0 || index >= _messages.Count)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Index is out of range.");

            _messages.Insert(index, message);
        }

        /// <summary>
        /// Removes the first occurrence of a specific message from the collection.
        /// </summary>
        /// <param name="message">The message to remove from the collection. The value cannot be null.</param>
        /// <returns>true if message is successfully removed; otherwise, false. This method also returns
        /// false if message was not found in the collection.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="message"/> is null.</exception>
        public bool Remove(Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            return _messages.Remove(message);
        }

        /// <summary>
        /// Removes the message at the specified index of the collection.
        /// </summary>
        /// <param name="index">The zero-based index of the message to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0; or <paramref name="index"/>
        /// is equal to or greater than <see cref="Count"/>.</exception>
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _messages.Count)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Index is out of range.");

            _messages.RemoveAt(index);
        }

        /// <summary>
        /// Removes all the messages that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="match">The <see cref="Predicate{T}"/> delegate that defines the conditions
        /// of the messages to remove.</param>
        /// <returns>The number of messages removed from the <see cref="MessagesCollection"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="match"/> is null.</exception>
        public int RemoveAll(Predicate<Message> match)
        {
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            return _messages.RemoveAll(match);
        }

        #endregion

        #region IEnumerable<Message>

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="MessagesCollection"/>.
        /// </summary>
        /// <returns>An enumerator for the <see cref="MessagesCollection"/>.</returns>
        public IEnumerator<Message> GetEnumerator()
        {
            return _messages.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="MessagesCollection"/>.
        /// </summary>
        /// <returns>An enumerator for the <see cref="MessagesCollection"/>.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _messages.GetEnumerator();
        }

        #endregion
    }
}
