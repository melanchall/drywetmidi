using Melanchall.DryWetMidi.Core;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    internal static class MidiFileEquality
    {
        #region Methods

        public static void AssertAreEqual(MidiFile file1, MidiFile file2, bool compareOriginalFormat, string additionalMessage = null)
        {
            if (ReferenceEquals(file1, file2))
                return;

            if (ReferenceEquals(null, file1) || ReferenceEquals(null, file2))
                Assert.Fail($"One of file is null.{additionalMessage}");

            if (compareOriginalFormat)
                Assert.AreEqual(file1.OriginalFormat, file2.OriginalFormat, $"Original format is invalid.{additionalMessage}");

            Assert.AreEqual(file1.TimeDivision, file2.TimeDivision, $"Time division is invalid.{additionalMessage}");

            var chunks1 = file1.Chunks;
            var chunks2 = file2.Chunks;

            Assert.AreEqual(chunks1.Count, chunks2.Count, $"Chunks count is invalid.{additionalMessage}");

            for (var i = 0; i < chunks1.Count; i++)
            {
                var c1 = chunks1[i];
                var c2 = chunks2[i];

                Assert.AreEqual(c1.GetType(), c2.GetType(), $"Type of chunk {i} is invalid.{additionalMessage}");

                var tc1 = c1 as TrackChunk;
                if (tc1 != null)
                {
                    var tc2 = c2 as TrackChunk;

                    var events1 = tc1.Events;
                    var events2 = tc2.Events;

                    Assert.AreEqual(events1.Count, events2.Count, $"Events count is invalid for chunk {i}.{additionalMessage}");

                    for (var j = 0; j < events1.Count; j++)
                    {
                        var e1 = events1[j];
                        var e2 = events2[j];
                        Assert.IsTrue(MidiEventEquality.AreEqual(e1, e2, true), $"Event {j} in chunk {i} is invalid.{additionalMessage}");
                    }

                    continue;
                }

                var uc1 = c1 as UnknownChunk;
                if (uc1 != null)
                {
                    var uc2 = c2 as UnknownChunk;
                    CollectionAssert.AreEqual(uc1.Data, uc2.Data, $"Data in chunk {i} is invalid.{additionalMessage}");
                }
            }
        }

        #endregion
    }
}
