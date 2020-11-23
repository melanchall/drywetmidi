using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class BuildObjectsUtilitiesTests
    {
        #region Nested classes

        private sealed class TimedObjectComparer : IComparer
        {
            #region IComparer

            public int Compare(object x, object y)
            {
                var timedObject1 = x as ITimedObject;
                var timedObject2 = y as ITimedObject;

                if (ReferenceEquals(timedObject1, timedObject2))
                    return 1;

                if (ReferenceEquals(timedObject1, null))
                    return -1;

                if (ReferenceEquals(timedObject2, null))
                    return 1;

                var timesDifference = timedObject1.Time - timedObject2.Time;
                if (timesDifference != 0)
                    return Math.Sign(timesDifference);

                var lengthedObject1 = x as ILengthedObject;
                var lengthedObject2 = y as ILengthedObject;

                if (lengthedObject1 != null && lengthedObject2 == null)
                    return 1;

                if (lengthedObject1 == null && lengthedObject2 != null)
                    return -1;

                if (lengthedObject1 != null && lengthedObject2 != null)
                {
                    var lengthsDifference = lengthedObject1.Length - lengthedObject2.Length;
                    if (lengthsDifference != 0)
                        return Math.Sign(lengthsDifference);
                }

                return TimedObjectEquality.AreEqual(timedObject1, timedObject2, false) ? 0 : -1;
            }

            #endregion
        }

        #endregion

        #region Setup

        [OneTimeSetUp]
        public void SetUp()
        {
            TestContext.AddFormatter<ITimedObject>(obj =>
            {
                var timedObject = (ITimedObject)obj;
                var lengthedObject = obj as ILengthedObject;
                return lengthedObject != null
                    ? $"{obj} (T = {lengthedObject.Time}, L = {lengthedObject.Length})"
                    : $"{obj} (T = {timedObject.Time})";
            });
        }

        #endregion

        #region Private methods

        private void CheckObjectsBuilding(
            IEnumerable<ITimedObject> inputObjects,
            IEnumerable<ITimedObject> outputObjects,
            ObjectsBuildingSettings settings)
        {
            var actualObjects = inputObjects
                .BuildObjects(settings)
                .ToList();

            CollectionAssert.AreEqual(outputObjects, actualObjects, new TimedObjectComparer());
        }

        #endregion
    }
}
