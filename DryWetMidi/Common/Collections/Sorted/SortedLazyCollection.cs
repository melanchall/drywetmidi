using System.Collections;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Common
{
    internal sealed class SortedLazyCollection<TObject> : ISortedCollection, IEnumerable<TObject>
    {
        #region Fields

        private readonly IEnumerable<TObject> _objects;

        #endregion

        #region Constructor

        public SortedLazyCollection(
            IEnumerable<TObject> objects)
        {
            _objects = objects;
        }

        #endregion

        #region IEnumerable<TObject>

        public IEnumerator<TObject> GetEnumerator()
        {
            return _objects.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
