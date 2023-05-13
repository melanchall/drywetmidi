using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Composing
{
    public sealed partial class PatternBuilder
    {
        #region Methods

        /// <summary>
        /// Repeats the specified number of previous actions.
        /// </summary>
        /// <param name="actionsCount">Number of previous actions to repeat.</param>
        /// <param name="repeatsNumber">Count of repetitions.</param>
        /// <param name="settings">Settings according to which actions should be repeated.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// Note that <see cref="SetNoteLength(ITimeSpan)"/>, <see cref="SetOctave"/>,
        /// <see cref="SetStep(ITimeSpan)"/> and <see cref="SetVelocity(SevenBitNumber)"/> are not
        /// actions and will not be repeated since default values applies immediately on next actions.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="actionsCount"/> is negative.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="actionsCount"/> is greater than count of existing actions.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="repeatsNumber"/> is negative.</description>
        /// </item>
        /// </list>
        /// </exception>
        public PatternBuilder Repeat(int actionsCount, int repeatsNumber, RepeatSettings settings = null)
        {
            ThrowIfArgument.IsNegative(nameof(actionsCount), actionsCount, "Actions count is negative.");
            ThrowIfArgument.IsGreaterThan(
                nameof(actionsCount),
                actionsCount,
                _actions.Count,
                "Actions count is greater than existing actions count.");
            ThrowIfArgument.IsNegative(nameof(repeatsNumber), repeatsNumber, "Repetitions count is negative.");

            return RepeatActions(actionsCount, repeatsNumber, settings);
        }

        /// <summary>
        /// Repeats the previous action the specified number of times.
        /// </summary>
        /// <param name="repeatsNumber">Count of repetitions.</param>
        /// <param name="settings">Settings according to which actions should be repeated.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// Note that <see cref="SetNoteLength(ITimeSpan)"/>, <see cref="SetOctave"/>,
        /// <see cref="SetStep(ITimeSpan)"/> and <see cref="SetVelocity(SevenBitNumber)"/> are not
        /// actions and will not be repeated since default values applies immediately on next actions.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="repeatsNumber"/> is negative.</exception>
        /// <exception cref="InvalidOperationException">There are no actions to repeat.</exception>
        public PatternBuilder Repeat(int repeatsNumber, RepeatSettings settings = null)
        {
            ThrowIfArgument.IsNegative(nameof(repeatsNumber), repeatsNumber, "Repetitions count is negative.");

            if (!_actions.Any())
                throw new InvalidOperationException("There is no action to repeat.");

            return RepeatActions(1, repeatsNumber, settings);
        }

        /// <summary>
        /// Repeats the previous action one time.
        /// </summary>
        /// <param name="settings">Settings according to which actions should be repeated.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// Note that <see cref="SetNoteLength(ITimeSpan)"/>, <see cref="SetOctave"/>,
        /// <see cref="SetStep(ITimeSpan)"/> and <see cref="SetVelocity(SevenBitNumber)"/> are not
        /// actions and will not be repeated since default values applies immediately on next actions.
        /// </remarks>
        /// <exception cref="InvalidOperationException">There are no actions to repeat.</exception>
        public PatternBuilder Repeat(RepeatSettings settings = null)
        {
            if (!_actions.Any())
                throw new InvalidOperationException("There is no action to repeat.");

            return RepeatActions(1, 1, settings);
        }

        private PatternBuilder RepeatActions(int actionsCount, int repeatsNumber, RepeatSettings settings)
        {
            settings = settings ?? new RepeatSettings();

            var actionsToRepeat = _actions.Skip(_actions.Count - actionsCount).ToList();
            var newActions = Enumerable.Range(0, repeatsNumber).SelectMany(i => actionsToRepeat);

            foreach (var action in ProcessActions(newActions, settings))
            {
                AddAction(action);
            }

            return this;
        }

        private IEnumerable<PatternAction> ProcessActions(IEnumerable<PatternAction> actions, RepeatSettings settings)
        {
            var processNote = settings.NoteTransformation ?? (n => n);
            var processChord = settings.ChordTransformation ?? (c => c);

            foreach (var action in actions)
            {
                var addNoteAction = action as AddNoteAction;
                if (addNoteAction != null)
                {
                    yield return new AddNoteAction(processNote(addNoteAction.NoteDescriptor));
                    continue;
                }

                var addChordAction = action as AddChordAction;
                if (addChordAction != null)
                {
                    yield return new AddChordAction(processChord(addChordAction.ChordDescriptor));
                    continue;
                }

                var addPatternAction = action as AddPatternAction;
                if (addPatternAction != null)
                {
                    yield return new AddPatternAction(new Pattern(
                        ProcessActions(addPatternAction.Pattern.Actions, settings)));
                    continue;
                }

                yield return action;
            }
        }

        #endregion
    }
}
