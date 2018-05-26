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

        #endregion

        #region Overrides

        protected sealed override long GetOldTime(TObject obj, TSettings settings)
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

        protected sealed override void SetNewTime(TObject obj, long time, TSettings settings)
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

        protected sealed override QuantizingCorrectionResult CorrectObject(TObject obj, long time, TSettings settings)
        {
            var target = settings.QuantizingTarget;

            switch (target)
            {
                case LengthedObjectTarget.Start:
                    if (settings.FixOppositeEnd)
                    {
                        var endTime = obj.Time + obj.Length;

                        if (time > endTime)
                        {
                            switch (settings.QuantizingBeyondFixedEndPolicy)
                            {
                                case QuantizingBeyondFixedEndPolicy.Skip:
                                    return QuantizingCorrectionResult.Skip;
                                case QuantizingBeyondFixedEndPolicy.Abort:
                                    throw new InvalidOperationException("Quantized start time is going beyong end one.");
                                case QuantizingBeyondFixedEndPolicy.CollapseAndFix:
                                    time = endTime;
                                    break;
                                case QuantizingBeyondFixedEndPolicy.CollapseAndMove:
                                    endTime = time;
                                    break;
                                case QuantizingBeyondFixedEndPolicy.SwapEnds:
                                    var tmp = time;
                                    time = endTime;
                                    endTime = tmp;
                                    break;
                            }
                        }

                        obj.Length = endTime - time;
                    }
                    break;
                case LengthedObjectTarget.End:
                    if (settings.FixOppositeEnd)
                    {
                        var startTime = obj.Time;

                        if (time < startTime)
                        {
                            switch (settings.QuantizingBeyondFixedEndPolicy)
                            {
                                case QuantizingBeyondFixedEndPolicy.Skip:
                                    return QuantizingCorrectionResult.Skip;
                                case QuantizingBeyondFixedEndPolicy.Abort:
                                    throw new InvalidOperationException("Quantized end time is going beyond start one.");
                                case QuantizingBeyondFixedEndPolicy.CollapseAndFix:
                                    time = startTime;
                                    break;
                                case QuantizingBeyondFixedEndPolicy.CollapseAndMove:
                                    startTime = time;
                                    break;
                                case QuantizingBeyondFixedEndPolicy.SwapEnds:
                                    var tmp = time;
                                    time = startTime;
                                    startTime = tmp;
                                    break;
                            }
                        }

                        obj.Length = time - startTime;
                    }
                    else
                    {
                        var newStartTime = time - obj.Length;
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
                    }
                    break;
            }

            return new QuantizingCorrectionResult(QuantizingInstruction.Apply, time);
        }

        #endregion
    }
}
