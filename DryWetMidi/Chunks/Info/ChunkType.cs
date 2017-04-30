using System;

namespace Melanchall.DryWetMidi
{
    public sealed class ChunkType
    {
        #region Constructor

        public ChunkType(Type type, string id)
        {
            Type = type;
            Id = id;
        }

        #endregion

        #region Properties

        public Type Type { get; }

        public string Id { get; }

        #endregion
    }
}
