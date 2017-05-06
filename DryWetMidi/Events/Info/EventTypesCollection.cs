using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi
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
        /// the specified status byte, if the key is found; otherwise, null. This parameter is passed
        /// uninitialized.</param>
        /// <returns>true if the <see cref="EventTypesCollection"/> contains an event type with the
        /// specified status byte; otherwise, false.</returns>
        public bool TryGetType(byte statusByte, out Type type)
        {
            return _types.TryGetValue(statusByte, out type);
        }

        public bool TryGetStatusByte(Type type, out byte statusByte)
        {
            return _statusBytes.TryGetValue(type, out statusByte);
        }

        #endregion

        #region IEnumerable<EventType>

        public IEnumerator<EventType> GetEnumerator()
        {
            return _statusBytes.Select(kv => new EventType(kv.Key, kv.Value))
                               .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
