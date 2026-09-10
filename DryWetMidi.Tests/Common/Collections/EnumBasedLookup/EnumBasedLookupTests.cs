using Melanchall.DryWetMidi.Common;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Common
{
    [TestFixture]
    public sealed class EnumBasedLookupTests
    {
        public enum EnumFromZero
        {
            A,
            B,
            C,
            D,
        }

        public enum EnumNotFromZero : byte
        {
            A = 24,
            B = 25,
            C = 29,
            D = 30,
        }

        [Test]
        public void CheckKeys_All_FromZero()
        {
            var lookup = new EnumBasedLookup<EnumFromZero, int>(
                (EnumFromZero.A, 1),
                (EnumFromZero.B, 2),
                (EnumFromZero.C, 3),
                (EnumFromZero.D, 4));

            var keys = lookup.Keys;
            CollectionAssert.IsNotEmpty(keys, "Keys collection is empty.");
            CollectionAssert.AreEqual(
                new[] { EnumFromZero.A, EnumFromZero.B, EnumFromZero.C, EnumFromZero.D },
                keys,
                "Keys collection is invalid.");
        }

        [Test]
        public void CheckKeys_Some_FromZero()
        {
            var lookup = new EnumBasedLookup<EnumFromZero, int>(
                (EnumFromZero.A, 1),
                (EnumFromZero.C, 3));

            var keys = lookup.Keys;
            CollectionAssert.IsNotEmpty(keys, "Keys collection is empty.");
            CollectionAssert.AreEqual(
                new[] { EnumFromZero.A, EnumFromZero.C },
                keys,
                "Keys collection is invalid.");
        }

        [Test]
        public void CheckValues_All_FromZero()
        {
            var lookup = new EnumBasedLookup<EnumFromZero, int>(
                (EnumFromZero.A, 1),
                (EnumFromZero.B, 2),
                (EnumFromZero.C, 3),
                (EnumFromZero.D, 4));

            var values = lookup.Values;
            CollectionAssert.IsNotEmpty(values, "Values collection is empty.");
            CollectionAssert.AreEqual(
                new[] { 1, 2, 3, 4 },
                values,
                "Values collection is invalid.");
        }

        [Test]
        public void CheckValues_Some_FromZero()
        {
            var lookup = new EnumBasedLookup<EnumFromZero, int>(
                (EnumFromZero.A, 1),
                (EnumFromZero.C, 3));

            var values = lookup.Values;
            CollectionAssert.IsNotEmpty(values, "Values collection is empty.");
            CollectionAssert.AreEqual(
                new[] { 1, 3 },
                values,
                "Values collection is invalid.");
        }

        [Test]
        public void GetValue_All_FromZero([Values] EnumFromZero key)
        {
            var lookup = new EnumBasedLookup<EnumFromZero, int>(
                (EnumFromZero.A, 1),
                (EnumFromZero.B, 2),
                (EnumFromZero.C, 3),
                (EnumFromZero.D, 4));

            var value = lookup[key];
            ClassicAssert.AreEqual(
                key switch
                {
                    EnumFromZero.A => 1,
                    EnumFromZero.B => 2,
                    EnumFromZero.C => 3,
                    EnumFromZero.D => 4,
                },
                value,
                "Value is invalid.");
        }

        [Test]
        public void GetValue_Some_FromZero([Values(EnumFromZero.A, EnumFromZero.C)] EnumFromZero key)
        {
            var lookup = new EnumBasedLookup<EnumFromZero, int>(
                (EnumFromZero.A, 1),
                (EnumFromZero.C, 3));

            var value = lookup[key];
            ClassicAssert.AreEqual(
                key switch
                {
                    EnumFromZero.A => 1,
                    EnumFromZero.C => 3,
                },
                value,
                "Value is invalid.");
        }

        [Test]
        public void TryGetValue_All_Valid_FromZero([Values] EnumFromZero key)
        {
            var lookup = new EnumBasedLookup<EnumFromZero, int>(
                (EnumFromZero.A, 1),
                (EnumFromZero.B, 2),
                (EnumFromZero.C, 3),
                (EnumFromZero.D, 4));

            var success = lookup.TryGetValue(key, out var value);
            ClassicAssert.IsTrue(success, "Value is not found.");
            ClassicAssert.AreEqual(
                key switch
                {
                    EnumFromZero.A => 1,
                    EnumFromZero.B => 2,
                    EnumFromZero.C => 3,
                    EnumFromZero.D => 4,
                },
                value,
                "Value is invalid.");
        }

        [Test]
        public void TryGetValue_All_Invalid_FromZero([Values((EnumFromZero)(-5), (EnumFromZero)10)] EnumFromZero key)
        {
            var lookup = new EnumBasedLookup<EnumFromZero, int>(
                (EnumFromZero.A, 1),
                (EnumFromZero.B, 2),
                (EnumFromZero.C, 3),
                (EnumFromZero.D, 4));

            var success = lookup.TryGetValue(key, out var value);
            ClassicAssert.IsFalse(success, "Value is found.");
        }

        [Test]
        public void TryGetValue_Some_Valid_FromZero([Values(EnumFromZero.A, EnumFromZero.C)] EnumFromZero key)
        {
            var lookup = new EnumBasedLookup<EnumFromZero, int>(
                (EnumFromZero.A, 1),
                (EnumFromZero.C, 3));

            var success = lookup.TryGetValue(key, out var value);
            ClassicAssert.IsTrue(success, "Value is not found.");
            ClassicAssert.AreEqual(
                key switch
                {
                    EnumFromZero.A => 1,
                    EnumFromZero.C => 3,
                },
                value,
                "Value is invalid.");
        }

        [Test]
        public void TryGetValue_Some_Invalid_FromZero([Values(EnumFromZero.B, EnumFromZero.D)] EnumFromZero key)
        {
            var lookup = new EnumBasedLookup<EnumFromZero, int>(
                (EnumFromZero.A, 1),
                (EnumFromZero.C, 3));

            var success = lookup.TryGetValue(key, out var value);
            ClassicAssert.IsFalse(success, "Value is found.");
        }

        [Test]
        public void CheckKeys_All_NotFromZero()
        {
            var lookup = new EnumBasedLookup<EnumNotFromZero, int>(
                (EnumNotFromZero.A, 24),
                (EnumNotFromZero.B, 25),
                (EnumNotFromZero.C, 29),
                (EnumNotFromZero.D, 30));

            var keys = lookup.Keys;
            CollectionAssert.IsNotEmpty(keys, "Keys collection is empty.");
            CollectionAssert.AreEqual(
                new[] { EnumNotFromZero.A, EnumNotFromZero.B, EnumNotFromZero.C, EnumNotFromZero.D },
                keys,
                "Keys collection is invalid.");
        }

        [Test]
        public void CheckKeys_Some_NotFromZero()
        {
            var lookup = new EnumBasedLookup<EnumNotFromZero, int>(
                (EnumNotFromZero.A, 24),
                (EnumNotFromZero.C, 29));

            var keys = lookup.Keys;
            CollectionAssert.IsNotEmpty(keys, "Keys collection is empty.");
            CollectionAssert.AreEqual(
                new[] { EnumNotFromZero.A, EnumNotFromZero.C },
                keys,
                "Keys collection is invalid.");
        }

        [Test]
        public void CheckValues_All_NotFromZero()
        {
            var lookup = new EnumBasedLookup<EnumNotFromZero, int>(
                (EnumNotFromZero.A, 24),
                (EnumNotFromZero.B, 25),
                (EnumNotFromZero.C, 29),
                (EnumNotFromZero.D, 30));

            var values = lookup.Values;
            CollectionAssert.IsNotEmpty(values, "Values collection is empty.");
            CollectionAssert.AreEqual(
                new[] { 24, 25, 29, 30 },
                values,
                "Values collection is invalid.");
        }

        [Test]
        public void CheckValues_Some_NotFromZero()
        {
            var lookup = new EnumBasedLookup<EnumNotFromZero, int>(
                (EnumNotFromZero.A, 24),
                (EnumNotFromZero.C, 29));

            var values = lookup.Values;
            CollectionAssert.IsNotEmpty(values, "Values collection is empty.");
            CollectionAssert.AreEqual(
                new[] { 24, 29 },
                values,
                "Values collection is invalid.");
        }

        [Test]
        public void GetValue_All_NotFromZero([Values] EnumNotFromZero key)
        {
            var lookup = new EnumBasedLookup<EnumNotFromZero, int>(
                (EnumNotFromZero.A, 24),
                (EnumNotFromZero.B, 25),
                (EnumNotFromZero.C, 29),
                (EnumNotFromZero.D, 30));

            var value = lookup[key];
            ClassicAssert.AreEqual(
                key switch
                {
                    EnumNotFromZero.A => 24,
                    EnumNotFromZero.B => 25,
                    EnumNotFromZero.C => 29,
                    EnumNotFromZero.D => 30,
                },
                value,
                "Value is invalid.");
        }

        [Test]
        public void GetValue_Some_NotFromZero([Values(EnumNotFromZero.A, EnumNotFromZero.C)] EnumNotFromZero key)
        {
            var lookup = new EnumBasedLookup<EnumNotFromZero, int>(
                (EnumNotFromZero.A, 24),
                (EnumNotFromZero.C, 29));

            var value = lookup[key];
            ClassicAssert.AreEqual(
                key switch
                {
                    EnumNotFromZero.A => 24,
                    EnumNotFromZero.C => 29,
                },
                value,
                "Value is invalid.");
        }

        [Test]
        public void TryGetValue_All_Valid_NotFromZero([Values] EnumNotFromZero key)
        {
            var lookup = new EnumBasedLookup<EnumNotFromZero, int>(
                (EnumNotFromZero.A, 24),
                (EnumNotFromZero.B, 25),
                (EnumNotFromZero.C, 29),
                (EnumNotFromZero.D, 30));

            var success = lookup.TryGetValue(key, out var value);
            ClassicAssert.IsTrue(success, "Value is not found.");
            ClassicAssert.AreEqual(
                key switch
                {
                    EnumNotFromZero.A => 24,
                    EnumNotFromZero.B => 25,
                    EnumNotFromZero.C => 29,
                    EnumNotFromZero.D => 30,
                },
                value,
                "Value is invalid.");
        }

        [Test]
        public void TryGetValue_All_Invalid_NotFromZero([Values((EnumNotFromZero)100, (EnumNotFromZero)10)] EnumNotFromZero key)
        {
            var lookup = new EnumBasedLookup<EnumNotFromZero, int>(
                (EnumNotFromZero.A, 24),
                (EnumNotFromZero.B, 25),
                (EnumNotFromZero.C, 29),
                (EnumNotFromZero.D, 30));

            var success = lookup.TryGetValue(key, out var value);
            ClassicAssert.IsFalse(success, "Value is found.");
        }

        [Test]
        public void TryGetValue_Some_Valid_NotFromZero([Values(EnumNotFromZero.A, EnumNotFromZero.C)] EnumNotFromZero key)
        {
            var lookup = new EnumBasedLookup<EnumNotFromZero, int>(
                (EnumNotFromZero.A, 24),
                (EnumNotFromZero.C, 29));

            var success = lookup.TryGetValue(key, out var value);
            ClassicAssert.IsTrue(success, "Value is not found.");
            ClassicAssert.AreEqual(
                key switch
                {
                    EnumNotFromZero.A => 24,
                    EnumNotFromZero.C => 29,
                },
                value,
                "Value is invalid.");
        }

        [Test]
        public void TryGetValue_Some_Invalid_NotFromZero([Values(EnumNotFromZero.B, EnumNotFromZero.D)] EnumNotFromZero key)
        {
            var lookup = new EnumBasedLookup<EnumNotFromZero, int>(
                (EnumNotFromZero.A, 24),
                (EnumNotFromZero.C, 29));

            var success = lookup.TryGetValue(key, out var value);
            ClassicAssert.IsFalse(success, "Value is found.");
        }
    }
}
