using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class ValueLineTests
    {
        #region Test methods

        [Test]
        public void Construct()
        {
            const string defaultValue = "A";

            var valueLine = new ValueLine<string>(defaultValue);

            CollectionAssert.IsEmpty(valueLine, "Value line contains changes.");
            ClassicAssert.AreEqual(defaultValue, valueLine.GetValueAtTime(0), "Invalid value at the start.");
            ClassicAssert.AreEqual(defaultValue, valueLine.GetValueAtTime(100), "Invalid value at the middle.");
        }

        [Test]
        public void SetValue_Default_AtStart()
        {
            const string defaultValue = "A";

            var valueLine = new ValueLine<string>(defaultValue);
            valueLine.SetValue(0, defaultValue);

            CollectionAssert.IsEmpty(valueLine, "Value line contains changes.");
            ClassicAssert.AreEqual(defaultValue, valueLine.GetValueAtTime(0), "Invalid value at the start.");
            ClassicAssert.AreEqual(defaultValue, valueLine.GetValueAtTime(100), "Invalid value at the middle.");
        }

        [Test]
        public void SetValue_Default_AtMiddle()
        {
            const string defaultValue = "A";
            const long valueChangeTime = 100;

            var valueLine = new ValueLine<string>(defaultValue);
            valueLine.SetValue(valueChangeTime, defaultValue);

            CollectionAssert.IsEmpty(valueLine, "Value line contains changes.");
            ClassicAssert.AreEqual(defaultValue, valueLine.GetValueAtTime(0), "Invalid value at the start.");
            ClassicAssert.AreEqual(defaultValue, valueLine.GetValueAtTime(valueChangeTime), "Invalid value at the value change time.");
            ClassicAssert.AreEqual(defaultValue, valueLine.GetValueAtTime(valueChangeTime + 1), "Invalid value after the value change time.");
        }

        [Test]
        public void SetValue_NonDefault_AtStart()
        {
            const string defaultValue = "A";
            const string nonDefaultValue = "B";

            var valueLine = new ValueLine<string>(defaultValue);
            valueLine.SetValue(0, nonDefaultValue);

            CollectionAssert.AreEqual(new[] { new ValueChange<string>(0, nonDefaultValue) }, valueLine, "Invalid value changes.");
            ClassicAssert.AreEqual(nonDefaultValue, valueLine.GetValueAtTime(0), "Invalid value at the start.");
            ClassicAssert.AreEqual(nonDefaultValue, valueLine.GetValueAtTime(100), "Invalid value at the middle.");
        }

        [Test]
        public void SetValue_NonDefault_AtMiddle()
        {
            const string defaultValue = "A";
            const string nonDefaultValue = "B";
            const long valueChangeTime = 100;

            var valueLine = new ValueLine<string>(defaultValue);
            valueLine.SetValue(valueChangeTime, nonDefaultValue);

            CollectionAssert.AreEqual(new[] { new ValueChange<string>(valueChangeTime, nonDefaultValue) }, valueLine, "Invalid value changes.");
            ClassicAssert.AreEqual(defaultValue, valueLine.GetValueAtTime(0), "Invalid value at the start.");
            ClassicAssert.AreEqual(nonDefaultValue, valueLine.GetValueAtTime(valueChangeTime), "Invalid value at the value change time.");
            ClassicAssert.AreEqual(nonDefaultValue, valueLine.GetValueAtTime(valueChangeTime + 1), "Invalid value after the value change time.");
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SetValue_Default_AtStart_Default_AtStart(bool checkValueChangesFirst)
        {
            const string defaultValue = "A";

            SetValue_TwoChanges(
                defaultValue: defaultValue,
                value1: defaultValue,
                valueChangeTime1: 0,
                value2: defaultValue,
                valueChangeTime2: 0,
                expectedValueChanges: Enumerable.Empty<ValueChange<string>>(),
                expectedValueAtStart: defaultValue,
                expectedValueBeforeFirstChange: defaultValue,
                expectedValueAtFirstChange: defaultValue,
                expectedValueAfterFirstChange: defaultValue,
                expectedValueBeforeSecondChange: defaultValue,
                expectedValueAtSecondChange: defaultValue,
                expectedValueAfterSecondChange: defaultValue,
                checkValueChangesFirst: checkValueChangesFirst);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SetValue_Default_AtStart_Default_AtMiddle(bool checkValueChangesFirst)
        {
            const string defaultValue = "A";

            SetValue_TwoChanges(
                defaultValue: defaultValue,
                value1: defaultValue,
                valueChangeTime1: 0,
                value2: defaultValue,
                valueChangeTime2: 100,
                expectedValueChanges: Enumerable.Empty<ValueChange<string>>(),
                expectedValueAtStart: defaultValue,
                expectedValueBeforeFirstChange: defaultValue,
                expectedValueAtFirstChange: defaultValue,
                expectedValueAfterFirstChange: defaultValue,
                expectedValueBeforeSecondChange: defaultValue,
                expectedValueAtSecondChange: defaultValue,
                expectedValueAfterSecondChange: defaultValue,
                checkValueChangesFirst: checkValueChangesFirst);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SetValue_Default_AtStart_NonDefault_AtStart(bool checkValueChangesFirst)
        {
            const string defaultValue = "A";
            const string nonDefaultValue = "B";

            SetValue_TwoChanges(
                defaultValue: defaultValue,
                value1: defaultValue,
                valueChangeTime1: 0,
                value2: nonDefaultValue,
                valueChangeTime2: 0,
                expectedValueChanges: new[] { new ValueChange<string>(0, nonDefaultValue) },
                expectedValueAtStart: nonDefaultValue,
                expectedValueBeforeFirstChange: nonDefaultValue,
                expectedValueAtFirstChange: nonDefaultValue,
                expectedValueAfterFirstChange: nonDefaultValue,
                expectedValueBeforeSecondChange: nonDefaultValue,
                expectedValueAtSecondChange: nonDefaultValue,
                expectedValueAfterSecondChange: nonDefaultValue,
                checkValueChangesFirst: checkValueChangesFirst);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SetValue_Default_AtStart_NonDefault_AtMiddle(bool checkValueChangesFirst)
        {
            const string defaultValue = "A";
            const string nonDefaultValue = "B";

            SetValue_TwoChanges(
                defaultValue: defaultValue,
                value1: defaultValue,
                valueChangeTime1: 0,
                value2: nonDefaultValue,
                valueChangeTime2: 100,
                expectedValueChanges: new[] { new ValueChange<string>(100, nonDefaultValue) },
                expectedValueAtStart: defaultValue,
                expectedValueBeforeFirstChange: defaultValue,
                expectedValueAtFirstChange: defaultValue,
                expectedValueAfterFirstChange: defaultValue,
                expectedValueBeforeSecondChange: defaultValue,
                expectedValueAtSecondChange: nonDefaultValue,
                expectedValueAfterSecondChange: nonDefaultValue,
                checkValueChangesFirst: checkValueChangesFirst);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SetValue_Default_AtMiddle_Default_AtStart(bool checkValueChangesFirst)
        {
            const string defaultValue = "A";

            SetValue_TwoChanges(
                defaultValue: defaultValue,
                value1: defaultValue,
                valueChangeTime1: 100,
                value2: defaultValue,
                valueChangeTime2: 0,
                expectedValueChanges: Enumerable.Empty<ValueChange<string>>(),
                expectedValueAtStart: defaultValue,
                expectedValueBeforeFirstChange: defaultValue,
                expectedValueAtFirstChange: defaultValue,
                expectedValueAfterFirstChange: defaultValue,
                expectedValueBeforeSecondChange: defaultValue,
                expectedValueAtSecondChange: defaultValue,
                expectedValueAfterSecondChange: defaultValue,
                checkValueChangesFirst: checkValueChangesFirst);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SetValue_Default_AtMiddle_Default_AtMiddle(bool checkValueChangesFirst)
        {
            const string defaultValue = "A";

            SetValue_TwoChanges(
                defaultValue: defaultValue,
                value1: defaultValue,
                valueChangeTime1: 100,
                value2: defaultValue,
                valueChangeTime2: 200,
                expectedValueChanges: Enumerable.Empty<ValueChange<string>>(),
                expectedValueAtStart: defaultValue,
                expectedValueBeforeFirstChange: defaultValue,
                expectedValueAtFirstChange: defaultValue,
                expectedValueAfterFirstChange: defaultValue,
                expectedValueBeforeSecondChange: defaultValue,
                expectedValueAtSecondChange: defaultValue,
                expectedValueAfterSecondChange: defaultValue,
                checkValueChangesFirst: checkValueChangesFirst);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SetValue_Default_AtMiddle_NonDefault_AtStart(bool checkValueChangesFirst)
        {
            const string defaultValue = "A";
            const string nonDefaultValue = "B";

            SetValue_TwoChanges(
                defaultValue: defaultValue,
                value1: defaultValue,
                valueChangeTime1: 100,
                value2: nonDefaultValue,
                valueChangeTime2: 0,
                expectedValueChanges: new[] { new ValueChange<string>(0, nonDefaultValue) },
                expectedValueAtStart: nonDefaultValue,
                expectedValueBeforeFirstChange: nonDefaultValue,
                expectedValueAtFirstChange: nonDefaultValue,
                expectedValueAfterFirstChange: nonDefaultValue,
                expectedValueBeforeSecondChange: nonDefaultValue,
                expectedValueAtSecondChange: nonDefaultValue,
                expectedValueAfterSecondChange: nonDefaultValue,
                checkValueChangesFirst: checkValueChangesFirst);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SetValue_Default_AtMiddle_NonDefault_AtMiddle(bool checkValueChangesFirst)
        {
            const string defaultValue = "A";
            const string nonDefaultValue = "B";

            SetValue_TwoChanges(
                defaultValue: defaultValue,
                value1: defaultValue,
                valueChangeTime1: 100,
                value2: nonDefaultValue,
                valueChangeTime2: 200,
                expectedValueChanges: new[] { new ValueChange<string>(200, nonDefaultValue) },
                expectedValueAtStart: defaultValue,
                expectedValueBeforeFirstChange: defaultValue,
                expectedValueAtFirstChange: defaultValue,
                expectedValueAfterFirstChange: defaultValue,
                expectedValueBeforeSecondChange: defaultValue,
                expectedValueAtSecondChange: nonDefaultValue,
                expectedValueAfterSecondChange: nonDefaultValue,
                checkValueChangesFirst: checkValueChangesFirst);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SetValue_NonDefault_AtStart_Default_AtStart(bool checkValueChangesFirst)
        {
            const string defaultValue = "A";
            const string nonDefaultValue = "B";

            SetValue_TwoChanges(
                defaultValue: defaultValue,
                value1: nonDefaultValue,
                valueChangeTime1: 0,
                value2: defaultValue,
                valueChangeTime2: 0,
                expectedValueChanges: Array.Empty<ValueChange<string>>(),
                expectedValueAtStart: defaultValue,
                expectedValueBeforeFirstChange: defaultValue,
                expectedValueAtFirstChange: defaultValue,
                expectedValueAfterFirstChange: defaultValue,
                expectedValueBeforeSecondChange: defaultValue,
                expectedValueAtSecondChange: defaultValue,
                expectedValueAfterSecondChange: defaultValue,
                checkValueChangesFirst: checkValueChangesFirst);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SetValue_NonDefault_AtStart_Default_AtMiddle(bool checkValueChangesFirst)
        {
            const string defaultValue = "A";
            const string nonDefaultValue = "B";

            SetValue_TwoChanges(
                defaultValue: defaultValue,
                value1: nonDefaultValue,
                valueChangeTime1: 0,
                value2: defaultValue,
                valueChangeTime2: 100,
                expectedValueChanges: new[] { new ValueChange<string>(0, nonDefaultValue), new ValueChange<string>(100, defaultValue) },
                expectedValueAtStart: nonDefaultValue,
                expectedValueBeforeFirstChange: nonDefaultValue,
                expectedValueAtFirstChange: nonDefaultValue,
                expectedValueAfterFirstChange: nonDefaultValue,
                expectedValueBeforeSecondChange: nonDefaultValue,
                expectedValueAtSecondChange: defaultValue,
                expectedValueAfterSecondChange: defaultValue,
                checkValueChangesFirst: checkValueChangesFirst);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SetValue_NonDefault_AtStart_NonDefault_AtStart(bool checkValueChangesFirst)
        {
            const string defaultValue = "A";
            const string nonDefaultValue1 = "B";
            const string nonDefaultValue2 = "C";

            SetValue_TwoChanges(
                defaultValue: defaultValue,
                value1: nonDefaultValue1,
                valueChangeTime1: 0,
                value2: nonDefaultValue2,
                valueChangeTime2: 0,
                expectedValueChanges: new[] { new ValueChange<string>(0, nonDefaultValue2) },
                expectedValueAtStart: nonDefaultValue2,
                expectedValueBeforeFirstChange: nonDefaultValue2,
                expectedValueAtFirstChange: nonDefaultValue2,
                expectedValueAfterFirstChange: nonDefaultValue2,
                expectedValueBeforeSecondChange: nonDefaultValue2,
                expectedValueAtSecondChange: nonDefaultValue2,
                expectedValueAfterSecondChange: nonDefaultValue2,
                checkValueChangesFirst: checkValueChangesFirst);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SetValue_NonDefault_AtStart_NonDefault_AtMiddle(bool checkValueChangesFirst)
        {
            const string defaultValue = "A";
            const string nonDefaultValue1 = "B";
            const string nonDefaultValue2 = "C";

            SetValue_TwoChanges(
                defaultValue: defaultValue,
                value1: nonDefaultValue1,
                valueChangeTime1: 0,
                value2: nonDefaultValue2,
                valueChangeTime2: 100,
                expectedValueChanges: new[] { new ValueChange<string>(0, nonDefaultValue1), new ValueChange<string>(100, nonDefaultValue2) },
                expectedValueAtStart: nonDefaultValue1,
                expectedValueBeforeFirstChange: nonDefaultValue1,
                expectedValueAtFirstChange: nonDefaultValue1,
                expectedValueAfterFirstChange: nonDefaultValue1,
                expectedValueBeforeSecondChange: nonDefaultValue1,
                expectedValueAtSecondChange: nonDefaultValue2,
                expectedValueAfterSecondChange: nonDefaultValue2,
                checkValueChangesFirst: checkValueChangesFirst);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SetValue_NonDefault_AtMiddle_Default_AtStart(bool checkValueChangesFirst)
        {
            const string defaultValue = "A";
            const string nonDefaultValue = "B";

            SetValue_TwoChanges(
                defaultValue: defaultValue,
                value1: nonDefaultValue,
                valueChangeTime1: 100,
                value2: defaultValue,
                valueChangeTime2: 0,
                expectedValueChanges: new[] { new ValueChange<string>(100, nonDefaultValue) },
                expectedValueAtStart: defaultValue,
                expectedValueBeforeFirstChange: defaultValue,
                expectedValueAtFirstChange: nonDefaultValue,
                expectedValueAfterFirstChange: nonDefaultValue,
                expectedValueBeforeSecondChange: defaultValue,
                expectedValueAtSecondChange: defaultValue,
                expectedValueAfterSecondChange: defaultValue,
                checkValueChangesFirst: checkValueChangesFirst);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SetValue_NonDefault_AtMiddle_Default_AtMiddle(bool checkValueChangesFirst)
        {
            const string defaultValue = "A";
            const string nonDefaultValue = "B";

            SetValue_TwoChanges(
                defaultValue: defaultValue,
                value1: nonDefaultValue,
                valueChangeTime1: 100,
                value2: defaultValue,
                valueChangeTime2: 200,
                expectedValueChanges: new[] { new ValueChange<string>(100, nonDefaultValue), new ValueChange<string>(200, defaultValue) },
                expectedValueAtStart: defaultValue,
                expectedValueBeforeFirstChange: defaultValue,
                expectedValueAtFirstChange: nonDefaultValue,
                expectedValueAfterFirstChange: nonDefaultValue,
                expectedValueBeforeSecondChange: nonDefaultValue,
                expectedValueAtSecondChange: defaultValue,
                expectedValueAfterSecondChange: defaultValue,
                checkValueChangesFirst: checkValueChangesFirst);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SetValue_NonDefault_AtMiddle_NonDefault_AtStart(bool checkValueChangesFirst)
        {
            const string defaultValue = "A";
            const string nonDefaultValue1 = "B";
            const string nonDefaultValue2 = "C";

            SetValue_TwoChanges(
                defaultValue: defaultValue,
                value1: nonDefaultValue1,
                valueChangeTime1: 100,
                value2: nonDefaultValue2,
                valueChangeTime2: 0,
                expectedValueChanges: new[] { new ValueChange<string>(0, nonDefaultValue2), new ValueChange<string>(100, nonDefaultValue1) },
                expectedValueAtStart: nonDefaultValue2,
                expectedValueBeforeFirstChange: nonDefaultValue2,
                expectedValueAtFirstChange: nonDefaultValue1,
                expectedValueAfterFirstChange: nonDefaultValue1,
                expectedValueBeforeSecondChange: nonDefaultValue2,
                expectedValueAtSecondChange: nonDefaultValue2,
                expectedValueAfterSecondChange: nonDefaultValue2,
                checkValueChangesFirst: checkValueChangesFirst);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SetValue_NonDefault_AtMiddle_NonDefault_AtMiddle(bool checkValueChangesFirst)
        {
            const string defaultValue = "A";
            const string nonDefaultValue1 = "B";
            const string nonDefaultValue2 = "C";

            SetValue_TwoChanges(
                defaultValue: defaultValue,
                value1: nonDefaultValue1,
                valueChangeTime1: 100,
                value2: nonDefaultValue2,
                valueChangeTime2: 200,
                expectedValueChanges: new[] { new ValueChange<string>(100, nonDefaultValue1), new ValueChange<string>(200, nonDefaultValue2) },
                expectedValueAtStart: defaultValue,
                expectedValueBeforeFirstChange: defaultValue,
                expectedValueAtFirstChange: nonDefaultValue1,
                expectedValueAfterFirstChange: nonDefaultValue1,
                expectedValueBeforeSecondChange: nonDefaultValue1,
                expectedValueAtSecondChange: nonDefaultValue2,
                expectedValueAfterSecondChange: nonDefaultValue2,
                checkValueChangesFirst: checkValueChangesFirst);
        }

        #endregion

        #region Private methods

        private void SetValue_TwoChanges<TValue>(
            TValue defaultValue,
            TValue value1,
            long valueChangeTime1,
            TValue value2,
            long valueChangeTime2,
            IEnumerable<ValueChange<TValue>> expectedValueChanges,
            TValue expectedValueAtStart,
            TValue expectedValueBeforeFirstChange,
            TValue expectedValueAtFirstChange,
            TValue expectedValueAfterFirstChange,
            TValue expectedValueBeforeSecondChange,
            TValue expectedValueAtSecondChange,
            TValue expectedValueAfterSecondChange,
            bool checkValueChangesFirst)
        {
            var valueLine = new ValueLine<TValue>(defaultValue);
            valueLine.SetValue(valueChangeTime1, value1);
            valueLine.SetValue(valueChangeTime2, value2);

            if (checkValueChangesFirst)
                CollectionAssert.AreEqual(expectedValueChanges, valueLine, "Invalid value changes.");

            ClassicAssert.AreEqual(expectedValueAtStart, valueLine.GetValueAtTime(0), "Invalid value at start.");
            if (valueChangeTime1 > 0)
                ClassicAssert.AreEqual(expectedValueBeforeFirstChange, valueLine.GetValueAtTime(valueChangeTime1 - 1), "Invalid value before first change.");
            ClassicAssert.AreEqual(expectedValueAtFirstChange, valueLine.GetValueAtTime(valueChangeTime1), "Invalid value at first change.");
            ClassicAssert.AreEqual(expectedValueAfterFirstChange, valueLine.GetValueAtTime(valueChangeTime1 + 1), "Invalid value after first change.");
            if (valueChangeTime2 > 0)
                ClassicAssert.AreEqual(expectedValueBeforeSecondChange, valueLine.GetValueAtTime(valueChangeTime2 - 1), "Invalid value before second change.");
            ClassicAssert.AreEqual(expectedValueAtSecondChange, valueLine.GetValueAtTime(valueChangeTime2), "Invalid value at second change.");
            ClassicAssert.AreEqual(expectedValueAfterSecondChange, valueLine.GetValueAtTime(valueChangeTime2 + 1), "Invalid value after second change.");

            CollectionAssert.AreEqual(expectedValueChanges, valueLine, "Invalid value changes.");
        }

        #endregion
    }
}
