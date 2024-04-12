using System.Collections;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class SortedTimedObjectsImmutableCollection<TObject> : ISortedTimedObjectsImmutableCollection, ICollection<TObject>
    {
        #region Fields

        private readonly ICollection<TObject> _objects;

        #endregion

        #region Constructor

        public SortedTimedObjectsImmutableCollection(
            ICollection<TObject> objects)
        {
            _objects = objects;
        }

        #endregion

        #region Properties

        public int Count
        {
            get { return _objects.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        #endregion

        public void Add(TObject item)
        {
            throw new System.NotSupportedException();
        }

        public void Clear()
        {
            throw new System.NotSupportedException();
        }

        public bool Contains(TObject item)
        {
            return _objects.Contains(item);
        }

        public void CopyTo(TObject[] array, int arrayIndex)
        {
            _objects.CopyTo(array, arrayIndex);
        }

        public IEnumerator<TObject> GetEnumerator()
        {
            return _objects.GetEnumerator();
        }

        public bool Remove(TObject item)
        {
            throw new System.NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_objects).GetEnumerator();
        }
    }
}
