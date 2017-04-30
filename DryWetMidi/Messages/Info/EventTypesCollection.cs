using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi
{
    public sealed class EventTypesCollection : IEnumerable<EventType>
    {
        #region Fields

        private readonly Dictionary<Type, byte> _statusBytes = new Dictionary<Type, byte>();
        private readonly Dictionary<byte, Type> _types = new Dictionary<byte, Type>();

        #endregion

        #region Methods

        public void Add(Type type, byte statusByte)
        {
            _statusBytes.Add(type, statusByte);
            _types.Add(statusByte, type);
        }

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
