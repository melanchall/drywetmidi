using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf
{
    public sealed class ChunkTypesCollection : IEnumerable<ChunkType>
    {
        #region Fields

        private readonly Dictionary<Type, string> _ids = new Dictionary<Type, string>();
        private readonly Dictionary<string, Type> _types = new Dictionary<string, Type>();

        #endregion

        #region Methods

        public void Add(Type type, string id)
        {
            _ids.Add(type, id);
            _types.Add(id, type);
        }

        public bool TryGetType(string id, out Type type)
        {
            return _types.TryGetValue(id, out type);
        }

        public bool TryGetId(Type type, out string id)
        {
            return _ids.TryGetValue(type, out id);
        }

        #endregion

        #region IEnumerable<ChunkType>

        public IEnumerator<ChunkType> GetEnumerator()
        {
            return _ids.Select(kv => new ChunkType(kv.Key, kv.Value))
                               .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
