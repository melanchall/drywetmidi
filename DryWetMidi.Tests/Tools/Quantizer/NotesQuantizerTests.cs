using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using System;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [Obsolete("OBS13")]
    [TestFixture]
    public sealed class NotesQuantizerTests : LengthedObjectsQuantizerTests<Note>
    {
        #region Nested classes

        private sealed class SkipNotesQuantizer : Quantizer
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

        private sealed class FixedTimeNotesQuantizer : Quantizer
        {
            #region Fields

            private readonly long _time;

            #endregion

            #region Constructor

            public FixedTimeNotesQuantizer(long time)
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

        public NotesQuantizerTests()
            : base(new NoteMethods(),
                   new SkipNotesQuantizer(),
                   time => new FixedTimeNotesQuantizer(time))
        {
        }

        #endregion
    }
}
