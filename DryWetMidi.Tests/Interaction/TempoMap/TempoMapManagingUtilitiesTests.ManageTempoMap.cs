using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class TempoMapManagingUtilitiesTests
    {
        #region Test methods

        [Test]
        public void ManageTempoMap_EmptyFile_NoException() =>
            Assert.DoesNotThrow(() => new MidiFile().ManageTempoMap());

        [Test]
        public void ManageTempoMap_EmptyTrackChunksCollection_NoException() =>
            Assert.DoesNotThrow(() => new List<TrackChunk>().ManageTempoMap(new TicksPerQuarterNoteTimeDivision()));

        [Test]
        public void ManageTempoMap_EmptyTrackChunksCollection_Exception() =>
            Assert.Throws<ArgumentOutOfRangeException>(() => Array.Empty<TrackChunk>().ManageTempoMap(new TicksPerQuarterNoteTimeDivision()));

        [Test]
        public void ManageTempoMap_EmptyEventsCollectionsCollection_NoException() =>
            Assert.DoesNotThrow(() => new List<EventsCollection>().ManageTempoMap(new TicksPerQuarterNoteTimeDivision()));

        [Test]
        public void ManageTempoMap_EmptyEventsCollectionsCollection_Exception() =>
            Assert.Throws<ArgumentOutOfRangeException>(() => Array.Empty<EventsCollection>().ManageTempoMap(new TicksPerQuarterNoteTimeDivision()));

        #endregion
    }
}
