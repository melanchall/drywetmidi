using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
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
                "Melanchall.DryWetMidi.Configuration",
            };

            var invalidNamespaces = actualNamespaces
                .Where(n => !expectedNamespaces.Contains(n.Namespace))
                .ToArray();
            ClassicAssert.IsFalse(
                invalidNamespaces.Any(),
                $"Following namespaces are invalid: {string.Join(", ", invalidNamespaces.Select(n => $"{n.Namespace} ({n.TypeName})"))}");
        }

        [Test]
        public void CheckCustomExceptions()
        {
            var customExceptionsTypes = typeof(MidiException)
                .Assembly
                .GetTypes()
                .Where(t => typeof(Exception).IsAssignableFrom(t) && t != typeof(MidiException))
                .ToArray();

            CollectionAssert.IsNotEmpty(customExceptionsTypes, "No custom exceptions found.");

            var notDerivedFromMidiExceptionTypes = new List<Type>();

            foreach (var exception in customExceptionsTypes)
            {
                if (!typeof(MidiException).IsAssignableFrom(exception))
                    notDerivedFromMidiExceptionTypes.Add(exception);
            }

            CollectionAssert.IsEmpty(
                notDerivedFromMidiExceptionTypes,
                $"Following exceptions are not derived from {nameof(MidiException)}:{Environment.NewLine}{string.Join(", ", notDerivedFromMidiExceptionTypes.Select(t => t.Name))}");
        }

        #endregion
    }
}
