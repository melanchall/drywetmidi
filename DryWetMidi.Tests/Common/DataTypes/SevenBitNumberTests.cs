using System;
using Melanchall.DryWetMidi.Common;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System.Collections.Generic;

#if NET7_0_OR_GREATER
using System.Text.Json;
#endif

namespace Melanchall.DryWetMidi.Tests.Common
{
    [TestFixture]
    public sealed class SevenBitNumberTests
    {
        private static readonly object[] ParseData_Valid = new[]
        {
            new object[] { "0", (SevenBitNumber)0 },
            new object[] { "  0", (SevenBitNumber)0 },
            new object[] { "1", (SevenBitNumber)1 },
            new object[] { "1   ", (SevenBitNumber)1 },
            new object[] { "127", (SevenBitNumber)127 },
            new object[] { " 127 ", (SevenBitNumber)127 },
        };

        private static readonly object[] ParseData_Invalid = new[]
        {
            new object[] { "a0" },
            new object[] { "1 0" },
            new object[] { "0b" },
            new object[] { "128" },
            new object[] { "abc" },
            new object[] { "-5" },
        };

#if NET7_0_OR_GREATER
        [Test]
        public void SerializeSevenBitNumberToJson_1()
        {
            ClassicAssert.AreEqual("42", JsonSerializer.Serialize((SevenBitNumber)42));
        }

        [Test]
        public void SerializeSevenBitNumberToJson_2()
        {
            ClassicAssert.AreEqual("{\"Key\":42,\"Value\":\"AAA\"}", JsonSerializer.Serialize(new KeyValuePair<SevenBitNumber, string>((SevenBitNumber)42, "AAA")));
        }

        [Test]
        public void SerializeSevenBitNumberKeyedDictionaryToJson()
        {
            ClassicAssert.AreEqual(
                "{\"42\":\"AAA\",\"43\":\"BBB\"}",
                JsonSerializer.Serialize(new Dictionary<SevenBitNumber, string>
                {
                    [(SevenBitNumber)42] = "AAA",
                    [(SevenBitNumber)43] = "BBB",
                }));
        }

        [Test]
        public void DeserializeSevenBitNumberFromJson()
        {
            ClassicAssert.AreEqual((SevenBitNumber)42, JsonSerializer.Deserialize<SevenBitNumber>("42"));
        }

        [Test]
        public void DeserializeSevenBitNumberKeyedDictionaryFromJson()
        {
            ClassicAssert.AreEqual(
                new Dictionary<SevenBitNumber, string>
                {
                    [(SevenBitNumber)42] = "AAA",
                    [(SevenBitNumber)43] = "BBB",
                },
                JsonSerializer.Deserialize<Dictionary<SevenBitNumber, string>>("{\"42\":\"AAA\",\"43\":\"BBB\"}"));
        }

        [Test]
        public void DeserializeSevenBitNumberFromJson_OutOfRange()
        {
            ClassicAssert.Throws<JsonException>(() => JsonSerializer.Deserialize<SevenBitNumber>("128"));
        }

        [Test]
        public void DeserializeSevenBitNumberKeyedDictionaryFromJson_OutOfRange()
        {
            ClassicAssert.Throws<JsonException>(() => JsonSerializer.Deserialize<Dictionary<SevenBitNumber, string>>("{\"42\":\"AAA\",\"145\":\"BBB\"}"));
        }
#endif

        [TestCaseSource(nameof(ParseData_Valid))]
        public void Parse_Valid(string s, SevenBitNumber expectedNumber) =>
            ClassicAssert.AreEqual(expectedNumber, SevenBitNumber.Parse(s));

        [TestCaseSource(nameof(ParseData_Valid))]
        public void TryParse_Valid(string s, SevenBitNumber expectedNumber)
        {
            ClassicAssert.IsTrue(SevenBitNumber.TryParse(s, out var result), $"Failed to parse '{s}'.");
            ClassicAssert.AreEqual(expectedNumber, result, $"Parsed value is invalid.");
        }

        [TestCaseSource(nameof(ParseData_Invalid))]
        public void Parse_Invalid(string s) =>
            ClassicAssert.Throws<FormatException>(() => SevenBitNumber.Parse(s));

        [TestCaseSource(nameof(ParseData_Invalid))]
        public void TryParse_Invalid(string s) =>
            ClassicAssert.IsFalse(SevenBitNumber.TryParse(s, out _), $"Parsed invalid value '{s}'.");

        [Test]
        public void Parse_Invalid_EmptyOrNull([Values(null, "", "  ")] string s) =>
            ClassicAssert.Throws<ArgumentException>(() => SevenBitNumber.Parse(s));

        [Test]
        public void TryParse_Invalid_EmptyOrNull([Values(null, "", "  ")] string s) =>
            ClassicAssert.IsFalse(SevenBitNumber.TryParse(s, out _), $"Parsed invalid value '{s}'.");
    }
}
