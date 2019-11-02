using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Provides methods to randomize objects time.
    /// </summary>
    /// <typeparam name="TObject">The type of objects to quantize.</typeparam>
    /// <typeparam name="TSettings">The type of quantizer's settings.</typeparam>
    public abstract class Randomizer<TObject, TSettings>
        where TSettings : RandomizingSettings<TObject>, new()
    {
        #region Fields

        private readonly Random _random = new Random();

        #endregion

        #region Methods

        /// <summary>
        /// Randomizes objects time using the specified bounds and settings.
        /// </summary>
        /// <param name="objects">Objects to randomize.</param>
        /// <param name="bounds">Bounds to randomize time within.</param>
        /// <param name="tempoMap">Tempo map used to calculate time bounds to randomize within.</param>
        /// <param name="settings">Settings according to which objects should be randomized.</param>
        protected void RandomizeInternal(IEnumerable<TObject> objects, IBounds bounds, TempoMap tempoMap, TSettings settings)
        {
            settings = settings ?? new TSettings();

            Func<TObject, bool> filter = o => o != null && settings.Filter?.Invoke(o) != false;

            foreach (var obj in objects.Where(filter))
            {
                var time = GetObjectTime(obj, settings);

                time = RandomizeTime(time, bounds, _random, tempoMap);

                var instruction = OnObjectRandomizing(obj, time, settings);

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
        /// Gets the time of an object that should be randomized.
        /// </summary>
        /// <param name="obj">Object to get time of.</param>
        /// <param name="settings">Settings according to which the object's time should be gotten.</param>
        /// <returns>The time of <paramref name="obj"/> that should be randomized.</returns>
        protected abstract long GetObjectTime(TObject obj, TSettings settings);

        /// <summary>
        /// Sets the new time of an object.
        /// </summary>
        /// <param name="obj">Object to set time for.</param>
        /// <param name="time">New time after randomizing.</param>
        /// <param name="settings">Settings according to which the object's time should be set.</param>
        protected abstract void SetObjectTime(TObject obj, long time, TSettings settings);

        /// <summary>
        /// Performs additional actions before the new time will be set to an object.
        /// </summary>
        /// <remarks>
        /// Inside this method the new time can be changed or randomizing of an object can be cancelled.
        /// </remarks>
        /// <param name="obj">Object to randomize.</param>
        /// <param name="time">The new time that is going to be set to the object. Can be changed
        /// inside this method.</param>
        /// <param name="settings">Settings according to which object should be randomized.</param>
        /// <returns>An object indicating whether the new time should be set to the object
        /// or not. Also returned object contains that new time.</returns>
        protected abstract TimeProcessingInstruction OnObjectRandomizing(
            TObject obj,
            long time,
            TSettings settings);

        private static long RandomizeTime(long time, IBounds bounds, Random random, TempoMap tempoMap)
        {
            var timeBounds = bounds.GetBounds(time, tempoMap);

            var minTime = Math.Max(0, timeBounds.Item1) - 1;
            var maxTime = timeBounds.Item2;

            var difference = (int)Math.Abs(maxTime - minTime);
            return minTime + random.Next(difference) + 1;
        }

        #endregion
    }
}
