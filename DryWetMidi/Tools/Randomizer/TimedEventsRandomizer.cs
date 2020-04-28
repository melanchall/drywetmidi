using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Settings according to which timed events should be randomized.
    /// </summary>
    public sealed class TimedEventsRandomizingSettings : RandomizingSettings<TimedEvent>
    {
    }

    /// <summary>
    /// Provides methods to randomize timed events time.
    /// </summary>
    public sealed class TimedEventsRandomizer : Randomizer<TimedEvent, TimedEventsRandomizingSettings>
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
        public void Randomize(IEnumerable<TimedEvent> objects, IBounds bounds, TempoMap tempoMap, TimedEventsRandomizingSettings settings = null)
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
        protected override long GetObjectTime(TimedEvent obj, TimedEventsRandomizingSettings settings)
        {
            return obj.Time;
        }

        /// <summary>
        /// Sets the new time of an object.
        /// </summary>
        /// <param name="obj">Object to set time for.</param>
        /// <param name="time">New time after randomizing.</param>
        /// <param name="settings">Settings according to which the object's time should be set.</param>
        protected override void SetObjectTime(TimedEvent obj, long time, TimedEventsRandomizingSettings settings)
        {
            obj.Time = time;
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
        protected override TimeProcessingInstruction OnObjectRandomizing(TimedEvent obj, long time, TimedEventsRandomizingSettings settings)
        {
            return new TimeProcessingInstruction(time);
        }

        #endregion
    }
}
