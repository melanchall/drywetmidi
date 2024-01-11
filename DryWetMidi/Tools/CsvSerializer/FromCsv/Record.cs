namespace Melanchall.DryWetMidi.Tools
{
    internal sealed class Record
    {
        #region Constants

        public const string HeaderType = "Header";
        public const string EventType = "Event";
        public const string NoteType = "Note";

        #endregion

        #region Constructor

        public Record(CsvRecord csvRecord, int? chunkIndex, string chunkId, int? objectIndex, string recordType, string[] parameters)
        {
            CsvRecord = csvRecord;
            ChunkIndex = chunkIndex;
            ChunkId = chunkId;
            ObjectIndex = objectIndex;
            RecordType = recordType;
            Parameters = parameters;
        }

        #endregion

        #region Properties

        public CsvRecord CsvRecord { get; }

        public int? ChunkIndex { get; }

        public string ChunkId { get; }

        public int? ObjectIndex { get; }

        public string RecordType { get; }

        public string[] Parameters { get; }

        #endregion
    }
}
