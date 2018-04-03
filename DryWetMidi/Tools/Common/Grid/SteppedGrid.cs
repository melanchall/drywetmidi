using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public sealed class SteppedGrid : IGrid
    {
        #region Constructor

        public SteppedGrid(ITimeSpan step)
            : this(new[] { step })
        {
        }

        public SteppedGrid(ITimeSpan start, ITimeSpan step)
            : this(start, new[] { step })
        {
        }

        public SteppedGrid(IEnumerable<ITimeSpan> steps)
            : this((MidiTimeSpan)0, steps)
        {
        }

        public SteppedGrid(ITimeSpan start, IEnumerable<ITimeSpan> steps)
        {
            ThrowIfArgument.IsNull(nameof(start), start);
            ThrowIfArgument.IsNull(nameof(steps), steps);

            Start = start;
            Steps = steps;
        }

        #endregion

        #region Properties

        public ITimeSpan Start { get; }

        public IEnumerable<ITimeSpan> Steps { get; }

        #endregion

        #region IGrid

        public IEnumerable<long> GetTimes(TempoMap tempoMap)
        {
            if (!Steps.Any())
                yield break;

            var time = TimeConverter.ConvertFrom(Start, tempoMap);
            yield return time;

            while (true)
            {
                foreach (var step in Steps)
                {
                    time += LengthConverter.ConvertFrom(step, time, tempoMap);
                    yield return time;
                }
            }
        }

        #endregion
    }
}
