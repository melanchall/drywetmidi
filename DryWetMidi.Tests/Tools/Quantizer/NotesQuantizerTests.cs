using System.Collections.Generic;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed class NotesQuantizerTests : LengthedObjectsQuantizerTests<Note, NotesQuantizingSettings>
    {
        #region Nested classes

        private sealed class SkipNotesQuantizer : NotesQuantizer
        {
            #region Overrides

            protected override QuantizingCorrectionResult CorrectObject(Note obj, long time, IGrid grid, IReadOnlyCollection<long> gridTimes, TempoMap tempoMap, NotesQuantizingSettings settings)
            {
                return QuantizingCorrectionResult.Skip;
            }

            #endregion
        }

        private sealed class FixedTimeNotesQuantizer : NotesQuantizer
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

            protected override QuantizingCorrectionResult CorrectObject(Note obj, long time, IGrid grid, IReadOnlyCollection<long> gridTimes, TempoMap tempoMap, NotesQuantizingSettings settings)
            {
                return new QuantizingCorrectionResult(QuantizingInstruction.Apply, _time);
            }

            #endregion
        }

        #endregion

        #region Constructor

        public NotesQuantizerTests()
            : base(new NoteMethods(),
                   new NotesQuantizer(),
                   new SkipNotesQuantizer(),
                   time => new FixedTimeNotesQuantizer(time))
        {
        }

        #endregion
    }
}
