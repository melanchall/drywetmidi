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

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            var changedTimedObject = obj as ChangedTimedObject;
            if (changedTimedObject == null)
                return false;

            return
                object.ReferenceEquals(Object, changedTimedObject.Object);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                var result = 17;
                result = result * 23 + Object.GetHashCode();
                return result;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{OldTime}: {Object}";
        }

        #endregion
    }
}
