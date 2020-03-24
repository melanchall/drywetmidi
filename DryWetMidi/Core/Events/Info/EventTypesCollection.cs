using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Collection of <see cref="EventType"/> objects which provide identity information of an event.
    /// </summary>
    public sealed class EventTypesCollection : IEnumerable<EventType>
    {
        #region Fields

        private readonly Dictionary<Type, byte> _statusBytes = new Dictionary<Type, byte>();
        private readonly Dictionary<byte, Type> _types = new Dictionary<byte, Type>();

        #endregion

        #region Methods

        /// <summary>
        /// Adds event type along with the corresponding status byte.
        /// </summary>
        /// <param name="type">Type of event.</param>
        /// <param name="statusByte">Status byte of event.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Event type specified by <paramref name="type"/> and
        /// <paramref name="statusByte"/> already exists in the <see cref="EventsCollection"/>.</exception>
        public void Add(Type type, byte statusByte)
        {
            _statusBytes.Add(type, statusByte);
            _types.Add(statusByte, type);
        }

        /// <summary>
        /// Gets the event type associated with the specified status byte.
        /// </summary>
        /// <param name="statusByte">The status byte of the event type to get.</param>
        /// <param name="type">When this method returns, contains the event type associated with
        /// the specified status byte, if the status byte is found; otherwise, <c>null</c>. This parameter
        /// is passed uninitialized.</param>
        /// <returns><c>true</c> if the <see cref="EventTypesCollection"/> contains an event type with the
        /// specified status byte; otherwise, <c>false</c>.</returns>
        public bool TryGetType(byte statusByte, out Type type)
        {
            return _types.TryGetValue(statusByte, out type);
        }

        /// <summary>
        /// Gets the status byte associated with the specified event type.
        /// </summary>
        /// <param name="type">Event type to get status byte for.</param>
        /// <param name="statusByte">When this method returns, contains the status byte associated with
        /// the specified event type, if the type is found; otherwise, 0. This parameter is passed
        /// uninitialized.</param>
        /// <returns><c>true</c> if the <see cref="EventTypesCollection"/> contains a status byte for the
        /// specified event type; otherwise, <c>false</c>.</returns>
        public bool TryGetStatusByte(Type type, out byte statusByte)
        {
            return _statusBytes.TryGetValue(type, out statusByte);
        }

        #endregion

        #region IEnumerable<EventType>

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<EventType> GetEnumerator()
        {
            return _statusBytes.Select(kv => new EventType(kv.Key, kv.Value))
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
