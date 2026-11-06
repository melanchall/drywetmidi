using System.Linq;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Composing
{
    [TestFixture]
    public sealed class PatternTests
    {
        #region Test methods

        [Test]
        public void Clone_Empty()
        {
            var pattern = new PatternBuilder()
                .Build();

            var patternClone = pattern.Clone();

            CollectionAssert.IsEmpty(patternClone.Actions, "Actions count is invalid.");
            CollectionAssert.AreEqual(pattern.Actions, patternClone.Actions, "Pattern clone is invalid.");
        }

        [Test]
        public void Clone()
        {
            var pattern = new PatternBuilder()
                .Note(Notes.A3)
                .Repeat(9)
                .Build();

            var patternClone = pattern.Clone();

            ClassicAssert.AreEqual(10, patternClone.Actions.Count(), "Actions count is invalid.");

            var actions = pattern.Actions.ToArray();
            var actionsClone = patternClone.Actions.ToArray();

            for (var i = 0; i < actions.Length; i++)
            {
                var action = actions[i];
                var actionClone = actionsClone[i];

                ClassicAssert.AreEqual(action.GetType(), actionClone.GetType(), "Action type is invalid.");
                ClassicAssert.AreNotSame(action, actionClone, "Actions are the same object.");
            }
        }

        [Test]
        public void Clone_PreservesNoteActionState([Values(
            PatternActionState.Enabled,
            PatternActionState.Disabled,
            PatternActionState.Excluded)] PatternActionState state)
        {
            var pattern = PatternTestUtilities.CreatePatternWithNoteActionState(state);

            var clonedPattern = pattern.Clone();

            PatternTestUtilities.TestActionStateNotes(clonedPattern, state, 1, false);
        }

        [Test]
        public void Clone_PreservesChordActionState([Values(
            PatternActionState.Enabled,
            PatternActionState.Disabled,
            PatternActionState.Excluded)] PatternActionState state)
        {
            var pattern = PatternTestUtilities.CreatePatternWithChordActionState(state);

            var clonedPattern = pattern.Clone();

            PatternTestUtilities.TestActionStateNotes(clonedPattern, state, 1, true);
        }

        #endregion
    }
}
