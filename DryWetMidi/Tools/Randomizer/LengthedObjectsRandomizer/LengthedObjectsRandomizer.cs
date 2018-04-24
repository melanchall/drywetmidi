using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public abstract class LengthedObjectsRandomizer<TObject, TSettings> : Randomizer<TObject, TSettings>
        where TObject : ILengthedObject
        where TSettings : LengthedObjectsRandomizingSettings, new()
    {
        #region Methods

        public void Randomize(IEnumerable<TObject> objects, IBounds bounds, TempoMap tempoMap, TSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(bounds), bounds);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            RandomizeInternal(objects, bounds, tempoMap, settings);
        }

        #endregion

        #region Overrides

        protected sealed override long GetOldTime(TObject obj, TSettings settings)
        {
            var target = settings.RandomizingTarget;
            switch (target)
            {
                case LengthedObjectTarget.Start:
                    return obj.Time;
                case LengthedObjectTarget.End:
                    return obj.Time + obj.Length;
                default:
                    throw new NotSupportedException($"{target} randomization target is not supported to get time.");
            }
        }

        protected sealed override void SetNewTime(TObject obj, long time, TSettings settings)
        {
            var target = settings.RandomizingTarget;
            switch (target)
            {
                case LengthedObjectTarget.Start:
                    obj.Time = time;
                    break;
                case LengthedObjectTarget.End:
                    obj.Time = time - obj.Length;
                    break;
                default:
                    throw new NotSupportedException($"{target} randomization target is not supported to set time.");
            }
        }

        protected sealed override RandomizingCorrectionResult CorrectObject(TObject obj, long time, TSettings settings)
        {
            var target = settings.RandomizingTarget;

            switch (target)
            {
                case LengthedObjectTarget.Start:
                    if (settings.FixOppositeEnd)
                        obj.Length = obj.Time + obj.Length - time;
                    break;
                case LengthedObjectTarget.End:
                    if (settings.FixOppositeEnd)
                        obj.Length = time - obj.Time;
                    break;
            }

            return new RandomizingCorrectionResult(RandomizingInstruction.Apply, time);
        }

        #endregion
    }
}
