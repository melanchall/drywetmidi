using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using System;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [Obsolete("OBS13")]
    [TestFixture]
    public sealed class ChordsQuantizerTests : LengthedObjectsQuantizerTests<Chord>
    {
        #region Nested classes

        private sealed class SkipChordsQuantizer : Quantizer
        {
            #region Overrides

            protected override TimeProcessingInstruction OnObjectQuantizing(
                ITimedObject obj,
                QuantizedTime quantizedTime,
                IGrid grid,
                LengthedObjectTarget target,
                TempoMap tempoMap,
                QuantizingSettings settings)
            {
                return TimeProcessingInstruction.Skip;
            }

            #endregion
        }

        private sealed class FixedTimeChordsQuantizer : Quantizer
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
                ITimedObject obj,
                QuantizedTime quantizedTime,
                IGrid grid,
                LengthedObjectTarget target,
                TempoMap tempoMap,
                QuantizingSettings settings)
            {
                return new TimeProcessingInstruction(_time);
            }

            #endregion
        }

        #endregion

        #region Constructor

        public ChordsQuantizerTests()
            : base(new ChordMethods(),
                   new SkipChordsQuantizer(),
                   time => new FixedTimeChordsQuantizer(time))
        {
        }

        #endregion
    }
}
