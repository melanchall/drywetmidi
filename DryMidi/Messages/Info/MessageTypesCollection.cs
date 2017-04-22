using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryMidi
{
    public sealed class MessageTypesCollection : IEnumerable<MessageType>
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

        #region IEnumerable<MessageType>

        public IEnumerator<MessageType> GetEnumerator()
        {
            return _statusBytes.Select(kv => new MessageType(kv.Key, kv.Value))
                               .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
