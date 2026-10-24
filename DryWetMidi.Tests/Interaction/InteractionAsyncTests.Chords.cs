using System.Linq;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    public sealed partial class InteractionAsyncTests
    {
        [Test]
        public async Task ProcessChords_AwaitedCollectionChangesPersist()
        {
            var track = CreateTrack();

            var count = await track.ProcessChordsAsync(async (chord, token) =>
            {
                await Task.Yield();
                token.ThrowIfCancellationRequested();
                chord.Notes.Remove(chord.Notes.First());
                chord.Notes.Add(new Note((SevenBitNumber)67, 60, 30));
            }, match: chord => chord.Notes.Count > 1, hint: ChordProcessingHint.AllPropertiesCanBeChanged);

            ClassicAssert.AreEqual(1, count, "Invalid processed chord count.");

            var notes = track.GetNotes();

            ClassicAssert.AreEqual(3, notes.Count, "Chord processing produced an invalid note count.");
            ClassicAssert.IsTrue(notes.Any(n => n.NoteNumber == (SevenBitNumber)67 && n.Time == 30 && n.Length == 60), "The awaited chord callback did not persist the added note.");
            ClassicAssert.IsFalse(notes.Any(n => n.NoteNumber == (SevenBitNumber)60 && n.Time == 0), "The awaited chord callback did not persist the removed note.");
        }
    }
}
