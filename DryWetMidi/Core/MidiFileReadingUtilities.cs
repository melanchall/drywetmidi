using System;

namespace Melanchall.DryWetMidi.Core
{
    internal static class MidiFileReadingUtilities
    {
        #region Constants

        private const string RiffChunkId = "RIFF";
        private const int RmidPreambleSize = 12; // RMID_size (4) + 'RMID' (4) + 'data' (4)

        #endregion

        #region Methods

        public static void ReadRmidPreamble(MidiReader reader, out long? smfEndPosition)
        {
            smfEndPosition = null;

            var chunkId = reader.ReadString(RiffChunkId.Length);
            if (chunkId == RiffChunkId)
            {
                reader.Position += RmidPreambleSize;
                var smfSize = reader.ReadDword();
                smfEndPosition = reader.Position + smfSize;
            }
            else
                reader.Position -= chunkId.Length;
        }

        public static MidiChunk TryCreateChunk(string chunkId, ChunkTypesCollection chunksTypes)
        {
            Type type = null;
            return chunksTypes?.TryGetType(chunkId, out type) == true && IsChunkType(type)
                ? (MidiChunk)Activator.CreateInstance(type)
                : null;
        }

        private static bool IsChunkType(Type type)
        {
            return type != null &&
                   type.IsSubclassOf(typeof(MidiChunk)) &&
                   type.GetConstructor(Type.EmptyTypes) != null;
        }

        #endregion
    }
}
