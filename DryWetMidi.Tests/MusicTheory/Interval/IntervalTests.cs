using System;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

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

        #endregion

        #region Test methods

        [Test]
        [Description("Get upward interval and check its direction.")]
        public void GetUp()
        {
            Assert.AreEqual(IntervalDirection.Up, Interval.GetUp(SevenBitNumber.MaxValue).Direction);
        }

        [Test]
        [Description("Get downward interval and check its direction.")]
        public void GetDown()
        {
            Assert.AreEqual(IntervalDirection.Down, Interval.GetDown(SevenBitNumber.MaxValue).Direction);
        }

        [Test]
        [Description("Get upward interval and get its downward version.")]
        public void GetUp_Down()
        {
            Assert.AreEqual(IntervalDirection.Down, Interval.GetUp(SevenBitNumber.MaxValue).Down().Direction);
        }

        [Test]
        [Description("Get downward interval and get its upward version.")]
        public void GetDown_Up()
        {
            Assert.AreEqual(IntervalDirection.Up, Interval.GetDown(SevenBitNumber.MaxValue).Up().Direction);
        }

        [Test]
        [Description("Check that interval of the same steps number are equal by reference.")]
        public void CheckReferences()
        {
            Assert.AreSame(Interval.FromHalfSteps(10), Interval.FromHalfSteps(10));
        }

        [Test]
        [Description("Parse valid positive interval without leading sign (+).")]
        public void Parse_Valid_Positive_WithoutLeadingSign()
        {
            Parse("7", Interval.FromHalfSteps(7));
        }

        [Test]
        [Description("Parse valid positive interval with leading sign (+).")]
        public void Parse_Valid_Positive_WithLeadingSign()
        {
            Parse("+8", Interval.FromHalfSteps(8));
        }

        [Test]
        [Description("Parse valid interval of zero half steps.")]
        public void Parse_Valid_Zero()
        {
            Parse("0", Interval.FromHalfSteps(0));
        }

        [Test]
        [Description("Parse valid negative interval.")]
        public void Parse_Valid_Negative()
        {
            Parse("-123", Interval.FromHalfSteps(-123));
        }

        [Test]
        [Description("Parse invalid interval where an input string is empty.")]
        public void Parse_Invalid_EmptyInputString()
        {
            ParseInvalid<ArgumentException>(string.Empty);
        }

        [Test]
        [Description("Parse invalid positive interval where half steps number is out of range.")]
        public void Parse_Invalid_Positive_OutOfRange()
        {
            ParseInvalid<FormatException>("+239");
        }

        [Test]
        [Description("Parse invalid negative interval where half steps number is out of range.")]
        public void Parse_Invalid_Negative_OutOfRange()
        {
            ParseInvalid<FormatException>("-9239");
        }

        [Test]
        [Description("Parse invalid positive interval where half steps number is out of int range.")]
        public void Parse_Invalid_Positive_OutOfIntegerRange()
        {
            ParseInvalid<FormatException>("92399999999999999999999");
        }

        [Test]
        [Description("Parse invalid interval where an input string doesn't represent an interval.")]
        public void Parse_Invalid_NotAnInterval()
        {
            ParseInvalid<FormatException>("abc");
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
            Assert.AreEqual(expectedIsPerfect, Interval.IsPerfect(intervalNumber), "Interval number 'is perfect' is invalid.");
        }

        [Test]
        public void IsPerfect_OutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Interval.IsPerfect(0));
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

                Assert.AreEqual(expected, Interval.IsQualityApplicable(quality, intervalNumber), "Interval number 'is quality applicable' is invalid.");
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
                    Assert.IsFalse(Interval.IsQualityApplicable(quality, intervalNumber), "Interval applicability is invalid.");
                    continue;
                }

                var interval = Interval.Get(quality, intervalNumber);
                Assert.AreEqual(Interval.FromHalfSteps(expected.Value), interval, "Interval is invalid.");
            }
        }

        [TestCase("P5", IntervalQuality.Perfect, 5)]
        [TestCase("m3", IntervalQuality.Minor, 3)]
        [TestCase("M3", IntervalQuality.Major, 3)]
        [TestCase("D21", IntervalQuality.Diminished, 21)]
        [TestCase("d8", IntervalQuality.Diminished, 8)]
        [TestCase("A7", IntervalQuality.Augmented, 7)]
        [TestCase("a18", IntervalQuality.Augmented, 18)]
        public void Parse_QualityNumber(string input, IntervalQuality expectedIntervalQuality, int expectedIntervalNumber)
        {
            var parsedInterval = Interval.Parse(input);
            var expectedInterval = Interval.Get(expectedIntervalQuality, expectedIntervalNumber);
            Assert.AreEqual(expectedInterval, parsedInterval, "Parsed interval is invalid.");
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
                Assert.AreEqual(expectedInterval, interval, $"Invalid interval from definition [{intervalDefinition}].");
            }
        }

        #endregion

        #region Private methods

        private static void Parse(string input, Interval expectedInterval)
        {
            Interval.TryParse(input, out var actualInterval);
            Assert.AreEqual(expectedInterval,
                            actualInterval,
                            "TryParse: incorrect result.");

            actualInterval = Interval.Parse(input);
            Assert.AreEqual(expectedInterval,
                            actualInterval,
                            "Parse: incorrect result.");

            Assert.AreEqual(expectedInterval,
                            Interval.Parse(expectedInterval.ToString()),
                            "Parse: string representation was not parsed to the original interval.");
        }

        private static void ParseInvalid<TException>(string input)
            where TException : Exception
        {
            Assert.Throws<TException>(() => Interval.Parse(input));
        }

        #endregion
    }
}
