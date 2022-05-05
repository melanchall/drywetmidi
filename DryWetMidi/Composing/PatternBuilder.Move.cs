using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using System;

namespace Melanchall.DryWetMidi.Composing
{
    public sealed partial class PatternBuilder
    {
        #region Methods

        /// <summary>
        /// Moves the current time by the specified step forward.
        /// </summary>
        /// <param name="step">Step to move by.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="step"/> is <c>null</c>.</exception>
        public PatternBuilder StepForward(ITimeSpan step)
        {
            ThrowIfArgument.IsNull(nameof(step), step);

            return AddAction(new StepForwardAction(step));
        }

        /// <summary>
        /// Moves the current time by the default step forward.
        /// </summary>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default step use <see cref="SetStep(ITimeSpan)"/> method. By default the step is 1/4.
        /// </remarks>
        public PatternBuilder StepForward()
        {
            return AddAction(new StepForwardAction(Step));
        }

        /// <summary>
        /// Moves the current time by the specified step back.
        /// </summary>
        /// <param name="step">Step to move by.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="step"/> is <c>null</c>.</exception>
        public PatternBuilder StepBack(ITimeSpan step)
        {
            ThrowIfArgument.IsNull(nameof(step), step);

            return AddAction(new StepBackAction(step));
        }

        /// <summary>
        /// Moves the current time by the default step back.
        /// </summary>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default step use <see cref="SetStep(ITimeSpan)"/> method. By default the step is 1/4.
        /// </remarks>
        public PatternBuilder StepBack()
        {
            return AddAction(new StepBackAction(Step));
        }

        /// <summary>
        /// Moves the current time to the specified one.
        /// </summary>
        /// <param name="time">Time to move to.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="time"/> is <c>null</c>.</exception>
        public PatternBuilder MoveToTime(ITimeSpan time)
        {
            ThrowIfArgument.IsNull(nameof(time), time);

            return AddAction(new MoveToTimeAction(time));
        }

        /// <summary>
        /// Moves the current time to the previous one.
        /// </summary>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// On every action current time is stored in the time history. To return to the last saved time
        /// you can call the <see cref="MoveToPreviousTime"/>.
        /// </remarks>
        public PatternBuilder MoveToPreviousTime()
        {
            return AddAction(new MoveToTimeAction());
        }

        /// <summary>
        /// Moves the current time to the start (zero time) of a pattern.
        /// </summary>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        public PatternBuilder MoveToStart()
        {
            return AddAction(new MoveToTimeAction((MidiTimeSpan)0));
        }

        #endregion
    }
}
