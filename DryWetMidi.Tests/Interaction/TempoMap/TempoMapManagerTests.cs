using System.Linq;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public class TempoMapManagerTests
    {
        [Test]
        [Description("Manage new tempo map without specified time division.")]
        public void Manage_New_WithoutTimeDivision()
        {
            using (var tempoMapManager = new TempoMapManager())
            {
                var tempoMap = tempoMapManager.TempoMap;

                ClassicAssert.AreEqual(TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote,
                                ((TicksPerQuarterNoteTimeDivision)tempoMap.TimeDivision).TicksPerQuarterNote);
                ClassicAssert.IsFalse(tempoMap.GetTempoChanges().Any());
                ClassicAssert.IsFalse(tempoMap.GetTimeSignatureChanges().Any());
            }
        }

        [Test]
        [Description("Manage new tempo map with the specified time division.")]
        public void Manage_New_WithTimeDivision()
        {
            using (var tempoMapManager = new TempoMapManager(new TicksPerQuarterNoteTimeDivision(100)))
            {
                var tempoMap = tempoMapManager.TempoMap;

                ClassicAssert.AreEqual(100, ((TicksPerQuarterNoteTimeDivision)tempoMap.TimeDivision).TicksPerQuarterNote);
                ClassicAssert.IsFalse(tempoMap.GetTempoChanges().Any());
                ClassicAssert.IsFalse(tempoMap.GetTimeSignatureChanges().Any());
            }
        }

        [Test]
        [Description("Replace a tempo map with the default one.")]
        public void ReplaceTempoMap_ByDefault()
        {
            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTempo(100, new Tempo(10));
                tempoMapManager.SetTempo(300, new Tempo(100));
                tempoMapManager.SetTimeSignature(1100, new TimeSignature(2, 4));

                var tempoMap = tempoMapManager.TempoMap;

                ClassicAssert.IsTrue(tempoMap.GetTempoChanges().Any(), "There are no tempo changes initially.");
                ClassicAssert.IsTrue(tempoMap.GetTimeSignatureChanges().Any(), "There are no time signature changes initially");

                tempoMapManager.ReplaceTempoMap(TempoMap.Default);

                ClassicAssert.AreEqual
                    (TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote,
                    ((TicksPerQuarterNoteTimeDivision)tempoMap.TimeDivision).TicksPerQuarterNote,
                    "Invalid TPQN after replacing.");
                ClassicAssert.IsFalse(tempoMap.GetTempoChanges().Any(), "There are tempo changes after replacing.");
                ClassicAssert.IsFalse(tempoMap.GetTimeSignatureChanges().Any(), "There are time signature changes after replacing.");
            }
        }
    }
}
