﻿using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Provides methods to quantize objects time.
    /// </summary>
    /// <typeparam name="TObject">The type of objects to quantize.</typeparam>
    /// <typeparam name="TSettings">The type of quantizer's settings.</typeparam>
    public abstract class Quantizer<TObject, TSettings>
        where TSettings : QuantizingSettings, new()
    {
        #region Methods

        /// <summary>
        /// Quantizes objects time using the specified grid and settings.
        /// </summary>
        /// <param name="objects">Objects to quantize.</param>
        /// <param name="grid">Grid to quantize objects by.</param>
        /// <param name="tempoMap">Tempo map used to calculate times to quantize by.</param>
        /// <param name="settings">Settings according to which objects should be quantized.</param>
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

                var newTimeIndex = FindNearestTime(times, oldTime, settings.DistanceCalculationType, tempoMap);
                var newTime = times[newTimeIndex];

                var instruction = OnObjectQuantizing(obj, newTime, grid, times, tempoMap, settings);

                switch (instruction.Action)
                {
                    case TimeProcessingAction.Apply:
                        SetObjectTime(obj, instruction.Time, settings);
                        break;
                    case TimeProcessingAction.Skip:
                        break;
                }
            }
        }

        /// <summary>
        /// Gets the time of an object that should be quantized.
        /// </summary>
        /// <param name="obj">Object to get time of.</param>
        /// <param name="settings">Settings according to which the object's time should be gotten.</param>
        /// <returns>The time of <paramref name="obj"/> that should be quantized.</returns>
        protected abstract long GetObjectTime(TObject obj, TSettings settings);

        /// <summary>
        /// Sets the new time of an object.
        /// </summary>
        /// <param name="obj">Object to set time for.</param>
        /// <param name="time">New time after quantizing.</param>
        /// <param name="settings">Settings according to which the object's time should be set.</param>
        protected abstract void SetObjectTime(TObject obj, long time, TSettings settings);

        /// <summary>
        /// Performs additional actions before the new time will be set to an object.
        /// </summary>
        /// <remarks>
        /// Inside this method the new time can be changed or quantizing of an object can be cancelled.
        /// </remarks>
        /// <param name="obj">Object to quantize.</param>
        /// <param name="time">The new time that is going to be set to the object. Can be changed
        /// inside this method.</param>
        /// <param name="grid">Grid to quantize object by.</param>
        /// <param name="gridTimes">Calculated grid's times object will be quantized by.</param>
        /// <param name="tempoMap">Tempo map used to quantize object.</param>
        /// <param name="settings">Settings according to which object should be quantized.</param>
        /// <returns>An object indicating whether the new time should be set to the object
        /// or not. Also returned object contains that new time.</returns>
        protected abstract TimeProcessingInstruction OnObjectQuantizing(
            TObject obj,
            long time,
            IGrid grid,
            IReadOnlyCollection<long> gridTimes,
            TempoMap tempoMap,
            TSettings settings);

        private static IEnumerable<long> GetGridTimes(IGrid grid, long lastTime, TempoMap tempoMap)
        {
            var times = grid.GetTimes(tempoMap);
            var enumerator = times.GetEnumerator();

            while (enumerator.MoveNext() && enumerator.Current < lastTime)
                yield return enumerator.Current;

            yield return enumerator.Current;
        }

        private static int FindNearestTime(IReadOnlyList<long> grid,
                                           long time,
                                           TimeSpanType distanceCalculationType,
                                           TempoMap tempoMap)
        {
            var difference = TimeSpanUtilities.GetMaxTimeSpan(distanceCalculationType);
            var nearestTimeIndex = -1;

            for (int i = 0; i < grid.Count; i++)
            {
                var gridTime = grid[i];

                var timeDelta = Math.Abs(time - gridTime);
                var convertedTimeDelta = LengthConverter.ConvertTo(timeDelta, distanceCalculationType, Math.Min(time, gridTime), tempoMap);
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