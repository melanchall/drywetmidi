using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class MidiFileSplitterTests
    {
        #region Test methods

        [Test]
        public void SplitByChannel_ValidFiles()
        {
            foreach (var filePath in TestFilesProvider.GetValidFilesPaths())
            {
                var midiFile = MidiFile.Read(filePath);
                var originalChannels = midiFile
                    .GetTrackChunks()
                    .SelectMany(c => c.Events)
                    .OfType<ChannelEvent>()
                    .Select(e => e.Channel)
                    .Distinct()
                    .ToArray();
                if (!originalChannels.Any())
                    continue;

                var filesByChannel = midiFile.SplitByChannel().ToList();
                var allChannels = new List<FourBitNumber>(FourBitNumber.Values.Length);

                foreach (var fileByChannel in filesByChannel)
                {
                    Assert.AreEqual(
                        fileByChannel.TimeDivision,
                        midiFile.TimeDivision,
                        "Time division of new file doesn't equal to the time division of the original one.");

                    var channels = fileByChannel
                        .GetTrackChunks()
                        .SelectMany(c => c.Events)
                        .OfType<ChannelEvent>()
                        .Select(e => e.Channel)
                        .Distinct()
                        .ToArray();

                    Assert.AreEqual(
                        1,
                        channels.Length,
                        "New file contains channel events for different channels.");

                    allChannels.Add(channels.First());
                }

                allChannels.Sort();

                CollectionAssert.AreEqual(
                    originalChannels.OrderBy(c => c),
                    allChannels,
                    "Channels from new files differs from those from original one.");
            }
        }

        [Test]
        public void SplitByChannel_EmptyFile()
        {
            SplitByChannel(
                timedEvents: new TimedEvent[0],
                expectedTimedEvents: new[] { new TimedEvent[0] });
        }

        [Test]
        public void SplitByChannel_SingleEvent_NonChannel()
        {
            SplitByChannel(
                timedEvents: new[] { new TimedEvent(new TextEvent("A")) },
                expectedTimedEvents: new[] { new[] { new TimedEvent(new TextEvent("A")) } });
        }

        [Test]
        public void SplitByChannel_SingleEvent_Channel()
        {
            SplitByChannel(
                timedEvents: new[] { new TimedEvent(new NoteOnEvent()) },
                expectedTimedEvents: new[] { new[] { new TimedEvent(new NoteOnEvent()) } });
        }

        [Test]
        public void SplitByChannel_OnlyChannelEvents_SingleChannel()
        {
            SplitByChannel(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent()),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new NoteOffEvent()),
                    }
                });
        }

        [Test]
        public void SplitByChannel_OnlyChannelEvents_MultipleChannels()
        {
            SplitByChannel(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent()),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new NoteOffEvent()),
                    },
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    }
                });
        }

        [Test]
        public void SplitByChannel_ChannelAndNonChannelEvents_SingleChannel()
        {
            SplitByChannel(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new TextEvent("A")),
                    new TimedEvent(new NoteOffEvent()),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new NoteOffEvent()),
                    }
                });
        }

        [Test]
        public void SplitByChannel_ChannelAndNonChannelEvents_MultipleChannels()
        {
            SplitByChannel(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new TextEvent("A")),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent()),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new NoteOffEvent()),
                    },
                    new[]
                    {
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    }
                });
        }

        [Test]
        public void SplitByChannel_ChannelAndNonChannelEvents_MultipleChannels_CustomTimeDivision()
        {
            SplitByChannel(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new TextEvent("A")),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent()),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new NoteOffEvent()),
                    },
                    new[]
                    {
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    }
                },
                timeDivision: new TicksPerQuarterNoteTimeDivision(10000));
        }

        [Test]
        public void SplitByChannel_AllChannels()
        {
            SplitByChannel(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)0 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)1 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)2 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)3 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)4 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)6 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)7 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)8 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)9 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)10 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)11 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)12 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)13 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)14 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)15 }),
                },
                expectedTimedEvents: new[]
                {
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)0 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)1 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)2 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)3 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)4 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)6 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)7 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)8 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)9 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)10 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)11 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)12 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)13 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)14 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)15 }) },
                },
                timeDivision: new TicksPerQuarterNoteTimeDivision(10000));
        }

        [Test]
        public void SplitByChannel_CopyNonChannelEventsToEachFile_EmptyFile([Values] bool copyNonChannelEventsToEachFile)
        {
            SplitByChannel_CopyNonChannelEventsToEachFile(
                timedEvents: new TimedEvent[0],
                expectedTimedEvents: new[] { new TimedEvent[0] },
                copyNonChannelEventsToEachFile: copyNonChannelEventsToEachFile);
        }

        [Test]
        public void SplitByChannel_CopyNonChannelEventsToEachFile_SingleEvent_NonChannel([Values] bool copyNonChannelEventsToEachFile)
        {
            SplitByChannel_CopyNonChannelEventsToEachFile(
                timedEvents: new[] { new TimedEvent(new TextEvent("A")) },
                expectedTimedEvents: new[] { new[] { new TimedEvent(new TextEvent("A")) } },
                copyNonChannelEventsToEachFile: copyNonChannelEventsToEachFile);
        }

        [Test]
        public void SplitByChannel_CopyNonChannelEventsToEachFile_SingleEvent_Channel([Values] bool copyNonChannelEventsToEachFile)
        {
            SplitByChannel_CopyNonChannelEventsToEachFile(
                timedEvents: new[] { new TimedEvent(new NoteOnEvent()) },
                expectedTimedEvents: new[] { new[] { new TimedEvent(new NoteOnEvent()) } },
                copyNonChannelEventsToEachFile: copyNonChannelEventsToEachFile);
        }

        [Test]
        public void SplitByChannel_CopyNonChannelEventsToEachFile_OnlyChannelEvents_SingleChannel([Values] bool copyNonChannelEventsToEachFile)
        {
            SplitByChannel_CopyNonChannelEventsToEachFile(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent()),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new NoteOffEvent()),
                    }
                },
                copyNonChannelEventsToEachFile: copyNonChannelEventsToEachFile);
        }

        [Test]
        public void SplitByChannel_CopyNonChannelEventsToEachFile_OnlyChannelEvents_MultipleChannels([Values] bool copyNonChannelEventsToEachFile)
        {
            SplitByChannel_CopyNonChannelEventsToEachFile(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent()),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new NoteOffEvent()),
                    },
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    }
                },
                copyNonChannelEventsToEachFile: copyNonChannelEventsToEachFile);
        }

        [Test]
        public void SplitByChannel_CopyNonChannelEventsToEachFile_ChannelAndNonChannelEvents_SingleChannel_Copy()
        {
            SplitByChannel_CopyNonChannelEventsToEachFile(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new TextEvent("A")),
                    new TimedEvent(new NoteOffEvent()),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new NoteOffEvent()),
                    }
                },
                copyNonChannelEventsToEachFile: true);
        }

        [Test]
        public void SplitByChannel_CopyNonChannelEventsToEachFile_ChannelAndNonChannelEvents_SingleChannel_DontCopy()
        {
            SplitByChannel_CopyNonChannelEventsToEachFile(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new TextEvent("A")),
                    new TimedEvent(new NoteOffEvent()),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new NoteOffEvent()),
                    }
                },
                copyNonChannelEventsToEachFile: false);
        }

        [Test]
        public void SplitByChannel_CopyNonChannelEventsToEachFile_ChannelAndNonChannelEvents_MultipleChannels_Copy()
        {
            SplitByChannel_CopyNonChannelEventsToEachFile(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new TextEvent("A")),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent()),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new NoteOffEvent()),
                    },
                    new[]
                    {
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    }
                },
                copyNonChannelEventsToEachFile: true);
        }

        [Test]
        public void SplitByChannel_CopyNonChannelEventsToEachFile_ChannelAndNonChannelEvents_MultipleChannels_DontCopy()
        {
            SplitByChannel_CopyNonChannelEventsToEachFile(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new TextEvent("A")),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent()),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new NoteOffEvent()),
                    },
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    }
                },
                copyNonChannelEventsToEachFile: false);
        }

        [Test]
        public void SplitByChannel_CopyNonChannelEventsToEachFile_AllChannels([Values] bool copyNonChannelEventsToEachFile)
        {
            SplitByChannel_CopyNonChannelEventsToEachFile(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)0 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)1 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)2 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)3 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)4 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)6 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)7 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)8 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)9 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)10 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)11 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)12 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)13 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)14 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)15 }),
                },
                expectedTimedEvents: new[]
                {
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)0 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)1 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)2 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)3 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)4 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)6 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)7 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)8 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)9 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)10 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)11 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)12 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)13 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)14 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)15 }) },
                },
                copyNonChannelEventsToEachFile: copyNonChannelEventsToEachFile);
        }

        [Test]
        public void SplitByChannel_Filter_NonChannel()
        {
            SplitByChannel_Filter(
                timedEvents: new[]
                {
                    new TimedEvent(new TextEvent("A")),
                    new TimedEvent(new TextEvent("B")),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new TextEvent("A")),
                    }
                },
                filter: e => e.Event is TextEvent textEvent && textEvent.Text == "A");
        }

        [Test]
        public void SplitByChannel_Filter_AllChannels()
        {
            SplitByChannel_Filter(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)0 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)1 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)2 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)3 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)4 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)6 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)7 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)8 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)9 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)10 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)11 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)12 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)13 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)14 }),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)15 }),
                },
                expectedTimedEvents: new[]
                {
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)0 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)1 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)2 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)3 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)4 }) },
                    new[] { new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }) },
                },
                filter: e => (e.Event as ChannelEvent)?.Channel < 6);
        }

        #endregion

        #region Private methods

        private void SplitByChannel(
            ICollection<TimedEvent> timedEvents,
            ICollection<ICollection<TimedEvent>> expectedTimedEvents,
            TimeDivision timeDivision = null)
        {
            var midiFile = timedEvents.ToFile();
            if (timeDivision != null)
                midiFile.TimeDivision = timeDivision;

            var midiFilesByChannel = midiFile.SplitByChannel().ToList();

            Assert.AreEqual(expectedTimedEvents.Count, midiFilesByChannel.Count, "Invalid count of new files.");

            var expectedTimedEventsEnumerator = expectedTimedEvents.GetEnumerator();
            var newMidiFilesEnumerator = midiFilesByChannel.GetEnumerator();

            var i = 0;

            while (expectedTimedEventsEnumerator.MoveNext() && newMidiFilesEnumerator.MoveNext())
            {
                var expectedEvents = expectedTimedEventsEnumerator.Current;
                var actualEvents = newMidiFilesEnumerator.Current.GetTimedEvents();

                MidiAsserts.AreEqual(expectedEvents, actualEvents, $"Invalid events of file {i}.");
                Assert.AreEqual(midiFile.TimeDivision, newMidiFilesEnumerator.Current.TimeDivision, $"Invalid time division of file {i}.");

                i++;
            }
        }

        private void SplitByChannel_CopyNonChannelEventsToEachFile(
            ICollection<TimedEvent> timedEvents,
            ICollection<ICollection<TimedEvent>> expectedTimedEvents,
            bool copyNonChannelEventsToEachFile)
        {
            SplitByChannel_WithSettings(
                timedEvents,
                expectedTimedEvents,
                copyNonChannelEventsToEachFile,
                null);
        }

        private void SplitByChannel_Filter(
            ICollection<TimedEvent> timedEvents,
            ICollection<ICollection<TimedEvent>> expectedTimedEvents,
            Predicate<TimedEvent> filter)
        {
            SplitByChannel_WithSettings(
                timedEvents,
                expectedTimedEvents,
                true,
                filter);
        }

        private void SplitByChannel_WithSettings(
            ICollection<TimedEvent> timedEvents,
            ICollection<ICollection<TimedEvent>> expectedTimedEvents,
            bool copyNonChannelEventsToEachFile,
            Predicate<TimedEvent> filter)
        {
            var midiFile = timedEvents.ToFile();

            var midiFilesByChannel = midiFile
                .SplitByChannel(new SplitFileByChannelSettings
                {
                    CopyNonChannelEventsToEachFile = copyNonChannelEventsToEachFile,
                    Filter = filter
                })
                .ToList();

            Assert.AreEqual(expectedTimedEvents.Count, midiFilesByChannel.Count, "Invalid count of new files.");

            var expectedTimedEventsEnumerator = expectedTimedEvents.GetEnumerator();
            var newMidiFilesEnumerator = midiFilesByChannel.GetEnumerator();

            var i = 0;

            while (expectedTimedEventsEnumerator.MoveNext() && newMidiFilesEnumerator.MoveNext())
            {
                var expectedEvents = expectedTimedEventsEnumerator.Current;
                var actualEvents = newMidiFilesEnumerator.Current.GetTimedEvents();

                MidiAsserts.AreEqual(expectedEvents, actualEvents, $"Invalid events of file {i}.");

                i++;
            }
        }

        #endregion
    }
}
