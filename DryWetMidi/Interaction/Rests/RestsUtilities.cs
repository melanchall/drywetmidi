using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Provides methods to get rests between different objects.
    /// </summary>
    public static class RestsUtilities
    {
        #region Methods

        /// <summary>
        /// Returns a collection of the specified objects and rests between them according to
        /// the provided settings. Objects (including rests) in the result collection are
        /// ordered by time.
        /// </summary>
        /// <param name="timedObjects">The input objects collection.</param>
        /// <param name="settings">Settings according to which rests should be detected and built.</param>
        /// <returns>Collection with objects from <paramref name="timedObjects"/> and rests
        /// between them.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timedObjects"/> is <c>null</c>.</exception>
        public static IEnumerable<ITimedObject> WithRests(
            this IEnumerable<ITimedObject> timedObjects,
            RestDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(timedObjects), timedObjects);

            settings = settings ?? new RestDetectionSettings();

            timedObjects = GetSortedObjects(timedObjects);
            var rests = GetSortedRestsFromObjects(timedObjects, settings);
            return EnumerateObjectsAndRests(timedObjects, rests);
        }

        /// <summary>
        /// Returns rests between objects within the specified collection according to
        /// the provided settings. Rests in the result collection are ordered by time.
        /// </summary>
        /// <param name="timedObjects">The input objects collection.</param>
        /// <param name="settings">Settings according to which rests should be detected and built.</param>
        /// <returns>Collection of rests between objects within <paramref name="timedObjects"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timedObjects"/> is <c>null</c>.</exception>
        public static ICollection<Rest> GetRests(
            this IEnumerable<ITimedObject> timedObjects,
            RestDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(timedObjects), timedObjects);

            settings = settings ?? new RestDetectionSettings();

            timedObjects = GetSortedObjects(timedObjects);
            return GetSortedRestsFromObjects(timedObjects, settings);
        }

        private static IEnumerable<ITimedObject> GetSortedObjects(
            IEnumerable<ITimedObject> timedObjects)
        {
            return timedObjects is ISortedTimedObjectsImmutableCollection
                ? timedObjects
                : timedObjects.OrderBy(o => o.Time);
        }

        private static IEnumerable<ITimedObject> EnumerateObjectsAndRests(
            IEnumerable<ITimedObject> objects,
            IEnumerable<Rest> rests)
        {
            using (var objectsEnumerator = objects.GetEnumerator())
            using (var restsEnumerator = rests.GetEnumerator())
            {
                var objectCanBeTaken = objectsEnumerator.MoveNext();

                var restCanBeTaken = restsEnumerator.MoveNext();

                while (objectCanBeTaken && restCanBeTaken)
                {
                    var rest = restsEnumerator.Current;
                    var obj = objectsEnumerator.Current;

                    if (obj.Time <= rest.Time)
                    {
                        yield return obj;
                        objectCanBeTaken = objectsEnumerator.MoveNext();
                    }
                    else
                    {
                        yield return rest;
                        restCanBeTaken = restsEnumerator.MoveNext();
                    }
                }

                while (objectCanBeTaken)
                {
                    yield return objectsEnumerator.Current;
                    objectCanBeTaken = objectsEnumerator.MoveNext();
                }
            }
        }

        private static ICollection<Rest> GetSortedRestsFromObjects(
            IEnumerable<ITimedObject> objects,
            RestDetectionSettings restDetectionSettings)
        {
            restDetectionSettings = restDetectionSettings ?? new RestDetectionSettings();

            var endTimes = new Dictionary<object, long>();
            var keySelector = restDetectionSettings.KeySelector;

            var rests = new List<Rest>();

            foreach (var obj in objects)
            {
                var key = keySelector?.Invoke(obj);
                if (key == null)
                    continue;

                long lastEndTime;
                endTimes.TryGetValue(key, out lastEndTime);

                if (obj.Time > lastEndTime)
                {
                    var rest = new Rest(
                        lastEndTime,
                        obj.Time - lastEndTime,
                        key);

                    rests.Add(rest);
                }

                endTimes[key] = Math.Max(
                    lastEndTime,
                    obj is ILengthedObject ? ((ILengthedObject)obj).EndTime : obj.Time);
            }

            rests.Sort((r1, r2) => Math.Sign(r1.Time - r2.Time));

            return rests;
        }

        #endregion
    }
}
