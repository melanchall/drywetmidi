using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class TempoMapManagingUtilitiesTests
    {
        #region Test methods

        [Test]
        public void ManageTempoMap_EmptyFile_NoException()
        {
            ClassicAssert.DoesNotThrow(() => new MidiFile().ManageTempoMap());
            ClassicAssert.DoesNotThrow(() => new MidiFile().ManageTempoMapAsync(cancellationToken: CancellationToken.None).Wait(), "Async tempo-map managing rejected an empty file.");
        }

        [Test]
        public void ManageTempoMap_EmptyTrackChunksCollection_NoException()
        {
            ClassicAssert.DoesNotThrow(() => new List<TrackChunk>().ManageTempoMap(new TicksPerQuarterNoteTimeDivision()));
            ClassicAssert.DoesNotThrow(() => new List<TrackChunk>().ManageTempoMapAsync(
                new TicksPerQuarterNoteTimeDivision(),
                cancellationToken: CancellationToken.None).Wait(), "Async tempo-map managing rejected an empty track list.");
        }

        [Test]
        public void ManageTempoMap_EmptyTrackChunksCollection_Exception()
        {
            ClassicAssert.Throws<ArgumentOutOfRangeException>(() => Array.Empty<TrackChunk>().ManageTempoMap(new TicksPerQuarterNoteTimeDivision()));

            var exception = ClassicAssert.Throws<AggregateException>(() => Array.Empty<TrackChunk>().ManageTempoMapAsync(
                new TicksPerQuarterNoteTimeDivision(),
                cancellationToken: CancellationToken.None).Wait(), "Async tempo-map managing accepted an empty track array.");

            ClassicAssert.IsInstanceOf<ArgumentOutOfRangeException>(exception.InnerException, "Async empty-track exception type differs.");
        }

        [Test]
        public void ManageTempoMap_EmptyEventsCollectionsCollection_NoException()
        {
            ClassicAssert.DoesNotThrow(() => new List<EventsCollection>().ManageTempoMap(new TicksPerQuarterNoteTimeDivision()));
            ClassicAssert.DoesNotThrow(() => new List<EventsCollection>().ManageTempoMapAsync(
                new TicksPerQuarterNoteTimeDivision(),
                cancellationToken: CancellationToken.None).Wait(), "Async tempo-map managing rejected an empty events-collection list.");
        }

        [Test]
        public void ManageTempoMap_EmptyEventsCollectionsCollection_Exception()
        {
            ClassicAssert.Throws<ArgumentOutOfRangeException>(() => Array.Empty<EventsCollection>().ManageTempoMap(new TicksPerQuarterNoteTimeDivision()));

            var exception = ClassicAssert.Throws<AggregateException>(() => Array.Empty<EventsCollection>().ManageTempoMapAsync(
                new TicksPerQuarterNoteTimeDivision(),
                cancellationToken: CancellationToken.None).Wait(), "Async tempo-map managing accepted an empty events-collection array.");

            ClassicAssert.IsInstanceOf<ArgumentOutOfRangeException>(exception.InnerException, "Async empty-events exception type differs.");
        }

        #endregion
    }
}
