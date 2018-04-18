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

        public void Randomize(IEnumerable<TObject> objects, ITimeSpan tolerance, TempoMap tempoMap, TSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(tolerance), tolerance);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            RandomizeInternal(objects, tolerance, tolerance, tempoMap, settings);
        }

        public void Randomize(IEnumerable<TObject> objects, ITimeSpan leftTolerance, ITimeSpan rightTolerance, TempoMap tempoMap, TSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(leftTolerance), leftTolerance);
            ThrowIfArgument.IsNull(nameof(rightTolerance), rightTolerance);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            RandomizeInternal(objects, leftTolerance, rightTolerance, tempoMap, settings);
        }

        // TODO: unify with quantizer
        // maybe add set for ILengthedObject.Time/Length
        protected abstract void SetObjectTime(TObject obj, long time);

        protected abstract void SetObjectLength(TObject obj, long length);

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
                    SetObjectTime(obj, time);
                    break;
                case LengthedObjectTarget.End:
                    SetObjectTime(obj, time - obj.Length);
                    break;
                default:
                    throw new NotSupportedException($"{target} randomization target is not supported to set time.");
            }
        }

        #endregion
    }
}
