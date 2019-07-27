using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.MusicTheory
{
    [TestFixture]
    public class IntervalTests
    {
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
