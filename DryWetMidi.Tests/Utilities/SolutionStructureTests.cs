using Melanchall.DryWetMidi.Core;
using NUnit.Framework;
using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    [TestFixture]
    public sealed class SolutionStructureTests
    {
        #region Test methods

        [Test]
        public void CheckNamespaces()
        {
            var actualNamespaces = typeof(MidiFile)
                .Assembly
                .GetTypes()
                .Where(t => t.Namespace?.StartsWith("Melanchall") == true)
                .Select(t => (Namespace: t.Namespace, TypeName: t.Name))
                .Distinct()
                .ToArray();

            var expectedNamespaces = new[]
            {
                "Melanchall.DryWetMidi.Common",
                "Melanchall.DryWetMidi.Composing",
                "Melanchall.DryWetMidi.Core",
                "Melanchall.DryWetMidi.Interaction",
                "Melanchall.DryWetMidi.Multimedia",
                "Melanchall.DryWetMidi.MusicTheory",
                "Melanchall.DryWetMidi.Standards",
                "Melanchall.DryWetMidi.Tools",
            };

            var invalidNamespaces = actualNamespaces
                .Where(n => !expectedNamespaces.Contains(n.Namespace))
                .ToArray();
            Assert.IsFalse(
                invalidNamespaces.Any(),
                $"Following namespaces are invalid: {string.Join(", ", invalidNamespaces.Select(n => $"{n.Namespace} ({n.TypeName})"))}");
        }

        #endregion
    }
}
