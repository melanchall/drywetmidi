using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Provides methods to randomize lengthed objects time.
    /// </summary>
    /// <typeparam name="TObject">The type of objects to quantize.</typeparam>
    /// <typeparam name="TSettings">The type of quantizer's settings.</typeparam>
    public abstract class LengthedObjectsRandomizer<TObject, TSettings> : Randomizer<TObject, TSettings>
        where TObject : ILengthedObject
        where TSettings : LengthedObjectsRandomizingSettings<TObject>, new()
    {
        #region Methods

        /// <summary>
        /// Randomizes objects time using the specified bounds and settings.
        /// </summary>
        /// <param name="objects">Objects to randomize.</param>
        /// <param name="bounds">Bounds to randomize time within.</param>
        /// <param name="tempoMap">Tempo map used to calculate time bounds to randomize within.</param>
        /// <param name="settings">Settings according to which objects should be randomized.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="objects"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="bounds"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public void Randomize(IEnumerable<TObject> objects, IBounds bounds, TempoMap tempoMap, TSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(bounds), bounds);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            RandomizeInternal(objects, bounds, tempoMap, settings);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Gets the time of an object that should be randomized.
        /// </summary>
        /// <param name="obj">Object to get time of.</param>
        /// <param name="settings">Settings according to which the object's time should be gotten.</param>
        /// <returns>The time of <paramref name="obj"/> that should be randomized.</returns>
        protected sealed override long GetObjectTime(TObject obj, TSettings settings)
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

        /// <summary>
        /// Sets the new time of an object.
        /// </summary>
        /// <param name="obj">Object to set time for.</param>
        /// <param name="time">New time after randomizing.</param>
        /// <param name="settings">Settings according to which the object's time should be set.</param>
        protected sealed override void SetObjectTime(TObject obj, long time, TSettings settings)
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
        protected override TimeProcessingInstruction OnObjectRandomizing(TObject obj, long time, TSettings settings)
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

            return new TimeProcessingInstruction(time);
        }

        #endregion
    }
}
