using System.Linq;
using System.Reflection;

namespace Melanchall.DryWetMidi.Tests
{
    public static class MidiEndpoints
    {
        #region Constants

        public const string A = "MIDI A";
        public const string B = "MIDI B 2";
        public const string C = "MIDI C 3";

        #endregion

        #region Methods

        public static string[] GetAllEndpointsNames() => typeof(MidiEndpoints)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.IsLiteral)
            .Select(f => f.GetValue(null).ToString())
            .ToArray();

        #endregion
    }
}
