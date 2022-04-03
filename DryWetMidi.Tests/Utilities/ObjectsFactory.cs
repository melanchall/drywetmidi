using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tests
{
    internal sealed class ObjectsFactory
    {
        #region Fields

        private readonly TempoMap _tempoMap;

        #endregion

        #region Constructor

        public ObjectsFactory(TempoMap tempoMap)
        {
            _tempoMap = tempoMap;
        }

        #endregion

        #region Methods

        public Note GetNote(int noteNumber, ITimeSpan time, ITimeSpan length) =>
            new Note(
                (SevenBitNumber)noteNumber,
                LengthConverter.ConvertFrom(length, time, _tempoMap),
                TimeConverter.ConvertFrom(time, _tempoMap));

        public Note GetNote(int noteNumber, long time, ITimeSpan length) =>
            GetNote(noteNumber, (ITimeSpan)(MidiTimeSpan)time, length);

        #endregion
    }
}
