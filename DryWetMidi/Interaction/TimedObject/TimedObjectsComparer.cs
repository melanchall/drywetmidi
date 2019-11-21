using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class TimedObjectsComparer<TObject> : IComparer<TObject>
        where TObject : ITimedObject
    {
        #region IComparer<TObject>

        public int Compare(TObject x, TObject y)
        {
            if (ReferenceEquals(x, y))
                return 0;

            if (ReferenceEquals(x, null))
                return -1;

            if (ReferenceEquals(y, null))
                return 1;

            return Math.Sign(x.Time - y.Time);
        }

        #endregion
    }
}
