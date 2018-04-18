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
                                  .Select(o => GetOldTime(o, settings))
                                  .DefaultIfEmpty()
                                  .Max();
            var times = GetGridTimes(grid, lastTime, tempoMap).ToList();

            foreach (var obj in objects.Where(o => o != null))
            {
                var oldTime = GetOldTime(obj, settings);

                var startGridIndex = 0;

                while (startGridIndex < times.Count)
                {
                    var newTimeIndex = FindNearestTime(times, oldTime, startGridIndex);
                    var newTime = times[newTimeIndex];

                    var correctionResult = CorrectObject(obj, newTime, settings);
                    var instruction = correctionResult.QuantizingInstruction;

                    if (instruction == QuantizingInstruction.Apply)
                    {
                        SetNewTime(obj, correctionResult.Time, settings);
                        break;
                    }

                    if (instruction == QuantizingInstruction.Skip)
                        break;

                    if (instruction == QuantizingInstruction.UseNextGridPoint)
                        startGridIndex = newTimeIndex + 1;
                }
            }
        }

        protected abstract long GetOldTime(TObject obj, TSettings settings);

        protected abstract void SetNewTime(TObject obj, long time, TSettings settings);

        protected abstract QuantizingCorrectionResult CorrectObject(TObject obj, long time, TSettings settings);

        private static IEnumerable<long> GetGridTimes(IGrid grid, long lastTime, TempoMap tempoMap)
        {
            var times = grid.GetTimes(tempoMap);
            var enumerator = times.GetEnumerator();

            while (enumerator.MoveNext() && enumerator.Current < lastTime)
                yield return enumerator.Current;

            yield return enumerator.Current;
        }

        // TODO: specify LengthType to calculate deltas instead of MIDI only
        private static int FindNearestTime(IReadOnlyList<long> grid, long time, int startIndex)
        {
            var difference = long.MaxValue;
            var nearestTimeIndex = -1;

            for (int i = startIndex; i < grid.Count; i++)
            {
                var gridTime = grid[i];

                var timeDelta = Math.Abs(time - gridTime);
                if (timeDelta >= difference)
                    break;

                difference = timeDelta;
                nearestTimeIndex = i;
            }

            return nearestTimeIndex;
        }

        #endregion
    }
}
