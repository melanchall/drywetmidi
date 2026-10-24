using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    public sealed partial class InteractionAsyncTests
    {
        [Test]
        public async Task CanceledManagerSave_PreservesEventsAndPreviousDeltaTimes()
        {
            using var cancellation = new CancellationTokenSource();
            var track = CreateTrack();
            var comparer = new CancelingComparer();
            using var manager = await track.ManageNotesAsync(comparer: comparer);
            await manager.SaveChangesAsync();
            var expected = track.GetTimedEvents();
            manager.Objects.First().Time += 20;
            comparer.OnCompare = cancellation.Cancel;

            Assert.CatchAsync<OperationCanceledException>(
                async () => await manager.SaveChangesAsync(cancellationToken: cancellation.Token),
                "Manager saving did not observe cancellation.");

            comparer.OnCompare = null;

            MidiAsserts.AreEqual(expected, track.GetTimedEvents(), "Canceled manager save must preserve committed contents");
        }

        [Test]
        public async Task Managers_CreateAndSaveChangesAsync()
        {
            var track = CreateTrack();

            using (var manager = await track.ManageNotesAsync())
            {
                manager.Objects.First().Length = 25;

                await manager.SaveChangesAsync();

                ClassicAssert.AreEqual(25, track.GetNotes().First().Length, "Async manager did not save note length.");
            }

            using (var manager = await TimedObjectsManager.CreateAsync(track.Events, ObjectType.Note))
            {
                manager.Objects.First().Time = 10;
                await manager.SaveChangesAsync();
            }

            using (var manager = await TimedObjectsManager<Note>.CreateAsync(track.Events))
                ClassicAssert.AreEqual(3, manager.Objects.Count, "Async generic manager detected an invalid note count.");

            using (var manager = await track.ManageChordsAsync())
                ClassicAssert.AreEqual(track.GetChords().Count, manager.Objects.Count, "Async chord manager count differs.");

            using (var manager = await track.ManageTimedEventsAsync())
                ClassicAssert.AreEqual(7, manager.Objects.Count, "Async timed-event manager detected an invalid event count.");
        }
    }
}
