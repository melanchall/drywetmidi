using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed class ChordsQuantizerTests : LengthedObjectsQuantizerTests<Chord, ChordsQuantizingSettings>
    {
        #region Nested classes

        private sealed class SkipChordsQuantizer : ChordsQuantizer
        {
            #region Overrides

            protected override TimeProcessingInstruction OnObjectQuantizing(
                Chord obj,
                QuantizedTime quantizedTime,
                IGrid grid,
                TempoMap tempoMap,
                ChordsQuantizingSettings settings)
            {
                return TimeProcessingInstruction.Skip;
            }

            #endregion
        }

        private sealed class FixedTimeChordsQuantizer : ChordsQuantizer
        {
            #region Fields

            private readonly long _time;

            #endregion

            #region Constructor

            public FixedTimeChordsQuantizer(long time)
            {
                _time = time;
            }

            #endregion

            #region Overrides

            protected override TimeProcessingInstruction OnObjectQuantizing(
                Chord obj,
                QuantizedTime quantizedTime,
                IGrid grid,
                TempoMap tempoMap,
                ChordsQuantizingSettings settings)
            {
                return new TimeProcessingInstruction(_time);
            }

            #endregion
        }

        #endregion

        #region Constructor

        public ChordsQuantizerTests()
            : base(new ChordMethods(),
                   new ChordsQuantizer(),
                   new SkipChordsQuantizer(),
                   time => new FixedTimeChordsQuantizer(time))
        {
        }

        #endregion
    }
}
