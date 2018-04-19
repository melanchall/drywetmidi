using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public abstract class Randomizer<TObject, TSettings>
        where TSettings : RandomizingSettings, new()
    {
        #region Methods

        // TODO: think about better names than tolerance
        protected void RandomizeInternal(IEnumerable<TObject> objects, IBounds bounds, TempoMap tempoMap, TSettings settings)
        {
            settings = settings ?? new TSettings();

            var random = new Random();

            foreach (var obj in objects.Where(o => o != null))
            {
                var time = GetOldTime(obj, settings);
                time = RandomizeTime(time, bounds, random, tempoMap);

                SetNewTime(obj, time, settings);
            }
        }

        protected abstract long GetOldTime(TObject obj, TSettings settings);

        protected abstract void SetNewTime(TObject obj, long time, TSettings settings);

        private static long RandomizeTime(long time, IBounds bounds, Random random, TempoMap tempoMap)
        {
            var timeBounds = bounds.GetBounds(time, tempoMap);

            var minTime = Math.Max(0, timeBounds.Item1) - 1;
            var maxTime = timeBounds.Item2;

            var difference = (int)Math.Abs(maxTime - minTime);
            return minTime + random.Next(difference) + 1;
        }

        private static long GetRandomTime(long minTime, long maxTime, Random random)
        {
            var difference = (int)Math.Abs(maxTime - minTime);
            return minTime + random.Next(difference);
        }

        #endregion
    }
}
