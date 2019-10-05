using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.MusicTheory
{
    [TestFixture]
    public sealed class NoteUtilitiesTests
    {
        #region Constants

        private static readonly object[] ParametersTransposeCheck =
        {
            new object[] { NoteName.A, 0, NoteName.A },
            new object[] { NoteName.A, 3, NoteName.C },
            new object[] { NoteName.A, 12, NoteName.A },
            new object[] { NoteName.A, 36, NoteName.A },
            new object[] { NoteName.A, -3, NoteName.FSharp },
            new object[] { NoteName.A, -12, NoteName.A },
            new object[] { NoteName.A, -36, NoteName.A },
        };

        #endregion

        #region Test methods

        [Test]
        [TestCaseSource(nameof(ParametersTransposeCheck))]
        public void Transpose(NoteName noteName, int halfSteps, NoteName expectedNoteName)
        {
            var interval = Interval.FromHalfSteps(halfSteps);
            var actualNoteName = noteName.Transpose(interval);
            Assert.AreEqual(expectedNoteName, actualNoteName, "Transposed note name is invalid.");
        }

        #endregion
    }
}
