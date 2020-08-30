using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Settings according to which timed events should be quantized.
    /// </summary>
    public class TimedEventsQuantizingSettings : QuantizingSettings<TimedEvent>
    {
    }

    /// <summary>
    /// Provides methods to quantize timed events time.
    /// </summary>
    /// <remarks>
    /// See <see href="xref:a_quantizer">Quantizer</see> article to learn more.
    /// </remarks>
    public class TimedEventsQuantizer : Quantizer<TimedEvent, TimedEventsQuantizingSettings>
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
        public void Quantize(IEnumerable<TimedEvent> objects, IGrid grid, TempoMap tempoMap, TimedEventsQuantizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            QuantizeInternal(objects, grid, tempoMap, settings);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Gets the time of an object that should be quantized.
        /// </summary>
        /// <param name="obj">Object to get time of.</param>
        /// <param name="settings">Settings according to which the object's time should be gotten.</param>
        /// <returns>The time of <paramref name="obj"/> that should be quantized.</returns>
        protected sealed override long GetObjectTime(TimedEvent obj, TimedEventsQuantizingSettings settings)
        {
            return obj.Time;
        }

        /// <summary>
        /// Sets the new time of an object.
        /// </summary>
        /// <param name="obj">Object to set time for.</param>
        /// <param name="time">New time after quantizing.</param>
        /// <param name="settings">Settings according to which the object's time should be set.</param>
        protected sealed override void SetObjectTime(TimedEvent obj, long time, TimedEventsQuantizingSettings settings)
        {
            obj.Time = time;
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
            TimedEvent obj,
            QuantizedTime quantizedTime,
            IGrid grid,
            TempoMap tempoMap,
            TimedEventsQuantizingSettings settings)
        {
            return new TimeProcessingInstruction(quantizedTime.NewTime);
        }

        #endregion
    }
}
