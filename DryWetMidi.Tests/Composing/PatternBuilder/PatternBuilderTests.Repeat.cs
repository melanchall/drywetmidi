using System;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Composing
{
    [TestFixture]
    public sealed partial class PatternBuilderTests
    {
        #region Test methods

        [Test]
        [Description("Try to repeat last action one time in case of no actions exist at the moment.")]
        public void Repeat_Last_Single_NoActions()
        {
            Assert.Throws<InvalidOperationException>(() => new PatternBuilder().Repeat());
        }

        [Test]
        [Description("Try to repeat last action several times in case of no actions exist at the moment.")]
        public void Repeat_Last_Multiple_Valid_NoActions()
        {
            Assert.Throws<InvalidOperationException>(() => new PatternBuilder().Repeat(2));
        }

        [Test]
        [Description("Try to repeat last action invalid number of times in case of no actions exist at the moment.")]
        public void Repeat_Last_Multiple_Invalid_NoActions()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PatternBuilder().Repeat(-7));
        }

        [Test]
        public void Repeat_Previous_NoActions()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PatternBuilder().Repeat(2, 2));
        }

        [Test]
        public void Repeat_Previous_NotEnoughActions()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new PatternBuilder()
                    .Anchor()
                    .Repeat(2, 2));
        }

        [Test]
        [Description("Repeat some actions and insert a note.")]
        public void Repeat_Previous()
        {
            var pattern = new PatternBuilder()
                .SetStep(MusicalTimeSpan.Eighth)

                .Anchor("A")
                .StepForward()
                .Anchor("B")
                .Repeat(2, 2)
                .MoveToNthAnchor("B", 2)
                .Note(NoteName.A)

                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 4, 3 * MusicalTimeSpan.Eighth, MusicalTimeSpan.Quarter)
            });
        }

        #endregion
    }
}
