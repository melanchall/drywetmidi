using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Provides methods to quantize lengthed objects time.
    /// </summary>
    /// <typeparam name="TObject">The type of objects to quantize.</typeparam>
    /// <typeparam name="TSettings">The type of quantizer's settings.</typeparam>
    public abstract class LengthedObjectsQuantizer<TObject, TSettings> : Quantizer<TObject, TSettings>
        where TObject : ILengthedObject
        where TSettings : LengthedObjectsQuantizingSettings<TObject>, new()
    {
        #region Methods

        /// <summary>
        /// Quantizes objects time using the specified grid and settings.
        /// </summary>
        /// <param name="objects">Objects to quantize.</param>
        /// <param name="grid">Grid to quantize objects by.</param>
        /// <param name="tempoMap">Tempo map used to calculate times to quantize by.</param>
        /// <param name="settings">Settings according to which objects should be quantized.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="objects"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="grid"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public void Quantize(IEnumerable<TObject> objects, IGrid grid, TempoMap tempoMap, TSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            QuantizeInternal(objects, grid, tempoMap, settings);
        }

        private static TimeProcessingInstruction CorrectObjectOnStartQuantizing(TObject obj, long time, TempoMap tempoMap, TSettings settings)
        {
            if (settings.FixOppositeEnd)
            {
                var endTime = obj.Time + obj.Length;

                if (time > endTime)
                {
                    var result = ProcessQuantizingBeyondFixedEnd(ref time,
                                                                 ref endTime,
                                                                 settings.QuantizingBeyondFixedEndPolicy,
                                                                 "Start time is going to be beyond the end one.");
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

            return new TimeProcessingInstruction(time);
        }

        private static TimeProcessingInstruction CorrectObjectOnEndQuantizing(TObject obj, long time, TempoMap tempoMap, TSettings settings)
        {
            if (settings.FixOppositeEnd)
            {
                var startTime = obj.Time;

                if (time < startTime)
                {
                    var result = ProcessQuantizingBeyondFixedEnd(ref time,
                                                                 ref startTime,
                                                                 settings.QuantizingBeyondFixedEndPolicy,
                                                                 "End time is going to be beyond the start one.");
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
                            return TimeProcessingInstruction.Skip;
                        case QuantizingBeyondZeroPolicy.Abort:
                            throw new InvalidOperationException("Object is going to be moved beyond zero.");
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

            return new TimeProcessingInstruction(time);
        }

        private static TimeProcessingInstruction ProcessQuantizingBeyondFixedEnd(
            ref long newTime,
            ref long oldTime,
            QuantizingBeyondFixedEndPolicy quantizingBeyondFixedEndPolicy,
            string errorMessage)
        {
            switch (quantizingBeyondFixedEndPolicy)
            {
                case QuantizingBeyondFixedEndPolicy.Skip:
                    return TimeProcessingInstruction.Skip;
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

        /// <summary>
        /// Gets the time of an object that should be quantized.
        /// </summary>
        /// <param name="obj">Object to get time of.</param>
        /// <param name="settings">Settings according to which the object's time should be gotten.</param>
        /// <returns>The time of <paramref name="obj"/> that should be quantized.</returns>
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

        /// <summary>
        /// Sets the new time of an object.
        /// </summary>
        /// <param name="obj">Object to set time for.</param>
        /// <param name="time">New time after quantizing.</param>
        /// <param name="settings">Settings according to which the object's time should be set.</param>
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

        /// <summary>
        /// Performs additional actions before the new time will be set to an object.
        /// </summary>
        /// <remarks>
        /// Inside this method the new time can be changed or quantizing of an object can be cancelled.
        /// </remarks>
        /// <param name="obj">Object to quantize.</param>
        /// <param name="quantizedTime">Holds information about new time for an object.</param>
        /// <param name="grid">Grid to quantize object by.</param>
        /// <param name="tempoMap">Tempo map used to quantize object.</param>
        /// <param name="settings">Settings according to which object should be quantized.</param>
        /// <returns>An object indicating whether the new time should be set to the object
        /// or not. Also returned object contains that new time.</returns>
        protected override TimeProcessingInstruction OnObjectQuantizing(
            TObject obj,
            QuantizedTime quantizedTime,
            IGrid grid,
            TempoMap tempoMap,
            TSettings settings)
        {
            var newTime = quantizedTime.NewTime;

            switch (settings.QuantizingTarget)
            {
                case LengthedObjectTarget.Start:
                    return CorrectObjectOnStartQuantizing(obj, newTime, tempoMap, settings);
                case LengthedObjectTarget.End:
                    return CorrectObjectOnEndQuantizing(obj, newTime, tempoMap, settings);
            }

            return new TimeProcessingInstruction(newTime);
        }

        #endregion
    }
}
