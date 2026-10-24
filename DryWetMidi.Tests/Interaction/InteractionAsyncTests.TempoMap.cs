using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    public sealed partial class InteractionAsyncTests
    {
        [Test]
        public async Task TempoMap_ReplacementAndManagerSaveMatchSynchronous()
        {
            var file = new MidiFile(CreateTrack());
            var tempoMap = TempoMap.Create(new Tempo(400000));

            await file.ReplaceTempoMapAsync(tempoMap);

            ClassicAssert.AreEqual(new Tempo(400000), (await file.GetTempoMapAsync()).GetTempoAtTime(new MidiTimeSpan(0)), "Async tempo-map replacement was not persisted.");

            using (var manager = await file.ManageTempoMapAsync())
            {
                await manager.ReplaceTempoMapAsync(TempoMap.Create(new Tempo(600000)));
                await manager.SaveChangesAsync();

                ClassicAssert.AreEqual(new Tempo(600000), file.GetTempoMap().GetTempoAtTime(new MidiTimeSpan(0)), "Async tempo-map manager save was not persisted.");
            }
        }

        [Test]
        public void CancellationDuringEmptyTrackEnumeration_StopsBeforeMaterialization()
        {
            using var cancellation = new CancellationTokenSource();
            var visited = 0;
            IEnumerable<TrackChunk> Produce()
            {
                while (visited < 100000)
                {
                    if (++visited == 100)
                        cancellation.Cancel();

                    yield return new TrackChunk();
                }
            }

            Assert.ThrowsAsync<OperationCanceledException>(async () => await Produce().GetTempoMapAsync(new TicksPerQuarterNoteTimeDivision(), cancellationToken: cancellation.Token), "Tempo-map detection did not observe cancellation.");
            ClassicAssert.AreEqual(100, visited, "Empty-track enumeration continued after cancellation.");
        }
    }
}
