using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public sealed class EventsQuantizingSettings : QuantizingSettings
    {
    }

    public sealed class EventsQuantizer : Quantizer<TimedEvent, EventsQuantizingSettings>
    {
        #region Methods

        protected void Quantize(IEnumerable<TimedEvent> objects, IGrid grid, TempoMap tempoMap, EventsQuantizingSettings settings)
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(settings), settings);

            QuantizeInternal(objects, grid, tempoMap, settings);
        }

        #endregion

        #region Overrides

        protected override long GetOldTime(TimedEvent obj, EventsQuantizingSettings settings)
        {
            return obj.Time;
        }

        protected override void SetNewTime(TimedEvent obj, long time, EventsQuantizingSettings settings)
        {
            obj.Time = time;
        }

        protected override QuantizingCorrectionResult CorrectObject(TimedEvent obj, long time, EventsQuantizingSettings settings)
        {
            return new QuantizingCorrectionResult(QuantizingInstruction.Apply, time);
        }

        #endregion
    }
}
