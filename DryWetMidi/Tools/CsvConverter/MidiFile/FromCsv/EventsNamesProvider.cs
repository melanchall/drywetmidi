using System.Linq;
using System.Reflection;

namespace Melanchall.DryWetMidi.Tools
{
    internal static class EventsNamesProvider
    {
        #region Constants


        private static readonly string[] EventsNames = typeof(RecordLabels.Events)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
            .Select(fi => fi.GetValue(null).ToString())
            .ToArray();

        #endregion

        #region Methods

        public static string[] Get()
        {
            return EventsNames;
        }

        #endregion
    }
}
