using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public sealed class TimedEventsQuantizingSettings : QuantizingSettings
    {
    }

    public sealed class TimedEventsQuantizer : Quantizer<TimedEvent, TimedEventsQuantizingSettings>
    {
        #region Methods

        public void Quantize(IEnumerable<TimedEvent> objects, IGrid grid, TempoMap tempoMap, TimedEventsQuantizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            QuantizeInternal(objects, grid, tempoMap, settings);
        }

        #endregion

        #region Overrides

        protected override long GetOldTime(TimedEvent obj, TimedEventsQuantizingSettings settings)
        {
            return obj.Time;
        }

        protected override void SetNewTime(TimedEvent obj, long time, TimedEventsQuantizingSettings settings)
        {
            obj.Time = time;
        }

        protected override QuantizingCorrectionResult CorrectObject(TimedEvent obj, long time, TimedEventsQuantizingSettings settings)
        {
            return new QuantizingCorrectionResult(QuantizingInstruction.Apply, time);
        }

        #endregion
    }
}
