using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using NUnit.Framework.Internal;
using NUnit.Framework.Legacy;
using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.MusicTheory
{
    [TestFixture]
    public class IntervalTests
    {
        #region Constants

        // Perfect, Minor, Major, Dim, Aug
        private static readonly object[] ParametersForGetByQualityCheck =
        {
            new object[] { 1, new int?[] { 0, null, null, null, 1 } },
            new object[] { 2, new int?[] { null, 1, 2, 0, 3 } },
            new object[] { 3, new int?[] { null, 3, 4, 2, 5 } },
            new object[] { 4, new int?[] { 5, null, null, 4, 6 } },
            new object[] { 5, new int?[] { 7, null, null, 6, 8 } },
            new object[] { 6, new int?[] { null, 8, 9, 7, 10 } },
            new object[] { 7, new int?[] { null, 10, 11, 9, 12 } },
            new object[] { 8, new int?[] { 12, null, null, 11, 13 } },

            new object[] { 9, new int?[] { null, 13, 14, 12, 15 } },
            new object[] { 10, new int?[] { null, 15, 16, 14, 17 } },
            new object[] { 11, new int?[] { 17, null, null, 16, 18 } },
            new object[] { 12, new int?[] { 19, null, null, 18, 20 } },
            new object[] { 13, new int?[] { null, 20, 21, 19, 22 } },
            new object[] { 14, new int?[] { null, 22, 23, 21, 24 } },
            new object[] { 15, new int?[] { 24, null, null, 23, 25 } },

            new object[] { 16, new int?[] { null, 25, 26, 24, 27 } },
            new object[] { 17, new int?[] { null, 27, 28, 26, 29 } },
            new object[] { 18, new int?[] { 29, null, null, 28, 30 } },
            new object[] { 19, new int?[] { 31, null, null, 30, 32 } },
            new object[] { 20, new int?[] { null, 32, 33, 31, 34 } },
            new object[] { 21, new int?[] { null, 34, 35, 33, 36 } },
            new object[] { 22, new int?[] { 36, null, null, 35, 37 } }
        };

        private static readonly object[] ParseData_Valid = new[]
        {
            new object[] { "7", Interval.FromHalfSteps(7) },
            new object[] { "+8", Interval.FromHalfSteps(8) },
            new object[] { " +8", Interval.FromHalfSteps(8) },
            new object[] { "0", Interval.FromHalfSteps(0) },
            new object[] { "-123", Interval.FromHalfSteps(-123) },
            new object[] { "-123  ", Interval.FromHalfSteps(-123) },
            new object[] { "P5", Interval.Get(IntervalQuality.Perfect, 5) },
            new object[] { "m3", Interval.Get(IntervalQuality.Minor, 3) },
            new object[] { "M3", Interval.Get(IntervalQuality.Major, 3) },
            new object[] { "D21", Interval.Get(IntervalQuality.Diminished, 21) },
            new object[] { "  D21", Interval.Get(IntervalQuality.Diminished, 21) },
            new object[] { "d8", Interval.Get(IntervalQuality.Diminished, 8) },
            new object[] { "A7", Interval.Get(IntervalQuality.Augmented, 7) },
            new object[] { "a18", Interval.Get(IntervalQuality.Augmented, 18) },
        };

        private static readonly object[] ParseData_Invalid = new[]
        {
            new object[] { "777" },
            new object[] { "++8" },
            new object[] { "+239" },
            new object[] { "+ 8" },
            new object[] { "x7" },
            new object[] { "7y" },
            new object[] { "abc" },
            new object[] { "--123" },
            new object[] { "10+" },
            new object[] { "10-" },
            new object[] { "92399999999999999999999" },
            new object[] { "P0" },
            new object[] { "P-1" },
            new object[] { "M0" },
        };

        #endregion

        #region Test methods

        [Test]
        [Description("Get upward interval and check its direction.")]
        public void GetUp()
        {
            ClassicAssert.AreEqual(IntervalDirection.Up, Interval.GetUp(SevenBitNumber.MaxValue).Direction);
        }

        [Test]
        [Description("Get downward interval and check its direction.")]
        public void GetDown()
        {
            ClassicAssert.AreEqual(IntervalDirection.Down, Interval.GetDown(SevenBitNumber.MaxValue).Direction);
        }

        [Test]
        [Description("Get upward interval and get its downward version.")]
        public void GetUp_Down()
        {
            ClassicAssert.AreEqual(IntervalDirection.Down, Interval.GetUp(SevenBitNumber.MaxValue).Down().Direction);
        }

        [Test]
        [Description("Get downward interval and get its upward version.")]
        public void GetDown_Up()
        {
            ClassicAssert.AreEqual(IntervalDirection.Up, Interval.GetDown(SevenBitNumber.MaxValue).Up().Direction);
        }

        [Test]
        [Description("Check that interval of the same steps number are equal by reference.")]
        public void CheckReferences()
        {
            ClassicAssert.AreSame(Interval.FromHalfSteps(10), Interval.FromHalfSteps(10));
        }

        [TestCase(1, true)]
        [TestCase(2, false)]
        [TestCase(3, false)]
        [TestCase(4, true)]
        [TestCase(5, true)]
        [TestCase(6, false)]
        [TestCase(7, false)]
        [TestCase(8, true)]
        [TestCase(9, false)]
        [TestCase(10, false)]
        [TestCase(11, true)]
        [TestCase(12, true)]
        [TestCase(13, false)]
        [TestCase(14, false)]
        [TestCase(15, true)]
        public void IsPerfect(int intervalNumber, bool expectedIsPerfect)
        {
            ClassicAssert.AreEqual(expectedIsPerfect, Interval.IsPerfect(intervalNumber), "Interval number 'is perfect' is invalid.");
        }

        [Test]
        public void IsPerfect_OutOfRange()
        {
            ClassicAssert.Throws<ArgumentOutOfRangeException>(() => Interval.IsPerfect(0));
        }

        [TestCase(1, new[] { true, false, false, false, true })]
        [TestCase(2, new[] { false, true, true, true, true })]
        [TestCase(3, new[] { false, true, true, true, true })]
        [TestCase(4, new[] { true, false, false, true, true })]
        [TestCase(5, new[] { true, false, false, true, true })]
        [TestCase(6, new[] { false, true, true, true, true })]
        [TestCase(7, new[] { false, true, true, true, true })]
        [TestCase(8, new[] { true, false, false, true, true })]
        [TestCase(9, new[] { false, true, true, true, true })]
        [TestCase(10, new[] { false, true, true, true, true })]
        [TestCase(11, new[] { true, false, false, true, true })]
        [TestCase(12, new[] { true, false, false, true, true })]
        [TestCase(13, new[] { false, true, true, true, true })]
        [TestCase(14, new[] { false, true, true, true, true })]
        [TestCase(15, new[] { true, false, false, true, true })]
        public void IsQualityApplicable(int intervalNumber, bool[] expectedIsApplicable)
        {
            var qualities = new[]
            {
                IntervalQuality.Perfect,
                IntervalQuality.Minor,
                IntervalQuality.Major,
                IntervalQuality.Diminished,
                IntervalQuality.Augmented
            };

            for (var i = 0; i < qualities.Length; i++)
            {
                var quality = qualities[i];
                var expected = expectedIsApplicable[i];

                ClassicAssert.AreEqual(expected, Interval.IsQualityApplicable(quality, intervalNumber), "Interval number 'is quality applicable' is invalid.");
            }
        }

        [Test]
        [TestCaseSource(nameof(ParametersForGetByQualityCheck))]
        public void Get_ByQuality(int intervalNumber, int?[] expectedHalfTones)
        {
            var qualities = new[]
            {
                IntervalQuality.Perfect,
                IntervalQuality.Minor,
                IntervalQuality.Major,
                IntervalQuality.Diminished,
                IntervalQuality.Augmented
            };

            for (var i = 0; i < qualities.Length; i++)
            {
                var quality = qualities[i];
                var expected = expectedHalfTones[i];
                if (expected == null)
                {
                    ClassicAssert.IsFalse(Interval.IsQualityApplicable(quality, intervalNumber), "Interval applicability is invalid.");
                    continue;
                }

                var interval = Interval.Get(quality, intervalNumber);
                ClassicAssert.AreEqual(Interval.FromHalfSteps(expected.Value), interval, "Interval is invalid.");
            }
        }

        [TestCaseSource(nameof(ParseData_Valid))]
        public void Parse_Valid(string input, Interval expectedInterval)
        {
            var parsedInterval = Interval.Parse(input);
            ClassicAssert.AreEqual(expectedInterval, parsedInterval, "Parsed interval is invalid.");
        }

        [TestCaseSource(nameof(ParseData_Valid))]
        public void TryParse_Valid(string input, Interval expectedInterval)
        {
            var parsedInterval = Interval.TryParse(input, out var interval);
            ClassicAssert.IsTrue(parsedInterval, "Interval parsing failed.");
            ClassicAssert.AreEqual(expectedInterval, interval, "Parsed interval is invalid.");
        }

        [TestCaseSource(nameof(ParseData_Invalid))]
        public void Parse_Invalid(string input) =>
            Assert.Throws<FormatException>(() => Interval.Parse(input));

        [TestCaseSource(nameof(ParseData_Invalid))]
        public void TryParse_Invalid(string input)=>
            ClassicAssert.IsFalse(Interval.TryParse(input, out var interval), "Interval parsing should have failed.");

        [Test]
        public void Parse_Invalid_EmptyOrNull([Values(null, "", "  ")] string input) =>
            ClassicAssert.Throws<ArgumentException>(() => Interval.Parse(input), "Interval parsing did not throw an exception.");

        [Test]
        public void TryParse_Invalid_EmptyOrNull([Values(null, "", "  ")] string input) =>
            ClassicAssert.IsFalse(Interval.TryParse(input, out var interval), $"Parsed invalid value '{input}'.");

        [TestCase(0, new object[] { new object[] { 1, IntervalQuality.Perfect }, new object[] { 2, IntervalQuality.Diminished } })]
        [TestCase(1, new object[] { new object[] { 2, IntervalQuality.Minor }, new object[] { 1, IntervalQuality.Augmented } })]
        [TestCase(2, new object[] { new object[] { 2, IntervalQuality.Major }, new object[] { 3, IntervalQuality.Diminished } })]
        [TestCase(3, new object[] { new object[] { 3, IntervalQuality.Minor }, new object[] { 2, IntervalQuality.Augmented } })]
        [TestCase(4, new object[] { new object[] { 3, IntervalQuality.Major }, new object[] { 4, IntervalQuality.Diminished } })]
        [TestCase(5, new object[] { new object[] { 4, IntervalQuality.Perfect }, new object[] { 3, IntervalQuality.Augmented } })]
        [TestCase(6, new object[] { new object[] { 5, IntervalQuality.Diminished }, new object[] { 4, IntervalQuality.Augmented } })]
        [TestCase(7, new object[] { new object[] { 5, IntervalQuality.Perfect }, new object[] { 6, IntervalQuality.Diminished } })]
        [TestCase(8, new object[] { new object[] { 6, IntervalQuality.Minor }, new object[] { 5, IntervalQuality.Augmented } })]
        [TestCase(9, new object[] { new object[] { 6, IntervalQuality.Major }, new object[] { 7, IntervalQuality.Diminished } })]
        [TestCase(10, new object[] { new object[] { 7, IntervalQuality.Minor }, new object[] { 6, IntervalQuality.Augmented } })]
        [TestCase(11, new object[] { new object[] { 7, IntervalQuality.Major }, new object[] { 8, IntervalQuality.Diminished } })]
        [TestCase(12, new object[] { new object[] { 8, IntervalQuality.Perfect }, new object[] { 7, IntervalQuality.Augmented }, new object[] { 9, IntervalQuality.Diminished } })]
        [TestCase(13, new object[] { new object[] { 9, IntervalQuality.Minor }, new object[] { 8, IntervalQuality.Augmented } })]
        [TestCase(14, new object[] { new object[] { 9, IntervalQuality.Major }, new object[] { 10, IntervalQuality.Diminished } })]
        [TestCase(15, new object[] { new object[] { 10, IntervalQuality.Minor }, new object[] { 9, IntervalQuality.Augmented } })]
        [TestCase(16, new object[] { new object[] { 10, IntervalQuality.Major }, new object[] { 11, IntervalQuality.Diminished } })]
        [TestCase(17, new object[] { new object[] { 11, IntervalQuality.Perfect }, new object[] { 10, IntervalQuality.Augmented } })]
        [TestCase(18, new object[] { new object[] { 12, IntervalQuality.Diminished }, new object[] { 11, IntervalQuality.Augmented } })]
        [TestCase(19, new object[] { new object[] { 12, IntervalQuality.Perfect }, new object[] { 13, IntervalQuality.Diminished } })]
        [TestCase(20, new object[] { new object[] { 13, IntervalQuality.Minor }, new object[] { 12, IntervalQuality.Augmented } })]
        [TestCase(21, new object[] { new object[] { 13, IntervalQuality.Major }, new object[] { 14, IntervalQuality.Diminished } })]
        [TestCase(22, new object[] { new object[] { 14, IntervalQuality.Minor }, new object[] { 13, IntervalQuality.Augmented } })]
        [TestCase(23, new object[] { new object[] { 14, IntervalQuality.Major }, new object[] { 15, IntervalQuality.Diminished } })]
        [TestCase(24, new object[] { new object[] { 15, IntervalQuality.Perfect }, new object[] { 14, IntervalQuality.Augmented }, new object[] { 16, IntervalQuality.Diminished } })]
        [TestCase(25, new object[] { new object[] { 16, IntervalQuality.Minor }, new object[] { 15, IntervalQuality.Augmented } })]
        public void GetIntervalDefinitions(int halfSteps, object[] expectedIntervalDefinitions)
        {
            var interval = Interval.FromHalfSteps(halfSteps);
            var intervalDefinitions = interval.GetIntervalDefinitions().ToArray();

            CollectionAssert.AreEqual(
                expectedIntervalDefinitions.OfType<object[]>()
                                           .Select(intervalNumberAndQuality => new IntervalDefinition(
                                               (int)intervalNumberAndQuality[0],
                                               (IntervalQuality)intervalNumberAndQuality[1]))
                                           .ToArray(),
                intervalDefinitions,
                "Interval definitions are invalid.");
        }

        [Test]
        public void SortIntervals()
        {
            var intervals = new[]
            {
                Interval.FromHalfSteps(100),
                Interval.FromHalfSteps(0),
                Interval.FromHalfSteps(-100),
                Interval.FromHalfSteps(10),
                Interval.FromHalfSteps(2),
                Interval.FromHalfSteps(-10),
                Interval.FromHalfSteps(-2)
            };

            var sortedIntervals = intervals.OrderBy(i => i).ToArray();

            CollectionAssert.AreEqual(
                new[]
                {
                    Interval.FromHalfSteps(-100),
                    Interval.FromHalfSteps(-10),
                    Interval.FromHalfSteps(-2),
                    Interval.FromHalfSteps(0),
                    Interval.FromHalfSteps(2),
                    Interval.FromHalfSteps(10),
                    Interval.FromHalfSteps(100)
                },
                sortedIntervals,
                "Intervals are sorted incorrectly.");
        }

        [TestCase(0, new object[] { new object[] { 1, IntervalQuality.Perfect }, new object[] { 2, IntervalQuality.Diminished } })]
        [TestCase(1, new object[] { new object[] { 2, IntervalQuality.Minor }, new object[] { 1, IntervalQuality.Augmented } })]
        [TestCase(2, new object[] { new object[] { 2, IntervalQuality.Major }, new object[] { 3, IntervalQuality.Diminished } })]
        [TestCase(3, new object[] { new object[] { 3, IntervalQuality.Minor }, new object[] { 2, IntervalQuality.Augmented } })]
        [TestCase(4, new object[] { new object[] { 3, IntervalQuality.Major }, new object[] { 4, IntervalQuality.Diminished } })]
        [TestCase(5, new object[] { new object[] { 4, IntervalQuality.Perfect }, new object[] { 3, IntervalQuality.Augmented } })]
        [TestCase(6, new object[] { new object[] { 5, IntervalQuality.Diminished }, new object[] { 4, IntervalQuality.Augmented } })]
        [TestCase(7, new object[] { new object[] { 5, IntervalQuality.Perfect }, new object[] { 6, IntervalQuality.Diminished } })]
        [TestCase(8, new object[] { new object[] { 6, IntervalQuality.Minor }, new object[] { 5, IntervalQuality.Augmented } })]
        [TestCase(9, new object[] { new object[] { 6, IntervalQuality.Major }, new object[] { 7, IntervalQuality.Diminished } })]
        [TestCase(10, new object[] { new object[] { 7, IntervalQuality.Minor }, new object[] { 6, IntervalQuality.Augmented } })]
        [TestCase(11, new object[] { new object[] { 7, IntervalQuality.Major }, new object[] { 8, IntervalQuality.Diminished } })]
        [TestCase(12, new object[] { new object[] { 8, IntervalQuality.Perfect }, new object[] { 7, IntervalQuality.Augmented }, new object[] { 9, IntervalQuality.Diminished } })]
        [TestCase(13, new object[] { new object[] { 9, IntervalQuality.Minor }, new object[] { 8, IntervalQuality.Augmented } })]
        [TestCase(14, new object[] { new object[] { 9, IntervalQuality.Major }, new object[] { 10, IntervalQuality.Diminished } })]
        [TestCase(15, new object[] { new object[] { 10, IntervalQuality.Minor }, new object[] { 9, IntervalQuality.Augmented } })]
        [TestCase(16, new object[] { new object[] { 10, IntervalQuality.Major }, new object[] { 11, IntervalQuality.Diminished } })]
        [TestCase(17, new object[] { new object[] { 11, IntervalQuality.Perfect }, new object[] { 10, IntervalQuality.Augmented } })]
        [TestCase(18, new object[] { new object[] { 12, IntervalQuality.Diminished }, new object[] { 11, IntervalQuality.Augmented } })]
        [TestCase(19, new object[] { new object[] { 12, IntervalQuality.Perfect }, new object[] { 13, IntervalQuality.Diminished } })]
        [TestCase(20, new object[] { new object[] { 13, IntervalQuality.Minor }, new object[] { 12, IntervalQuality.Augmented } })]
        [TestCase(21, new object[] { new object[] { 13, IntervalQuality.Major }, new object[] { 14, IntervalQuality.Diminished } })]
        [TestCase(22, new object[] { new object[] { 14, IntervalQuality.Minor }, new object[] { 13, IntervalQuality.Augmented } })]
        [TestCase(23, new object[] { new object[] { 14, IntervalQuality.Major }, new object[] { 15, IntervalQuality.Diminished } })]
        [TestCase(24, new object[] { new object[] { 15, IntervalQuality.Perfect }, new object[] { 14, IntervalQuality.Augmented }, new object[] { 16, IntervalQuality.Diminished } })]
        [TestCase(25, new object[] { new object[] { 16, IntervalQuality.Minor }, new object[] { 15, IntervalQuality.Augmented } })]
        public void FromDefinition(int expectedHalfSteps, object[] intervalDefinitions)
        {
            var expectedInterval = Interval.FromHalfSteps(expectedHalfSteps);

            foreach (var intervalDefinition in intervalDefinitions.OfType<object[]>()
                .Select(intervalNumberAndQuality => new IntervalDefinition((int)intervalNumberAndQuality[0], (IntervalQuality)intervalNumberAndQuality[1])))
            {
                var interval = Interval.FromDefinition(intervalDefinition);
                ClassicAssert.AreEqual(expectedInterval, interval, $"Invalid interval from definition [{intervalDefinition}].");
            }
        }

        #endregion

        #region Private methods

        private static void Parse(string input, Interval expectedInterval)
        {
            Interval.TryParse(input, out var actualInterval);
            ClassicAssert.AreEqual(expectedInterval,
                            actualInterval,
                            "TryParse: incorrect result.");

            actualInterval = Interval.Parse(input);
            ClassicAssert.AreEqual(expectedInterval,
                            actualInterval,
                            "Parse: incorrect result.");

            ClassicAssert.AreEqual(expectedInterval,
                            Interval.Parse(expectedInterval.ToString()),
                            "Parse: string representation was not parsed to the original interval.");
        }

        private static void ParseInvalid<TException>(string input)
            where TException : Exception
        {
            ClassicAssert.Throws<TException>(() => Interval.Parse(input));
        }

        #endregion
    }
}
