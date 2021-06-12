using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Devices
{
    [TestFixture]
    public sealed partial class PlaybackTests
    {
        #region Nested classes

        private sealed class CustomTimedObject : ITimedObject
        {
            public long Time { get; set; }
        }

        private sealed class CustomTimedEvent : TimedEvent, IMetadata
        {
            public CustomTimedEvent(MidiEvent midiEvent, object metadata)
                : base(midiEvent)
            {
                Metadata = metadata;
            }

            public object Metadata { get; set; }
        }

        private sealed class CustomObjectsPlayback : Playback
        {
            public CustomObjectsPlayback(IEnumerable<ITimedObject> timedObjects, TempoMap tempoMap)
                : base(timedObjects, tempoMap)
            {
            }

            protected override IEnumerable<TimedEvent> GetTimedEvents(ITimedObject timedObject)
            {
                var customTimedObject = timedObject as CustomTimedObject;
                if (customTimedObject != null)
                    return new[]
                    {
                        new CustomTimedEvent(new NoteOnEvent(), 0),
                        new CustomTimedEvent(new NoteOffEvent(), 1),
                    };

                return Enumerable.Empty<TimedEvent>();
            }
        }

        #endregion

        #region Test methods

        [Test]
        public void CustomPlayback_PlayCustomObject()
        {
            var actualMetadata = new List<(MidiEvent, object)>();

            using (var playback = new CustomObjectsPlayback(new[] { new CustomTimedObject() }, TempoMap.Default))
            {
                playback.EventPlayed += (_, e) => actualMetadata.Add((e.Event, e.Metadata));

                playback.Start();
                SpinWait.SpinUntil(() => !playback.IsRunning);

                CheckRegisteredMetadata(
                    expectedMetadata: new (MidiEvent, object)[]
                    {
                        (new NoteOnEvent(), 0),
                        (new NoteOffEvent(), 1)
                    },
                    actualMetadata: actualMetadata.ToArray());
            }
        }

        #endregion
    }
}
