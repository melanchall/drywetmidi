using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    internal sealed class TypesProvider
    {
        #region Methods

        public static IEnumerable<Type> GetAllEventTypes() =>
            GetAllTypes<MidiEvent>();

        public static IEnumerable<Type> GetAllTokensTypes() =>
            GetAllTypes<MidiToken>();

        private static IEnumerable<Type> GetAllTypes<TBase>()
        {
            var baseType = typeof(TBase);
            return baseType
                .Assembly
                .GetTypes()
                .Where(t => !t.IsAbstract && t.IsSubclassOf(baseType))
                .ToList();
        }

        #endregion
    }
}
