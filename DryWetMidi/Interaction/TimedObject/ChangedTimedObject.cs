namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Holds information about an object that has been changed within <see cref="IObservableTimedObjectsCollection"/>.
    /// </summary>
    /// <seealso cref="IObservableTimedObjectsCollection"/>
    /// <seealso cref="ObservableTimedObjectsCollectionChangedEventArgs"/>
    public sealed class ChangedTimedObject
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangedTimedObject"/> with the specified object and
        /// its time before changing.
        /// </summary>
        /// <param name="timedObject">The object that has been changed.</param>
        /// <param name="oldTime">The time (in ticks) of the <paramref name="timedObject"/> before changing.</param>
        public ChangedTimedObject(
            ITimedObject timedObject,
            long oldTime)
        {
            Object = timedObject;
            OldTime = oldTime;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the changed object.
        /// </summary>
        public ITimedObject Object { get; }

        /// <summary>
        /// Gets the time (in ticks) of the <see cref="Object"/> before changing.
        /// </summary>
        public long OldTime { get; }

        #endregion

        #region Overrides

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            var changedTimedObject = obj as ChangedTimedObject;
            if (changedTimedObject == null)
                return false;

            return
                object.ReferenceEquals(Object, changedTimedObject.Object);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var result = 17;
                result = result * 23 + Object.GetHashCode();
                return result;
            }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{OldTime}: {Object}";
        }

        #endregion
    }
}
