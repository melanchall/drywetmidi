using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public abstract class Quantizer<TObject, TSettings>
        where TSettings : QuantizingSettings, new()
    {
        #region Methods

        protected void QuantizeInternal(IEnumerable<TObject> objects, IGrid grid, TempoMap tempoMap, TSettings settings)
        {
            settings = settings ?? new TSettings();

            var lastTime = objects.Where(o => o != null)
                                  .Select(o => GetObjectTime(o, settings))
                                  .DefaultIfEmpty()
                                  .Max();
            var times = GetGridTimes(grid, lastTime, tempoMap).ToList();

            foreach (var obj in objects.Where(o => o != null))
            {
                var oldTime = GetObjectTime(obj, settings);

                var newTimeIndex = FindNearestTime(times, oldTime, settings.DistanceType, tempoMap);
                var newTime = times[newTimeIndex];

                var correctionResult = CorrectObject(obj, newTime, grid, times, tempoMap, settings);
                var instruction = correctionResult.QuantizingInstruction;

                switch (instruction)
                {
                    case QuantizingInstruction.Apply:
                        SetObjectTime(obj, correctionResult.Time, settings);
                        break;
                    case QuantizingInstruction.Skip:
                        break;
                }
            }
        }

        protected abstract long GetObjectTime(TObject obj, TSettings settings);

        protected abstract void SetObjectTime(TObject obj, long time, TSettings settings);

        protected abstract QuantizingCorrectionResult CorrectObject(TObject obj, long time, IGrid grid, IReadOnlyCollection<long> gridTimes, TempoMap tempoMap, TSettings settings);

        private static IEnumerable<long> GetGridTimes(IGrid grid, long lastTime, TempoMap tempoMap)
        {
            var times = grid.GetTimes(tempoMap);
            var enumerator = times.GetEnumerator();

            while (enumerator.MoveNext() && enumerator.Current < lastTime)
                yield return enumerator.Current;

            yield return enumerator.Current;
        }

        private static int FindNearestTime(IReadOnlyList<long> grid, long time, TimeSpanType distanceType, TempoMap tempoMap)
        {
            var difference = TimeSpanUtilities.GetMaxTimeSpan(distanceType);
            var nearestTimeIndex = -1;

            for (int i = 0; i < grid.Count; i++)
            {
                var gridTime = grid[i];

                var timeDelta = Math.Abs(time - gridTime);
                var convertedTimeDelta = LengthConverter.ConvertTo(timeDelta, distanceType, Math.Min(time, gridTime), tempoMap);
                if (convertedTimeDelta.CompareTo(difference) >= 0)
                    break;

                difference = convertedTimeDelta;
                nearestTimeIndex = i;
            }

            return nearestTimeIndex;
        }

        #endregion
    }
}
