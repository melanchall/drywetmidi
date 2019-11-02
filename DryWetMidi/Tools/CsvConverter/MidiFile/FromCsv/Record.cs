using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    internal sealed class Record
    {
        #region Constructor

        public Record(int lineNumber, int? trackNumber, ITimeSpan time, string recordType, string[] parameters)
        {
            LineNumber = lineNumber;
            TrackNumber = trackNumber;
            Time = time;
            RecordType = recordType;
            Parameters = parameters;
        }

        #endregion

        #region Properties

        public int LineNumber { get; }

        public int? TrackNumber { get; }

        public ITimeSpan Time { get; }

        public string RecordType { get; }

        public string[] Parameters { get; }

        #endregion
    }
}
