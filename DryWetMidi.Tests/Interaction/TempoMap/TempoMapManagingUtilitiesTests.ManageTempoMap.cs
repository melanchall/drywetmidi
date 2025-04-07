using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using NUnit.Framework.Legacy;
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
            ClassicAssert.DoesNotThrow(() => new MidiFile().ManageTempoMap());

        [Test]
        public void ManageTempoMap_EmptyTrackChunksCollection_NoException() =>
            ClassicAssert.DoesNotThrow(() => new List<TrackChunk>().ManageTempoMap(new TicksPerQuarterNoteTimeDivision()));

        [Test]
        public void ManageTempoMap_EmptyTrackChunksCollection_Exception() =>
            ClassicAssert.Throws<ArgumentOutOfRangeException>(() => Array.Empty<TrackChunk>().ManageTempoMap(new TicksPerQuarterNoteTimeDivision()));

        [Test]
        public void ManageTempoMap_EmptyEventsCollectionsCollection_NoException() =>
            ClassicAssert.DoesNotThrow(() => new List<EventsCollection>().ManageTempoMap(new TicksPerQuarterNoteTimeDivision()));

        [Test]
        public void ManageTempoMap_EmptyEventsCollectionsCollection_Exception() =>
            ClassicAssert.Throws<ArgumentOutOfRangeException>(() => Array.Empty<EventsCollection>().ManageTempoMap(new TicksPerQuarterNoteTimeDivision()));

        #endregion
    }
}
