using System;
using System.Collections;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Common
{
    internal sealed class SortedImmutableCollection<TObject> : ISortedCollection, ICollection<TObject>
    {
        #region Fields

        private readonly ICollection<TObject> _objects;

        #endregion

        #region Constructor

        public SortedImmutableCollection(
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

        #region Methods

        private void ThrowCannotBeModified()
        {
            throw new NotSupportedException("Collection cannot be modified.");
        }

        #endregion

        #region ICollection<TObject>

        public void Add(TObject item)
        {
            ThrowCannotBeModified();
        }

        public void Clear()
        {
            ThrowCannotBeModified();
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
            ThrowCannotBeModified();
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_objects).GetEnumerator();
        }

        #endregion
    }
}
