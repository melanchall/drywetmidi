using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class RepeaterTests
    {
        #region Nested classes

        private sealed class SinglePassEnumerable<T> : IEnumerable<T>
        {
            private readonly IEnumerable<T> _items;

            public SinglePassEnumerable(IEnumerable<T> items)
            {
                _items = items;
            }

            public int EnumerationCount { get; private set; }

            public IEnumerator<T> GetEnumerator()
            {
                EnumerationCount++;
                if (EnumerationCount > 1)
                    throw new InvalidOperationException("The collection can only be enumerated once.");

                return _items.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        #endregion

        #region Test methods

        [Test]
        public void Repeat_EmptyTimedObjects_WithMaximumRepeatCount()
        {
            var actualObjects = new Repeater().Repeat(
                Array.Empty<ITimedObject>(),
                int.MaxValue,
                TempoMap.Default);

            Assert.That(actualObjects, Is.Empty, "Repeating an empty collection should return no objects.");
        }

        [Test]
        public void Repeat_EmptyTimedObjectsExtension_WithMaximumRepeatCount()
        {
            var actualObjects = Array.Empty<ITimedObject>().Repeat(
                int.MaxValue,
                TempoMap.Default);

            Assert.That(actualObjects, Is.Empty, "Repeating an empty collection should return no objects.");
        }

        [Test]
        public void Repeat_EmptyTrackChunkCollection()
        {
            var actualTrackChunks = new Repeater().Repeat(
                Array.Empty<TrackChunk>(),
                1,
                TempoMap.Default);

            Assert.That(actualTrackChunks, Is.Empty, "Repeating an empty track chunk collection should return no chunks.");
        }

        [Test]
        public void Repeat_EmptyTrackChunkCollectionExtension()
        {
            var actualTrackChunks = Array.Empty<TrackChunk>().Repeat(
                1,
                TempoMap.Default);

            Assert.That(actualTrackChunks, Is.Empty, "Repeating an empty track chunk collection should return no chunks.");
        }

        [Test]
        public void Repeat_EmptyMidiFile()
        {
            var actualFile = new MidiFile().Repeat(1);

            Assert.That(actualFile.Chunks, Is.Empty, "Repeating an empty MIDI file should return a file without chunks.");
        }

        [Test]
        public void Repeat_TimedObjectsEnumeratesInputOnce()
        {
            var timedObjects = new SinglePassEnumerable<ITimedObject?>(
                new ITimedObject?[] { new TimedEvent(new TextEvent("A")) });

            var actualObjects = new Repeater().Repeat(timedObjects, 1, TempoMap.Default);

            Assert.That(timedObjects.EnumerationCount, Is.EqualTo(1), "The source collection should be enumerated once.");
            Assert.That(actualObjects, Has.Count.EqualTo(2), "The repeated collection should contain the original and one repeated object.");
        }

        [Test]
        public void Repeat_TimedObjectsExtensionEnumeratesInputOnce()
        {
            var timedObjects = new SinglePassEnumerable<ITimedObject?>(
                new ITimedObject?[] { new TimedEvent(new TextEvent("A")) });

            var actualObjects = timedObjects.Repeat(1, TempoMap.Default);

            Assert.That(timedObjects.EnumerationCount, Is.EqualTo(1), "The source collection should be enumerated once.");
            Assert.That(actualObjects, Has.Count.EqualTo(2), "The repeated collection should contain the original and one repeated object.");
        }

        #endregion
    }
}
