using System.Linq;
using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    internal static class TimedObjectsCollectionTestUtilities
    {
        public static void CheckTimedObjectsCollectionTimes<TObject>(TimedObjectsCollection<TObject> collection, params long[] expectedTimes)
            where TObject : ITimedObject
        {
            CollectionAssert.AreEqual(expectedTimes, collection.Select(o => o.Time).ToArray());
        }
    }
}
