using System.Linq;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

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

            Assert.AreEqual(10, patternClone.Actions.Count(), "Actions count is invalid.");

            var actions = pattern.Actions.ToArray();
            var actionsClone = patternClone.Actions.ToArray();

            for (var i = 0; i < actions.Length; i++)
            {
                var action = actions[i];
                var actionClone = actionsClone[i];

                Assert.AreEqual(action.GetType(), actionClone.GetType(), "Action type is invalid.");
                Assert.AreNotSame(action, actionClone, "Actions are the same object.");
            }
        }

        #endregion
    }
}
