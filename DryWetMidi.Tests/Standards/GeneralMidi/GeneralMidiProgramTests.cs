using Melanchall.DryWetMidi.Standards;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Standards
{
    [TestFixture]
    public sealed class GeneralMidiProgramTests
    {
        #region Test methods

        [Test]
        public void CheckAllValuesAreUnique()
        {
            var values = Enum
                .GetValues(typeof(GeneralMidiProgram))
                .Cast<GeneralMidiProgram>()
                .Select(v => (byte)v)
                .ToArray();
            var uniqueValues = values
                .Distinct()
                .ToArray();
            ClassicAssert.AreEqual(
                values.Length,
                uniqueValues.Length,
                "There are duplicate values.");
        }

        #endregion
    }
}
