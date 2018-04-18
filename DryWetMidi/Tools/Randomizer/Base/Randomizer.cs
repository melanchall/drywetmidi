using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public abstract class Randomizer<TObject, TSettings>
        where TSettings : RandomizingSettings, new()
    {
        #region Methods

        // TODO: think about better names than tolerance
        protected void RandomizeInternal(IEnumerable<TObject> objects, ITimeSpan leftTolerance, ITimeSpan rightTolerance, TempoMap tempoMap, TSettings settings)
        {
            settings = settings ?? new TSettings();

            var random = new Random();

            foreach (var obj in objects)
            {
                if (obj == null)
                    continue;

                var time = GetOldTime(obj, settings);
                time = RandomizeTime(time, leftTolerance, rightTolerance, random, tempoMap);

                SetNewTime(obj, time, settings);
            }
        }

        protected abstract long GetOldTime(TObject obj, TSettings settings);

        protected abstract void SetNewTime(TObject obj, long time, TSettings settings);

        private static long RandomizeTime(long time, ITimeSpan leftTolerance, ITimeSpan rightTolerance, Random random, TempoMap tempoMap)
        {
            var minTime = CalculateBoundaryTime(time, leftTolerance, MathOperation.Subtract, tempoMap);
            var maxTime = CalculateBoundaryTime(time, rightTolerance, MathOperation.Add, tempoMap);

            // Max time is always nonnegative since grid cannot start below zero
            // so result of randomizing is guaranteed to be nonnegative

            return minTime < 0 && maxTime >= 0
                ? GetRandomTime(-1, maxTime, random) + 1
                : GetRandomTime(minTime - 1, maxTime, random) + 1;
        }

        private static long CalculateBoundaryTime(long time, ITimeSpan tolerance, MathOperation operation, TempoMap tempoMap)
        {
            ITimeSpan boundaryTime = (MidiTimeSpan)time;

            switch (operation)
            {
                case MathOperation.Add:
                    boundaryTime = boundaryTime.Add(tolerance, TimeSpanMode.TimeLength);
                    break;

                case MathOperation.Subtract:
                    boundaryTime = boundaryTime.Subtract(tolerance, TimeSpanMode.TimeLength);
                    break;
            }

            return TimeConverter.ConvertFrom(boundaryTime, tempoMap);
        }

        private static long GetRandomTime(long minTime, long maxTime, Random random)
        {
            var difference = (int)Math.Abs(maxTime - minTime);
            return minTime + random.Next(difference);
        }

        #endregion
    }
}
