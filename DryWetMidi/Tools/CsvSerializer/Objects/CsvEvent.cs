using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    internal sealed class CsvEvent : CsvObject
    {
        #region Constructor

        public CsvEvent(
            MidiEvent midiEvent,
            int? chunkIndex,
            string chunkId,
            int? objectIndex,
            ITimeSpan time)
            : base(chunkIndex, chunkId, objectIndex)
        {
            Event = midiEvent;
            Time = time;
        }

        #endregion

        #region Properties

        public MidiEvent Event { get; }

        public ITimeSpan Time { get; }

        #endregion
    }
}
