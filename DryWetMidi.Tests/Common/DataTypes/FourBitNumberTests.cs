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
    public sealed class FourBitNumberTests
    {
        private static readonly object[] ParseData_Valid = new[]
        {
            new object[] { "0", (FourBitNumber)0 },
            new object[] { "  0", (FourBitNumber)0 },
            new object[] { "1", (FourBitNumber)1 },
            new object[] { "1   ", (FourBitNumber)1 },
            new object[] { "15", (FourBitNumber)15 },
            new object[] { " 15 ", (FourBitNumber)15 },
        };

        private static readonly object[] ParseData_Invalid = new[]
        {
            new object[] { "a0" },
            new object[] { "1 0" },
            new object[] { "0b" },
            new object[] { "16" },
            new object[] { "abc" },
            new object[] { "-5" },
        };

#if NET7_0_OR_GREATER
        [Test]
        public void SerializeFourBitNumberToJson_1()
        {
            ClassicAssert.AreEqual("14", JsonSerializer.Serialize((FourBitNumber)14));
        }

        [Test]
        public void SerializeFourBitNumberToJson_2()
        {
            ClassicAssert.AreEqual("{\"Key\":14,\"Value\":\"AAA\"}", JsonSerializer.Serialize(new KeyValuePair<FourBitNumber, string>((FourBitNumber)14, "AAA")));
        }

        [Test]
        public void SerializeFourBitNumberKeyedDictionaryToJson()
        {
            ClassicAssert.AreEqual(
                "{\"14\":\"AAA\",\"15\":\"BBB\"}",
                JsonSerializer.Serialize(new Dictionary<FourBitNumber, string>
                {
                    [(FourBitNumber)14] = "AAA",
                    [(FourBitNumber)15] = "BBB",
                }));
        }

        [Test]
        public void DeserializeFourBitNumberFromJson()
        {
            ClassicAssert.AreEqual((FourBitNumber)4, JsonSerializer.Deserialize<FourBitNumber>("4"));
        }

        [Test]
        public void DeserializeFourBitNumberKeyedDictionaryFromJson()
        {
            ClassicAssert.AreEqual(
                new Dictionary<FourBitNumber, string>
                {
                    [(FourBitNumber)14] = "AAA",
                    [(FourBitNumber)15] = "BBB",
                },
                JsonSerializer.Deserialize<Dictionary<FourBitNumber, string>>("{\"14\":\"AAA\",\"15\":\"BBB\"}"));
        }

        [Test]
        public void DeserializeFourBitNumberFromJson_OutOfRange()
        {
            ClassicAssert.Throws<JsonException>(() => JsonSerializer.Deserialize<FourBitNumber>("17"));
        }

        [Test]
        public void DeserializeFourBitNumberKeyedDictionaryFromJson_OutOfRange()
        {
            ClassicAssert.Throws<JsonException>(() => JsonSerializer.Deserialize<Dictionary<FourBitNumber, string>>("{\"14\":\"AAA\",\"145\":\"BBB\"}"));
        }
#endif

        [TestCaseSource(nameof(ParseData_Valid))]
        public void Parse_Valid(string s, FourBitNumber expectedNumber) =>
            ClassicAssert.AreEqual(expectedNumber, FourBitNumber.Parse(s));

        [TestCaseSource(nameof(ParseData_Valid))]
        public void TryParse_Valid(string s, FourBitNumber expectedNumber)
        {
            ClassicAssert.IsTrue(FourBitNumber.TryParse(s, out var result), $"Failed to parse '{s}'.");
            ClassicAssert.AreEqual(expectedNumber, result, $"Parsed value is invalid.");
        }

        [TestCaseSource(nameof(ParseData_Invalid))]
        public void Parse_Invalid(string s) =>
            ClassicAssert.Throws<FormatException>(() => FourBitNumber.Parse(s));

        [TestCaseSource(nameof(ParseData_Invalid))]
        public void TryParse_Invalid(string s) =>
            ClassicAssert.IsFalse(FourBitNumber.TryParse(s, out _), $"Parsed invalid value '{s}'.");

        [Test]
        public void Parse_Invalid_EmptyOrNull([Values(null, "", "  ")] string s) =>
            ClassicAssert.Throws<ArgumentException>(() => FourBitNumber.Parse(s));

        [Test]
        public void TryParse_Invalid_EmptyOrNull([Values(null, "", "  ")] string s) =>
            ClassicAssert.IsFalse(FourBitNumber.TryParse(s, out _), $"Parsed invalid value '{s}'.");
    }
}
