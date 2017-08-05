using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class StepBackAction : StepAction
    {
        #region Fields

        private static readonly Dictionary<Type, Func<ILength, ILength, ITime>> _lengthToTimeConverters =
            new Dictionary<Type, Func<ILength, ILength, ITime>>
            {
                [typeof(MetricLength)] = (length, step) =>
                {
                    var metricLength = (MetricLength)length;
                    var metricStep = (MetricLength)step;
                    return metricLength < metricStep
                        ? null
                        : new MetricTime(metricLength - metricStep);
                },
                [typeof(MusicalLength)] = (length, step) =>
                {
                    var musicalLength = (MusicalLength)length;
                    var musicalStep = (MusicalLength)step;
                    return musicalLength < musicalStep
                        ? null
                        : new MusicalTime(musicalLength - musicalStep);
                }
            };

        #endregion

        #region Constructor

        public StepBackAction(ILength step)
            : base(step)
        {
        }

        #endregion

        #region IPatternAction

        public override PatternActionResult Invoke(long time, PatternContext context)
        {
            var step = Step;
            var tempoMap = context.TempoMap;

            var stepType = step.GetType();
            var length = LengthConverter.ConvertTo(time, 0, stepType, tempoMap);

            context.SaveTime(time);

            var newTime = _lengthToTimeConverters[stepType](length, step);
            return new PatternActionResult(newTime == null
                ? 0
                : TimeConverter.ConvertFrom(newTime, tempoMap));
        }

        #endregion
    }
}
