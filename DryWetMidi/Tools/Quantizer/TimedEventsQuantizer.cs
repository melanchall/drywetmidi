using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public class TimedEventsQuantizingSettings : QuantizingSettings
    {
    }

    public class TimedEventsQuantizer : Quantizer<TimedEvent, TimedEventsQuantizingSettings>
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

        protected sealed override long GetObjectTime(TimedEvent obj, TimedEventsQuantizingSettings settings)
        {
            return obj.Time;
        }

        protected sealed override void SetObjectTime(TimedEvent obj, long time, TimedEventsQuantizingSettings settings)
        {
            obj.Time = time;
        }

        protected override QuantizingCorrectionResult CorrectObject(TimedEvent obj, long time, IGrid grid, IReadOnlyCollection<long> gridTimes, TempoMap tempoMap, TimedEventsQuantizingSettings settings)
        {
            return new QuantizingCorrectionResult(QuantizingInstruction.Apply, time);
        }

        #endregion
    }
}
