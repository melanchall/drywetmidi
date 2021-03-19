using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class GetObjectsUtilitiesTests
    {
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

        private void GetObjects(
            IEnumerable<ITimedObject> inputObjects,
            IEnumerable<ITimedObject> outputObjects,
            ObjectType objectType,
            ObjectDetectionSettings settings)
        {
            var actualObjects = inputObjects
                .GetObjects(objectType, settings)
                .ToList();

            MidiAsserts.AreEqual(outputObjects, actualObjects, true, 0, "Objects are invalid.");
        }

        #endregion
    }
}
