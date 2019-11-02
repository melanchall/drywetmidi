using System.Linq;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    internal static class TimedObjectsCollectionTestUtilities
    {
        #region Methods

        public static void CheckTimedObjectsCollectionTimes<TObject>(TimedObjectsCollection<TObject> collection, params long[] expectedTimes)
            where TObject : ITimedObject
        {
            CollectionAssert.AreEqual(expectedTimes, collection.Select(o => o.Time).ToArray());
        }

        #endregion
    }
}
