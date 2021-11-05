using System.Linq;
using System.Reflection;

namespace Melanchall.DryWetMidi.Tests.Common
{
    public static class MidiDevicesNames
    {
        #region Constants

        public const string DeviceA = "MIDI A";
        public const string DeviceB = "MIDI B";
        public const string DeviceC = "MIDI C";

        #endregion

        #region Methods

        public static string[] GetAllDevicesNames() => typeof(MidiDevicesNames)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.IsLiteral)
            .Select(f => f.GetValue(null).ToString())
            .ToArray();

        #endregion
    }
}
