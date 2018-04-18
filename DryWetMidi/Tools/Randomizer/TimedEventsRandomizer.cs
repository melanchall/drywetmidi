using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public sealed class TimedEventsRandomizingSettings : RandomizingSettings
    {
    }

    public sealed class TimedEventsRandomizer : Randomizer<TimedEvent, TimedEventsRandomizingSettings>
    {
        #region Methods

        public void Randomize(IEnumerable<TimedEvent> objects, ITimeSpan tolerance, TempoMap tempoMap, TimedEventsRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(tolerance), tolerance);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            RandomizeInternal(objects, tolerance, tolerance, tempoMap, settings);
        }

        public void Randomize(IEnumerable<TimedEvent> objects, ITimeSpan leftTolerance, ITimeSpan rightTolerance, TempoMap tempoMap, TimedEventsRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(leftTolerance), leftTolerance);
            ThrowIfArgument.IsNull(nameof(rightTolerance), rightTolerance);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            RandomizeInternal(objects, leftTolerance, rightTolerance, tempoMap, settings);
        }

        #endregion

        #region Overrides

        protected override long GetOldTime(TimedEvent obj, TimedEventsRandomizingSettings settings)
        {
            return obj.Time;
        }

        protected override void SetNewTime(TimedEvent obj, long time, TimedEventsRandomizingSettings settings)
        {
            obj.Time = time;
        }

        #endregion
    }
}
