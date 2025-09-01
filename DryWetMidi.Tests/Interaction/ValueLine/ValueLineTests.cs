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

        //   ↑
        //   D
        [Test]
        public void SetValue_1_D([Values(0, 10)] long time) => SetValue(
            initialValueChanges: Array.Empty<(long Time, string Value)>(),
            newTime: time,
            newValue: "D",
            expectedValueChanges: Array.Empty<(long Time, string Value)>(),
            expectedResult: false);

        //   ↑
        //   A
        [Test]
        public void SetValue_1_A([Values(0, 10)] long time) => SetValue(
            initialValueChanges: Array.Empty<(long Time, string Value)>(),
            newTime: time,
            newValue: "A",
            expectedValueChanges: new[] { (time, "A") },
            expectedResult: true);

        //  |  ↑
        //  A  D
        [Test]
        public void SetValue_2_D([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A") },
            newTime: initialTime + 10,
            newValue: "D",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "D") },
            expectedResult: true);

        //  |  ↑
        //  A  A
        [Test]
        public void SetValue_2_A([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A") },
            newTime: initialTime + 10,
            newValue: "A",
            expectedValueChanges: new[] { (initialTime, "A") },
            expectedResult: false);

        //  |  ↑
        //  A  B
        [Test]
        public void SetValue_2_B([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A") },
            newTime: initialTime + 10,
            newValue: "B",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B") },
            expectedResult: true);

        //  |
        //  A
        //  ↑
        //  D
        [Test]
        public void SetValue_3_D([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A") },
            newTime: initialTime,
            newValue: "D",
            expectedValueChanges: Array.Empty<(long Time, string Value)>(),
            expectedResult: true);

        //  |
        //  A
        //  ↑
        //  A
        [Test]
        public void SetValue_3_A([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A") },
            newTime: initialTime,
            newValue: "A",
            expectedValueChanges: new[] { (initialTime, "A") },
            expectedResult: false);

        //  |
        //  A
        //  ↑
        //  B
        [Test]
        public void SetValue_3_B([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A") },
            newTime: initialTime,
            newValue: "B",
            expectedValueChanges: new[] { (initialTime, "B") },
            expectedResult: true);

        //  ↑  |
        //  D  A
        [Test]
        public void SetValue_4_D([Values(0, 10)] long newTime) => SetValue(
            initialValueChanges: new[] { (100L, "A") },
            newTime: newTime,
            newValue: "D",
            expectedValueChanges: new[] { (100L, "A") },
            expectedResult: false);

        //  ↑  |
        //  A  A
        [Test]
        public void SetValue_4_A([Values(0, 10)] long newTime) => SetValue(
            initialValueChanges: new[] { (100L, "A") },
            newTime: newTime,
            newValue: "A",
            expectedValueChanges: new[] { (newTime, "A") },
            expectedResult: true);

        //  ↑  |
        //  B  A
        [Test]
        public void SetValue_4_B([Values(0, 10)] long newTime) => SetValue(
            initialValueChanges: new[] { (100L, "A") },
            newTime: newTime,
            newValue: "B",
            expectedValueChanges: new[] { (newTime, "B"), (100L, "A") },
            expectedResult: true);

        //  |  |  ↑
        //  A  B  D
        [Test]
        public void SetValue_5_D([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B") },
            newTime: initialTime + 20,
            newValue: "D",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "D") },
            expectedResult: true);

        //  |  |  ↑
        //  A  B  A
        [Test]
        public void SetValue_5_A([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B") },
            newTime: initialTime + 20,
            newValue: "A",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A") },
            expectedResult: true);

        //  |  |  ↑
        //  A  B  B
        [Test]
        public void SetValue_5_B([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B") },
            newTime: initialTime + 20,
            newValue: "B",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B") },
            expectedResult: false);

        //  |  |
        //  A  B
        //     ↑
        //     D
        [Test]
        public void SetValue_6_D([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B") },
            newTime: initialTime + 10,
            newValue: "D",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "D") },
            expectedResult: true);

        //  |  |
        //  A  B
        //     ↑
        //     A
        [Test]
        public void SetValue_6_A([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B") },
            newTime: initialTime + 10,
            newValue: "A",
            expectedValueChanges: new[] { (initialTime, "A") },
            expectedResult: true);

        //  |  |
        //  A  B
        //     ↑
        //     B
        [Test]
        public void SetValue_6_B([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B") },
            newTime: initialTime + 10,
            newValue: "B",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B") },
            expectedResult: false);

        //  |   |
        //  A   B
        //    ↑
        //    D
        [Test]
        public void SetValue_7_D([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B") },
            newTime: initialTime + 5,
            newValue: "D",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 5, "D"), (initialTime + 10, "B") },
            expectedResult: true);

        //  |   |
        //  A   B
        //    ↑
        //    A
        [Test]
        public void SetValue_7_A([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B") },
            newTime: initialTime + 5,
            newValue: "A",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B") },
            expectedResult: false);

        //  |   |
        //  A   B
        //    ↑
        //    B
        [Test]
        public void SetValue_7_B([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B") },
            newTime: initialTime + 5,
            newValue: "B",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 5, "B") },
            expectedResult: true);

        //  |  |
        //  A  B
        //  ↑
        //  D
        [Test]
        public void SetValue_8_D([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B") },
            newTime: initialTime,
            newValue: "D",
            expectedValueChanges: new[] { (initialTime + 10, "B") },
            expectedResult: true);

        //  |  |
        //  A  B
        //  ↑
        //  A
        [Test]
        public void SetValue_8_A([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B") },
            newTime: initialTime,
            newValue: "A",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B") },
            expectedResult: false);

        //  |  |
        //  A  B
        //  ↑
        //  B
        [Test]
        public void SetValue_8_B([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B") },
            newTime: initialTime,
            newValue: "B",
            expectedValueChanges: new[] { (initialTime, "B") },
            expectedResult: true);

        //    |  |
        //    A  B
        //  ↑
        //  D
        [Test]
        public void SetValue_9_D([Values(0, 5)] long newTime) => SetValue(
            initialValueChanges: new[] { (10L, "A"), (100L, "B") },
            newTime: newTime,
            newValue: "D",
            expectedValueChanges: new[] { (10L, "A"), (100L, "B") },
            expectedResult: false);

        //    |  |
        //    A  B
        //  ↑
        //  A
        [Test]
        public void SetValue_9_A([Values(0, 5)] long newTime) => SetValue(
            initialValueChanges: new[] { (10L, "A"), (100L, "B") },
            newTime: newTime,
            newValue: "A",
            expectedValueChanges: new[] { (newTime, "A"), (100L, "B") },
            expectedResult: true);

        //    |  |
        //    A  B
        //  ↑
        //  B
        [Test]
        public void SetValue_9_B([Values(0, 5)] long newTime) => SetValue(
            initialValueChanges: new[] { (10L, "A"), (100L, "B") },
            newTime: newTime,
            newValue: "B",
            expectedValueChanges: new[] { (newTime, "B"), (10L, "A"), (100L, "B") },
            expectedResult: true);

        //  |  |  |  ↑
        //  A  B  A  D
        [Test]
        public void SetValue_10_D([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A") },
            newTime: initialTime + 30,
            newValue: "D",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "D") },
            expectedResult: true);

        //  |  |  |  ↑
        //  A  B  A  A
        [Test]
        public void SetValue_10_A([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A") },
            newTime: initialTime + 30,
            newValue: "A",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A") },
            expectedResult: false);

        //  |  |  |  ↑
        //  A  B  A  B
        [Test]
        public void SetValue_10_B([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A") },
            newTime: initialTime + 30,
            newValue: "B",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            expectedResult: true);

        //  |  |  |
        //  A  B  A
        //        ↑
        //        D
        [Test]
        public void SetValue_11_D([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A") },
            newTime: initialTime + 20,
            newValue: "D",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "D") },
            expectedResult: true);

        //  |  |  |
        //  A  B  A
        //        ↑
        //        A
        [Test]
        public void SetValue_11_A([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A") },
            newTime: initialTime + 20,
            newValue: "A",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A") },
            expectedResult: false);

        //  |  |  |
        //  A  B  A
        //        ↑
        //        B
        [Test]
        public void SetValue_11_B([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A") },
            newTime: initialTime + 20,
            newValue: "B",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B") },
            expectedResult: true);

        //  |  |   |
        //  A  B   A
        //       ↑
        //       D
        [Test]
        public void SetValue_12_D([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A") },
            newTime: initialTime + 15,
            newValue: "D",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 15, "D"), (initialTime + 20, "A") },
            expectedResult: true);

        //  |  |   |
        //  A  B   A
        //       ↑
        //       A
        [Test]
        public void SetValue_12_A([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A") },
            newTime: initialTime + 15,
            newValue: "A",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 15, "A") },
            expectedResult: true);

        //  |  |   |
        //  A  B   A
        //       ↑
        //       B
        [Test]
        public void SetValue_12_B([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A") },
            newTime: initialTime + 15,
            newValue: "B",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A") },
            expectedResult: false);

        //  |  |  |
        //  A  B  A
        //     ↑
        //     D
        [Test]
        public void SetValue_13_D([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A") },
            newTime: initialTime + 10,
            newValue: "D",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "D"), (initialTime + 20, "A") },
            expectedResult: true);

        //  |  |  |
        //  A  B  A
        //     ↑
        //     A
        [Test]
        public void SetValue_13_A([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A") },
            newTime: initialTime + 10,
            newValue: "A",
            expectedValueChanges: new[] { (initialTime, "A") },
            expectedResult: true);

        //  |  |  |
        //  A  B  A
        //     ↑
        //     B
        [Test]
        public void SetValue_13_B([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A") },
            newTime: initialTime + 10,
            newValue: "B",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A") },
            expectedResult: false);

        //  |   |  |
        //  A   B  A
        //    ↑
        //    D
        [Test]
        public void SetValue_14_D([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A") },
            newTime: initialTime + 5,
            newValue: "D",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 5, "D"),(initialTime + 10, "B"), (initialTime + 20, "A") },
            expectedResult: true);

        //  |   |  |
        //  A   B  A
        //    ↑
        //    A
        [Test]
        public void SetValue_14_A([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A") },
            newTime: initialTime + 5,
            newValue: "A",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A") },
            expectedResult: false);

        //  |   |  |
        //  A   B  A
        //    ↑
        //    B
        [Test]
        public void SetValue_14_B([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A") },
            newTime: initialTime + 5,
            newValue: "B",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 5, "B"), (initialTime + 20, "A") },
            expectedResult: true);

        //  |  |  |
        //  A  B  A
        //  ↑
        //  D
        [Test]
        public void SetValue_15_D([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A") },
            newTime: initialTime,
            newValue: "D",
            expectedValueChanges: new[] { (initialTime + 10, "B"), (initialTime + 20, "A") },
            expectedResult: true);

        //  |  |  |
        //  A  B  A
        //  ↑
        //  A
        [Test]
        public void SetValue_15_A([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A") },
            newTime: initialTime,
            newValue: "A",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A") },
            expectedResult: false);

        //  |  |  |
        //  A  B  A
        //  ↑
        //  B
        [Test]
        public void SetValue_15_B([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A") },
            newTime: initialTime,
            newValue: "B",
            expectedValueChanges: new[] { (initialTime, "B"), (initialTime + 20, "A") },
            expectedResult: true);

        //    |  |  |
        //    A  B  A
        //  ↑
        //  D
        [Test]
        public void SetValue_16_D([Values(0, 5)] long newTime) => SetValue(
            initialValueChanges: new[] { (10L, "A"), (100L, "B"), (1000L, "A") },
            newTime: newTime,
            newValue: "D",
            expectedValueChanges: new[] { (10L, "A"), (100L, "B"), (1000L, "A") },
            expectedResult: false);

        //    |  |  |
        //    A  B  A
        //  ↑
        //  A
        [Test]
        public void SetValue_16_A([Values(0, 5)] long newTime) => SetValue(
            initialValueChanges: new[] { (10L, "A"), (100L, "B"), (1000L, "A") },
            newTime: newTime,
            newValue: "A",
            expectedValueChanges: new[] { (newTime, "A"), (100L, "B"), (1000L, "A") },
            expectedResult: true);

        //    |  |  |
        //    A  B  A
        //  ↑
        //  B
        [Test]
        public void SetValue_16_B([Values(0, 5)] long newTime) => SetValue(
            initialValueChanges: new[] { (10L, "A"), (100L, "B"), (1000L, "A") },
            newTime: newTime,
            newValue: "B",
            expectedValueChanges: new[] { (newTime, "B"), (10L, "A"), (100L, "B"), (1000L, "A") },
            expectedResult: true);

        //  |  |  |  |  ↑
        //  A  B  A  B  D
        [Test]
        public void SetValue_17_D([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            newTime: initialTime + 40,
            newValue: "D",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B"), (initialTime + 40, "D") },
            expectedResult: true);

        //  |  |  |  |  ↑
        //  A  B  A  B  A
        [Test]
        public void SetValue_17_A([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            newTime: initialTime + 40,
            newValue: "A",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B"), (initialTime + 40, "A") },
            expectedResult: true);

        //  |  |  |  |  ↑
        //  A  B  A  B  B
        [Test]
        public void SetValue_17_B([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            newTime: initialTime + 40,
            newValue: "B",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            expectedResult: false);

        //  |  |  |  |
        //  A  B  A  B
        //           ↑
        //           D
        [Test]
        public void SetValue_18_D([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            newTime: initialTime + 30,
            newValue: "D",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "D") },
            expectedResult: true);

        //  |  |  |  |
        //  A  B  A  B
        //           ↑
        //           A
        [Test]
        public void SetValue_18_A([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            newTime: initialTime + 30,
            newValue: "A",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A") },
            expectedResult: true);

        //  |  |  |  |
        //  A  B  A  B
        //           ↑
        //           B
        [Test]
        public void SetValue_18_B([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            newTime: initialTime + 30,
            newValue: "B",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            expectedResult: false);

        //  |  |  |   |
        //  A  B  A   B
        //          ↑
        //          D
        [Test]
        public void SetValue_19_D([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            newTime: initialTime + 25,
            newValue: "D",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 25, "D"), (initialTime + 30, "B") },
            expectedResult: true);

        //  |  |  |   |
        //  A  B  A   B
        //          ↑
        //          A
        [Test]
        public void SetValue_19_A([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            newTime: initialTime + 25,
            newValue: "A",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            expectedResult: false);

        //  |  |  |   |
        //  A  B  A   B
        //          ↑
        //          B
        [Test]
        public void SetValue_19_B([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            newTime: initialTime + 25,
            newValue: "B",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 25, "B") },
            expectedResult: true);

        //  |  |  |  |
        //  A  B  A  B
        //        ↑
        //        D
        [Test]
        public void SetValue_20_D([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            newTime: initialTime + 20,
            newValue: "D",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "D"), (initialTime + 30, "B") },
            expectedResult: true);

        //  |  |  |  |
        //  A  B  A  B
        //        ↑
        //        A
        [Test]
        public void SetValue_20_A([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            newTime: initialTime + 20,
            newValue: "A",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            expectedResult: false);

        //  |  |  |  |
        //  A  B  A  B
        //        ↑
        //        B
        [Test]
        public void SetValue_20_B([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            newTime: initialTime + 20,
            newValue: "B",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B") },
            expectedResult: true);

        //  |  |   |  |
        //  A  B   A  B
        //       ↑
        //       D
        [Test]
        public void SetValue_21_D([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            newTime: initialTime + 15,
            newValue: "D",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 15, "D"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            expectedResult: true);

        //  |  |   |  |
        //  A  B   A  B
        //       ↑
        //       A
        [Test]
        public void SetValue_21_A([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            newTime: initialTime + 15,
            newValue: "A",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 15, "A"), (initialTime + 30, "B") },
            expectedResult: true);

        //  |  |   |  |
        //  A  B   A  B
        //       ↑
        //       B
        [Test]
        public void SetValue_21_B([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            newTime: initialTime + 15,
            newValue: "B",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            expectedResult: false);

        //  |  |  |  |
        //  A  B  A  B
        //     ↑
        //     D
        [Test]
        public void SetValue_22_D([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            newTime: initialTime + 10,
            newValue: "D",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "D"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            expectedResult: true);

        //  |  |  |  |
        //  A  B  A  B
        //     ↑
        //     A
        [Test]
        public void SetValue_22_A([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            newTime: initialTime + 10,
            newValue: "A",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 30, "B") },
            expectedResult: true);

        //  |  |  |  |
        //  A  B  A  B
        //     ↑
        //     B
        [Test]
        public void SetValue_22_B([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            newTime: initialTime + 10,
            newValue: "B",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            expectedResult: false);

        //  |   |  |  |
        //  A   B  A  B
        //    ↑
        //    D
        [Test]
        public void SetValue_23_D([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            newTime: initialTime + 5,
            newValue: "D",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 5, "D"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            expectedResult: true);

        //  |   |  |  |
        //  A   B  A  B
        //    ↑
        //    A
        [Test]
        public void SetValue_23_A([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            newTime: initialTime + 5,
            newValue: "A",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            expectedResult: false);

        //  |   |  |  |
        //  A   B  A  B
        //    ↑
        //    B
        [Test]
        public void SetValue_23_B([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            newTime: initialTime + 5,
            newValue: "B",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 5, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            expectedResult: true);

        //  |  |  |  |
        //  A  B  A  B
        //  ↑
        //  D
        [Test]
        public void SetValue_24_D([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            newTime: initialTime,
            newValue: "D",
            expectedValueChanges: new[] { (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            expectedResult: true);

        //  |  |  |  |
        //  A  B  A  B
        //  ↑
        //  A
        [Test]
        public void SetValue_24_A([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            newTime: initialTime,
            newValue: "A",
            expectedValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            expectedResult: false);

        //  |  |  |  |
        //  A  B  A  B
        //  ↑
        //  B
        [Test]
        public void SetValue_24_B([Values(0, 10)] long initialTime) => SetValue(
            initialValueChanges: new[] { (initialTime, "A"), (initialTime + 10, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            newTime: initialTime,
            newValue: "B",
            expectedValueChanges: new[] { (initialTime, "B"), (initialTime + 20, "A"), (initialTime + 30, "B") },
            expectedResult: true);

        //    |  |  |  |
        //    A  B  A  B
        //  ↑
        //  D
        [Test]
        public void SetValue_25_D([Values(0, 5)] long newTime) => SetValue(
            initialValueChanges: new[] { (10L, "A"), (100L, "B"), (1000L, "A"), (10000L, "B") },
            newTime: newTime,
            newValue: "D",
            expectedValueChanges: new[] { (10L, "A"), (100L, "B"), (1000L, "A"), (10000L, "B") },
            expectedResult: false);

        //    |  |  |  |
        //    A  B  A  B
        //  ↑
        //  A
        [Test]
        public void SetValue_25_A([Values(0, 5)] long newTime) => SetValue(
            initialValueChanges: new[] { (10L, "A"), (100L, "B"), (1000L, "A"), (10000L, "B") },
            newTime: newTime,
            newValue: "A",
            expectedValueChanges: new[] { (newTime, "A"), (100L, "B"), (1000L, "A"), (10000L, "B") },
            expectedResult: true);

        //    |  |  |  |
        //    A  B  A  B
        //  ↑
        //  B
        [Test]
        public void SetValue_25_B([Values(0, 5)] long newTime) => SetValue(
            initialValueChanges: new[] { (10L, "A"), (100L, "B"), (1000L, "A"), (10000L, "B") },
            newTime: newTime,
            newValue: "B",
            expectedValueChanges: new[] { (newTime, "B"), (10L, "A"), (100L, "B"), (1000L, "A"), (10000L, "B") },
            expectedResult: true);

        // └---┘
        [Test]
        public void DeleteValues_StartEnd_1([Values(0, 10)] long startTime) => DeleteValues_StartEnd(
            initialValueChanges: Array.Empty<(long Time, string Value)>(),
            startTime: startTime,
            endTime: startTime + 10,
            expectedValueChanges: Array.Empty<(long Time, string Value)>(),
            expectedResult: false);

        // └
        // ┘
        [Test]
        public void DeleteValues_StartEnd_2([Values(0, 10)] long startTime) => DeleteValues_StartEnd(
            initialValueChanges: Array.Empty<(long Time, string Value)>(),
            startTime: startTime,
            endTime: startTime,
            expectedValueChanges: Array.Empty<(long Time, string Value)>(),
            expectedResult: false);

        //       A
        //       |
        // └---┘
        [Test]
        public void DeleteValues_StartEnd_3([Values(0, 10)] long startTime) => DeleteValues_StartEnd(
            initialValueChanges: new[] { (startTime + 20, "A") },
            startTime: startTime,
            endTime: startTime + 10,
            expectedValueChanges: new[] { (startTime + 20, "A") },
            expectedResult: false);

        //     A
        //     |
        // └---┘
        [Test]
        public void DeleteValues_StartEnd_4([Values(0, 10)] long startTime) => DeleteValues_StartEnd(
            initialValueChanges: new[] { (100L, "A") },
            startTime: startTime,
            endTime: 100,
            expectedValueChanges: new[] { (100L, "A") },
            expectedResult: false);

        //   A
        //   |
        // └---┘
        [Test]
        public void DeleteValues_StartEnd_5([Values(0, 10)] long startTime) => DeleteValues_StartEnd(
            initialValueChanges: new[] { (100L, "A") },
            startTime: startTime,
            endTime: 1000,
            expectedValueChanges: Array.Empty<(long Time, string Value)>(),
            expectedResult: true);

        // A
        // |
        // └---┘
        [Test]
        public void DeleteValues_StartEnd_6([Values(0, 10)] long changeTime) => DeleteValues_StartEnd(
            initialValueChanges: new[] { (changeTime, "A") },
            startTime: changeTime,
            endTime: changeTime + 10,
            expectedValueChanges: Array.Empty<(long Time, string Value)>(),
            expectedResult: true);

        // A
        // |
        //   └---┘
        [Test]
        public void DeleteValues_StartEnd_7([Values(0, 10)] long changeTime) => DeleteValues_StartEnd(
            initialValueChanges: new[] { (changeTime, "A") },
            startTime: changeTime + 10,
            endTime: changeTime + 20,
            expectedValueChanges: new[] { (changeTime, "A") },
            expectedResult: false);

        //       A  B
        //       |  |
        // └---┘
        [Test]
        public void DeleteValues_StartEnd_8([Values(0, 10)] long startTime) => DeleteValues_StartEnd(
            initialValueChanges: new[] { (100L, "A"), (1000L, "B") },
            startTime: startTime,
            endTime: startTime + 10,
            expectedValueChanges: new[] { (100L, "A"), (1000L, "B") },
            expectedResult: false);

        //     A  B
        //     |  |
        // └---┘
        [Test]
        public void DeleteValues_StartEnd_9([Values(0, 10)] long startTime) => DeleteValues_StartEnd(
            initialValueChanges: new[] { (startTime + 10, "A"), (1000L, "B") },
            startTime: startTime,
            endTime: startTime + 10,
            expectedValueChanges: new[] { (startTime + 10, "A"), (1000L, "B") },
            expectedResult: false);

        //   A   B
        //   |   |
        // └---┘
        [Test]
        public void DeleteValues_StartEnd_10_B([Values(0, 10)] long startTime) => DeleteValues_StartEnd(
            initialValueChanges: new[] { (100L, "A"), (1000L, "B") },
            startTime: startTime,
            endTime: 110,
            expectedValueChanges: new[] { (1000L, "B") },
            expectedResult: true);

        //   A   D
        //   |   |
        // └---┘
        [Test]
        public void DeleteValues_StartEnd_10_D([Values(0, 10)] long startTime) => DeleteValues_StartEnd(
            initialValueChanges: new[] { (100L, "A"), (1000L, "D") },
            startTime: startTime,
            endTime: 110,
            expectedValueChanges: Array.Empty<(long Time, string Value)>(),
            expectedResult: true);

        //   A  B
        //   |  |
        // └----┘
        [Test]
        public void DeleteValues_StartEnd_11([Values(0, 10)] long startTime) => DeleteValues_StartEnd(
            initialValueChanges: new[] { (100L, "A"), (1000L, "B") },
            startTime: startTime,
            endTime: 1000L,
            expectedValueChanges: new[] { (1000L, "B") },
            expectedResult: true);

        // A   B
        // |   |
        // └---┘
        [Test]
        public void DeleteValues_StartEnd_12([Values(0, 10)] long firstChangeTime) => DeleteValues_StartEnd(
            initialValueChanges: new[] { (firstChangeTime, "A"), (firstChangeTime + 10, "B") },
            startTime: firstChangeTime,
            endTime: firstChangeTime + 10,
            expectedValueChanges: new[] { (firstChangeTime + 10, "B") },
            expectedResult: true);

        // A   B
        // |   |
        //   └---┘
        [Test]
        public void DeleteValues_StartEnd_13([Values(0, 10)] long firstChangeTime) => DeleteValues_StartEnd(
            initialValueChanges: new[] { (firstChangeTime, "A"), (firstChangeTime + 10, "B") },
            startTime: firstChangeTime + 5,
            endTime: firstChangeTime + 15,
            expectedValueChanges: new[] { (firstChangeTime, "A") },
            expectedResult: true);

        //   A B
        //   | |
        // └-----┘
        [Test]
        public void DeleteValues_StartEnd_14([Values(0, 10)] long startTime) => DeleteValues_StartEnd(
            initialValueChanges: new[] { (100L, "A"), (1000L, "B") },
            startTime: startTime,
            endTime: 2000,
            expectedValueChanges: Array.Empty<(long Time, string Value)>(),
            expectedResult: true);

        // A  B
        // |  |
        // └----┘
        [Test]
        public void DeleteValues_StartEnd_15([Values(0, 10)] long firstChangeTime) => DeleteValues_StartEnd(
            initialValueChanges: new[] { (firstChangeTime, "A"), (firstChangeTime + 10, "B") },
            startTime: firstChangeTime,
            endTime: firstChangeTime + 20,
            expectedValueChanges: Array.Empty<(long Time, string Value)>(),
            expectedResult: true);

        // A   B   C   E   A
        // |   |   |   |   |
        //   └-----------┘
        [Test]
        public void DeleteValues_StartEnd_16_A() => DeleteValues_StartEnd(
            initialValueChanges: new[] { (10L, "A"), (20L, "B"), (30L, "C"), (40L, "E"), (50L, "A") },
            startTime: 15,
            endTime: 45,
            expectedValueChanges: new[] { (10L, "A") },
            expectedResult: true);

        // A   B   C   E   B
        // |   |   |   |   |
        //   └-----------┘
        [Test]
        public void DeleteValues_StartEnd_16_B() => DeleteValues_StartEnd(
            initialValueChanges: new[] { (10L, "A"), (20L, "B"), (30L, "C"), (40L, "E"), (50L, "B") },
            startTime: 15,
            endTime: 45,
            expectedValueChanges: new[] { (10L, "A"), (50L, "B") },
            expectedResult: true);

        // A   B   C   A   B
        // |   |   |   |   |
        //     └-------┘
        [Test]
        public void DeleteValues_StartEnd_17_A() => DeleteValues_StartEnd(
            initialValueChanges: new[] { (10L, "A"), (20L, "B"), (30L, "C"), (40L, "A"), (50L, "B") },
            startTime: 20,
            endTime: 40,
            expectedValueChanges: new[] { (10L, "A"), (50L, "B") },
            expectedResult: true);

        // A   B   C   B   C
        // |   |   |   |   |
        //     └-------┘
        [Test]
        public void DeleteValues_StartEnd_17_B() => DeleteValues_StartEnd(
            initialValueChanges: new[] { (10L, "A"), (20L, "B"), (30L, "C"), (40L, "B"), (50L, "C") },
            startTime: 20,
            endTime: 40,
            expectedValueChanges: new[] { (10L, "A"), (40L, "B"), (50L, "C") },
            expectedResult: true);

        // A   B   C   B   C
        // |   |   |   |   |
        //       └---------┘
        [Test]
        public void DeleteValues_StartEnd_18() => DeleteValues_StartEnd(
            initialValueChanges: new[] { (10L, "A"), (20L, "B"), (30L, "C"), (40L, "B"), (50L, "C") },
            startTime: 25,
            endTime: 50,
            expectedValueChanges: new[] { (10L, "A"), (20L, "B"), (50L, "C") },
            expectedResult: true);

        // ↑
        [Test]
        public void DeleteValues_Start_1([Values(0, 10)] long startTime) => DeleteValues_Start(
            initialValueChanges: Array.Empty<(long Time, string Value)>(),
            startTime: startTime,
            expectedValueChanges: Array.Empty<(long Time, string Value)>(),
            expectedResult: false);

        //   A
        //   |
        // ↑
        [Test]
        public void DeleteValues_Start_2([Values(0, 10)] long startTime) => DeleteValues_Start(
            initialValueChanges: new[] { (100L, "A") },
            startTime: startTime,
            expectedValueChanges: Array.Empty<(long Time, string Value)>(),
            expectedResult: true);

        // A
        // |
        // ↑
        [Test]
        public void DeleteValues_Start_3([Values(0, 10)] long changeTime) => DeleteValues_Start(
            initialValueChanges: new[] { (changeTime, "A") },
            startTime: changeTime,
            expectedValueChanges: Array.Empty<(long Time, string Value)>(),
            expectedResult: true);

        // A
        // |
        //   ↑
        [Test]
        public void DeleteValues_Start_4([Values(0, 10)] long changeTime) => DeleteValues_Start(
            initialValueChanges: new[] { (changeTime, "A") },
            startTime: changeTime + 10,
            expectedValueChanges: new[] { (changeTime, "A") },
            expectedResult: false);

        //   A  B
        //   |  |
        // ↑
        [Test]
        public void DeleteValues_Start_5([Values(0, 10)] long startTime) => DeleteValues_Start(
            initialValueChanges: new[] { (100L, "A"), (1000L, "B") },
            startTime: startTime,
            expectedValueChanges: Array.Empty<(long Time, string Value)>(),
            expectedResult: true);

        // A  B
        // |  |
        // ↑
        [Test]
        public void DeleteValues_Start_6([Values(0, 10)] long firstChangeTime) => DeleteValues_Start(
            initialValueChanges: new[] { (firstChangeTime, "A"), (firstChangeTime + 10, "B") },
            startTime: firstChangeTime,
            expectedValueChanges: Array.Empty<(long Time, string Value)>(),
            expectedResult: true);

        // A   B
        // |   |
        //   ↑
        [Test]
        public void DeleteValues_Start_7([Values(0, 10)] long firstChangeTime) => DeleteValues_Start(
            initialValueChanges: new[] { (firstChangeTime, "A"), (firstChangeTime + 10, "B") },
            startTime: firstChangeTime + 5,
            expectedValueChanges: new[] { (firstChangeTime, "A") },
            expectedResult: true);

        // A  B
        // |  |
        //    ↑
        [Test]
        public void DeleteValues_Start_8([Values(0, 10)] long firstChangeTime) => DeleteValues_Start(
            initialValueChanges: new[] { (firstChangeTime, "A"), (firstChangeTime + 10, "B") },
            startTime: firstChangeTime + 10,
            expectedValueChanges: new[] { (firstChangeTime, "A") },
            expectedResult: true);

        // A  B
        // |  |
        //      ↑
        [Test]
        public void DeleteValues_Start_9([Values(0, 10)] long firstChangeTime) => DeleteValues_Start(
            initialValueChanges: new[] { (firstChangeTime, "A"), (firstChangeTime + 10, "B") },
            startTime: firstChangeTime + 20,
            expectedValueChanges: new[] { (firstChangeTime, "A"), (firstChangeTime + 10, "B") },
            expectedResult: false);

        // A  B  C  A
        // |  |  |  |
        //    ↑
        [Test]
        public void DeleteValues_Start_10([Values(0, 10)] long firstChangeTime) => DeleteValues_Start(
            initialValueChanges: new[] { (firstChangeTime, "A"), (firstChangeTime + 10, "B"), (firstChangeTime + 20, "C"), (firstChangeTime + 30, "A") },
            startTime: firstChangeTime + 10,
            expectedValueChanges: new[] { (firstChangeTime, "A") },
            expectedResult: true);

        // A   B  C  A
        // |   |  |  |
        //   ↑
        [Test]
        public void DeleteValues_Start_11([Values(0, 10)] long firstChangeTime) => DeleteValues_Start(
            initialValueChanges: new[] { (firstChangeTime, "A"), (firstChangeTime + 10, "B"), (firstChangeTime + 20, "C"), (firstChangeTime + 30, "A") },
            startTime: firstChangeTime + 5,
            expectedValueChanges: new[] { (firstChangeTime, "A") },
            expectedResult: true);

        [Test]
        public void Construct()
        {
            const string defaultValue = "A";

            var valueLine = new ValueLine<string>(defaultValue);

            CollectionAssert.IsEmpty(valueLine, "Value line contains changes.");
            ClassicAssert.AreEqual(defaultValue, valueLine.GetValueAtTime(0), "Invalid value at the start.");
            ClassicAssert.AreEqual(defaultValue, valueLine.GetValueAtTime(100), "Invalid value at the middle.");
        }

        // ↑
        [Test]
        public void GetValueAtTime_1([Values(0, 10)] long time) => GetValueAtTime(
            initialValueChanges: Array.Empty<(long Time, string Value)>(),
            time: time,
            expectedValue: "D");

        //   A
        //   |
        // ↑
        [Test]
        public void GetValueAtTime_2([Values(0, 10)] long time) => GetValueAtTime(
            initialValueChanges: new[] { (100L, "A") },
            time: time,
            expectedValue: "D");

        // A
        // |
        // ↑
        [Test]
        public void GetValueAtTime_3([Values(0, 10)] long changeTime) => GetValueAtTime(
            initialValueChanges: new[] { (changeTime, "A") },
            time: changeTime,
            expectedValue: "A");

        // A
        // |
        //   ↑
        [Test]
        public void GetValueAtTime_4([Values(0, 10)] long changeTime) => GetValueAtTime(
            initialValueChanges: new[] { (changeTime, "A") },
            time: changeTime + 10,
            expectedValue: "A");

        //   A  B
        //   |  |
        // ↑
        [Test]
        public void GetValueAtTime_5([Values(0, 10)] long time) => GetValueAtTime(
            initialValueChanges: new[] { (100L, "A"), (1000L, "B") },
            time: time,
            expectedValue: "D");

        // A  B
        // |  |
        // ↑
        [Test]
        public void GetValueAtTime_6([Values(0, 10)] long firstChangeTime) => GetValueAtTime(
            initialValueChanges: new[] { (firstChangeTime, "A"), (firstChangeTime + 10, "B") },
            time: firstChangeTime,
            expectedValue: "A");

        // A   B
        // |   |
        //   ↑
        [Test]
        public void GetValueAtTime_7([Values(0, 10)] long firstChangeTime) => GetValueAtTime(
            initialValueChanges: new[] { (firstChangeTime, "A"), (firstChangeTime + 10, "B") },
            time: firstChangeTime + 5,
            expectedValue: "A");

        // A  B
        // |  |
        //    ↑
        [Test]
        public void GetValueAtTime_8([Values(0, 10)] long firstChangeTime) => GetValueAtTime(
            initialValueChanges: new[] { (firstChangeTime, "A"), (firstChangeTime + 10, "B") },
            time: firstChangeTime + 10,
            expectedValue: "B");

        // A  B
        // |  |
        //      ↑
        [Test]
        public void GetValueAtTime_9([Values(0, 10)] long firstChangeTime) => GetValueAtTime(
            initialValueChanges: new[] { (firstChangeTime, "A"), (firstChangeTime + 10, "B") },
            time: firstChangeTime + 20,
            expectedValue: "B");

        //   A  B  C
        //   |  |  |
        // ↑
        [Test]
        public void GetValueAtTime_10([Values(0, 10)] long time) => GetValueAtTime(
            initialValueChanges: new[] { (100L, "A"), (1000L, "B"), (10000L, "C") },
            time: time,
            expectedValue: "D");

        // A  B  C
        // |  |  |
        // ↑
        [Test]
        public void GetValueAtTime_11([Values(0, 10)] long firstChangeTime) => GetValueAtTime(
            initialValueChanges: new[] { (firstChangeTime, "A"), (firstChangeTime + 10, "B"), (firstChangeTime + 20, "C") },
            time: firstChangeTime,
            expectedValue: "A");

        // A   B  C
        // |   |  |
        //   ↑
        [Test]
        public void GetValueAtTime_12([Values(0, 10)] long firstChangeTime) => GetValueAtTime(
            initialValueChanges: new[] { (firstChangeTime, "A"), (firstChangeTime + 10, "B"), (firstChangeTime + 20, "C") },
            time: firstChangeTime + 5,
            expectedValue: "A");

        // A  B  C
        // |  |  |
        //    ↑
        [Test]
        public void GetValueAtTime_13([Values(0, 10)] long firstChangeTime) => GetValueAtTime(
            initialValueChanges: new[] { (firstChangeTime, "A"), (firstChangeTime + 10, "B"), (firstChangeTime + 20, "C") },
            time: firstChangeTime + 10,
            expectedValue: "B");

        // A  B   C
        // |  |   |
        //      ↑
        [Test]
        public void GetValueAtTime_14([Values(0, 10)] long firstChangeTime) => GetValueAtTime(
            initialValueChanges: new[] { (firstChangeTime, "A"), (firstChangeTime + 10, "B"), (firstChangeTime + 20, "C") },
            time: firstChangeTime + 15,
            expectedValue: "B");

        // A  B  C
        // |  |  |
        //       ↑
        [Test]
        public void GetValueAtTime_15([Values(0, 10)] long firstChangeTime) => GetValueAtTime(
            initialValueChanges: new[] { (firstChangeTime, "A"), (firstChangeTime + 10, "B"), (firstChangeTime + 20, "C") },
            time: firstChangeTime + 20,
            expectedValue: "C");

        // A  B  C
        // |  |  |
        //         ↑
        [Test]
        public void GetValueAtTime_16([Values(0, 10)] long firstChangeTime) => GetValueAtTime(
            initialValueChanges: new[] { (firstChangeTime, "A"), (firstChangeTime + 10, "B"), (firstChangeTime + 20, "C") },
            time: firstChangeTime + 30,
            expectedValue: "C");

        #endregion

        #region Private methods

        private void GetValueAtTime(
            ICollection<(long Time, string Value)> initialValueChanges,
            long time,
            string expectedValue)
        {
            var valueLine = new ValueLine<string>("D");

            foreach (var (initialTime, initialValue) in initialValueChanges)
            {
                valueLine.SetValue(initialTime, initialValue);
            }

            CollectionAssert.AreEqual(
                initialValueChanges.Select(c => new ValueChange<string>(c.Time, c.Value)),
                valueLine,
                "Invalid initial value changes.");

            ClassicAssert.AreEqual(
                expectedValue,
                valueLine.GetValueAtTime(time),
                "Invalid value at the time.");
        }

        private void DeleteValues_Start(
            ICollection<(long Time, string Value)> initialValueChanges,
            long startTime,
            ICollection<(long Time, string Value)> expectedValueChanges,
            bool expectedResult) =>
            ModifyValueLine(
                initialValueChanges,
                valueLine => ClassicAssert.AreEqual(
                    expectedResult,
                    valueLine.DeleteValues(startTime),
                    "Invalid result."),
                expectedValueChanges);

        private void DeleteValues_StartEnd(
            ICollection<(long Time, string Value)> initialValueChanges,
            long startTime,
            long endTime,
            ICollection<(long Time, string Value)> expectedValueChanges,
            bool expectedResult) =>
            ModifyValueLine(
                initialValueChanges,
                valueLine => ClassicAssert.AreEqual(
                    expectedResult,
                    valueLine.DeleteValues(startTime, endTime),
                    "Invalid result."),
                expectedValueChanges);

        private void SetValue(
            ICollection<(long Time, string Value)> initialValueChanges,
            long newTime,
            string newValue,
            ICollection<(long Time, string Value)> expectedValueChanges,
            bool expectedResult) =>
            ModifyValueLine(
                initialValueChanges,
                valueLine => ClassicAssert.AreEqual(
                    expectedResult,
                    valueLine.SetValue(newTime, newValue),
                    "Invalid result."),
                expectedValueChanges);

        private void ModifyValueLine(
            ICollection<(long Time, string Value)> initialValueChanges,
            Action<ValueLine<string>> modify,
            ICollection<(long Time, string Value)> expectedValueChanges)
        {
            var valueLine = new ValueLine<string>("D");

            foreach (var (initialTime, initialValue) in initialValueChanges)
            {
                valueLine.SetValue(initialTime, initialValue);
            }

            CollectionAssert.AreEqual(
                initialValueChanges.Select(c => new ValueChange<string>(c.Time, c.Value)),
                valueLine,
                "Invalid initial value changes.");

            modify(valueLine);

            CollectionAssert.AreEqual(
                expectedValueChanges.Select(c => new ValueChange<string>(c.Time, c.Value)),
                valueLine,
                "Invalid value changes after modofication.");
        }

        #endregion
    }
}
