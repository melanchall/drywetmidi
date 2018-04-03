using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public sealed class ArbitraryGrid : IGrid
    {
        #region Constructor

        public ArbitraryGrid(IEnumerable<ITimeSpan> times)
        {
            ThrowIfArgument.IsNull(nameof(times), times);

            Times = times;
        }

        #endregion

        #region Properties

        public IEnumerable<ITimeSpan> Times { get; }

        #endregion

        #region IGrid

        public IEnumerable<long> GetTimes(TempoMap tempoMap)
        {
            return Times.Select(t => TimeConverter.ConvertFrom(t, tempoMap));
        }

        #endregion
    }
}
