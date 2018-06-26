using System.Collections.Generic;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    internal static class RestEquality
    {
        #region Nested classes

        private sealed class RestComparer : IEqualityComparer<Rest>
        {
            #region IEqualityComparer<Rest>

            public bool Equals(Rest rest1, Rest rest2)
            {
                if (ReferenceEquals(rest1, rest2))
                    return true;

                if (ReferenceEquals(null, rest1) || ReferenceEquals(null, rest2))
                    return false;

                return rest1.NoteNumber == rest2.NoteNumber &&
                       rest1.Channel == rest2.Channel &&
                       rest1.Time == rest2.Time &&
                       rest1.Length == rest2.Length;
            }

            public int GetHashCode(Rest rest)
            {
                return rest.NoteNumber.GetHashCode() ^
                       rest.Channel.GetHashCode() ^
                       rest.Time.GetHashCode() ^
                       rest.Length.GetHashCode();
            }

            #endregion
        }

        #endregion

        #region Methods

        public static bool AreEqual(Rest rest1, Rest rest2)
        {
            return new RestComparer().Equals(rest1, rest2);
        }

        #endregion
    }
}
