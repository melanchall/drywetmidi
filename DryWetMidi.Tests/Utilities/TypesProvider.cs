using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    internal sealed class TypesProvider
    {
        #region Methods

        public static IEnumerable<Type> GetAllEventTypes()
        {
            var midiEventType = typeof(MidiEvent);
            return midiEventType
                .Assembly
                .GetTypes()
                .Where(t => !t.IsAbstract && t.IsSubclassOf(midiEventType))
                .ToList();
        }

        #endregion
    }
}
