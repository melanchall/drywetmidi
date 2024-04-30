namespace Melanchall.DryWetMidi.Tools
{
    internal abstract class CsvObject
    {
        #region Constrcutor

        protected CsvObject(int? chunkIndex, string chunkId, int? objectIndex)
        {
            ChunkIndex = chunkIndex;
            ChunkId = chunkId;
            ObjectIndex = objectIndex;
        }

        #endregion

        #region Properties

        public int? ChunkIndex { get; }

        public string ChunkId { get; }

        public int? ObjectIndex { get; }

        #endregion
    }
}
