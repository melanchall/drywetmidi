using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public abstract class LengthedObjectsQuantizer<TObject, TSettings> : Quantizer<TObject, TSettings>
        where TObject : ILengthedObject
        where TSettings : LengthedObjectsQuantizingSettings, new()
    {
        #region Methods

        public void Quantize(IEnumerable<TObject> objects, IGrid grid, TempoMap tempoMap, TSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            QuantizeInternal(objects, grid, tempoMap, settings);
        }

        private static QuantizingCorrectionResult CorrectObjectOnStartQuantizing(TObject obj, long time, TempoMap tempoMap, TSettings settings)
        {
            if (settings.FixOppositeEnd)
            {
                var endTime = obj.Time + obj.Length;

                if (time > endTime)
                {
                    var result = ProcessQuantizingBeyondFixedEnd(ref time,
                                                                 ref endTime,
                                                                 settings.QuantizingBeyondFixedEndPolicy,
                                                                 "Quantized start time is going beyond end one.");
                    if (result != null)
                        return result;
                }

                obj.Length = endTime - time;
            }
            else
            {
                var length = obj.LengthAs(settings.LengthType, tempoMap);
                obj.Length = LengthConverter.ConvertFrom(length, time, tempoMap);
            }

            return new QuantizingCorrectionResult(QuantizingInstruction.Apply, time);
        }

        private static QuantizingCorrectionResult CorrectObjectOnEndQuantizing(TObject obj, long time, TempoMap tempoMap, TSettings settings)
        {
            if (settings.FixOppositeEnd)
            {
                var startTime = obj.Time;

                if (time < startTime)
                {
                    var result = ProcessQuantizingBeyondFixedEnd(ref time,
                                                                 ref startTime,
                                                                 settings.QuantizingBeyondFixedEndPolicy,
                                                                 "Quantized end time is going beyond start one.");
                    if (result != null)
                        return result;
                }

                obj.Length = time - startTime;
            }
            else
            {
                var length = obj.LengthAs(settings.LengthType, tempoMap);

                var newStartTime = settings.LengthType == TimeSpanType.Midi
                    ? time - obj.Length
                    : TimeConverter.ConvertFrom(((MidiTimeSpan)time).Subtract(length, TimeSpanMode.TimeLength), tempoMap);
                if (newStartTime < 0)
                {
                    switch (settings.QuantizingBeyondZeroPolicy)
                    {
                        case QuantizingBeyondZeroPolicy.Skip:
                            return QuantizingCorrectionResult.Skip;
                        case QuantizingBeyondZeroPolicy.UseNextGridPoint:
                            return QuantizingCorrectionResult.UseNextGridPoint;
                        case QuantizingBeyondZeroPolicy.Abort:
                            throw new InvalidOperationException("Quantized object is going below zero.");
                        case QuantizingBeyondZeroPolicy.FixAtZero:
                            obj.Length = time;
                            break;
                    }
                }
                else
                {
                    obj.Length = LengthConverter.ConvertFrom(length, newStartTime, tempoMap);
                }
            }

            return new QuantizingCorrectionResult(QuantizingInstruction.Apply, time);
        }

        private static QuantizingCorrectionResult ProcessQuantizingBeyondFixedEnd(
            ref long newTime,
            ref long oldTime,
            QuantizingBeyondFixedEndPolicy quantizingBeyondFixedEndPolicy,
            string errorMessage)
        {
            switch (quantizingBeyondFixedEndPolicy)
            {
                case QuantizingBeyondFixedEndPolicy.Skip:
                    return QuantizingCorrectionResult.Skip;
                case QuantizingBeyondFixedEndPolicy.Abort:
                    throw new InvalidOperationException(errorMessage);
                case QuantizingBeyondFixedEndPolicy.CollapseAndFix:
                    newTime = oldTime;
                    break;
                case QuantizingBeyondFixedEndPolicy.CollapseAndMove:
                    oldTime = newTime;
                    break;
                case QuantizingBeyondFixedEndPolicy.SwapEnds:
                    var tmp = newTime;
                    newTime = oldTime;
                    oldTime = tmp;
                    break;
            }

            return null;
        }

        #endregion

        #region Overrides

        protected sealed override long GetObjectTime(TObject obj, TSettings settings)
        {
            var target = settings.QuantizingTarget;

            switch (target)
            {
                case LengthedObjectTarget.Start:
                    return obj.Time;
                case LengthedObjectTarget.End:
                    return obj.Time + obj.Length;
                default:
                    throw new NotSupportedException($"{target} quantization target is not supported to get time.");
            }
        }

        protected sealed override void SetObjectTime(TObject obj, long time, TSettings settings)
        {
            var target = settings.QuantizingTarget;

            switch (target)
            {
                case LengthedObjectTarget.Start:
                    obj.Time = time;
                    break;
                case LengthedObjectTarget.End:
                    obj.Time = time - obj.Length;
                    break;
                default:
                    throw new NotSupportedException($"{target} quantization target is not supported to set time.");
            }
        }

        protected sealed override QuantizingCorrectionResult CorrectObject(TObject obj, long time, TempoMap tempoMap, TSettings settings)
        {
            switch (settings.QuantizingTarget)
            {
                case LengthedObjectTarget.Start:
                    return CorrectObjectOnStartQuantizing(obj, time, tempoMap, settings);
                case LengthedObjectTarget.End:
                    return CorrectObjectOnEndQuantizing(obj, time, tempoMap, settings);
            }

            return new QuantizingCorrectionResult(QuantizingInstruction.Apply, time);
        }

        #endregion
    }
}
