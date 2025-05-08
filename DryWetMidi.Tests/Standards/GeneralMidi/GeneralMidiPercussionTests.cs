using Melanchall.DryWetMidi.Standards;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Standards
{
    [TestFixture]
    public sealed class GeneralMidiPercussionTests
    {
        #region Test methods

        [Test]
        public void CheckAllValuesAreUnique()
        {
            var values = Enum
                .GetValues(typeof(GeneralMidiPercussion))
                .Cast<GeneralMidiPercussion>()
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
