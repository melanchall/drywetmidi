using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class SplitterTests
    {
        #region Test methods

        [Test]
        public void SplitByNotes_ValidFiles()
        {
            foreach (var filePath in TestFilesProvider.GetValidFilesPaths())
            {
                var midiFile = MidiFile.Read(filePath);
                var originalNoteEvents = midiFile.GetTrackChunks()
                                                 .SelectMany(c => c.Events)
                                                 .OfType<NoteEvent>()
                                                 .ToList();
                if (!originalNoteEvents.Any())
                    continue;

                var fileIndex = 0;
                var allNoteEventsCount = 0;
                var allNotesIds = new HashSet<NoteId>();

                foreach (var fileByNotes in midiFile.SplitByNotes())
                {
                    var noteEvents = fileByNotes.GetTrackChunks()
                                                .SelectMany(c => c.Events)
                                                .OfType<NoteEvent>()
                                                .ToList();
                    var notesIds = new HashSet<NoteId>(noteEvents.Select(n => n.GetNoteId()));

                    allNoteEventsCount += noteEvents.Count;
                    foreach (var noteId in notesIds)
                    {
                        allNotesIds.Add(noteId);
                    }

                    Assert.AreEqual(1, notesIds.Count, $"New file ({fileIndex}) contains different notes.");

                    fileIndex++;
                }

                var originalNoteEventsCount = originalNoteEvents.Count();
                var originalNotesIds = new HashSet<NoteId>(originalNoteEvents.Select(e => e.GetNoteId()));

                Assert.AreEqual(originalNoteEventsCount,
                                allNoteEventsCount,
                                "Notes count of new files doesn't equal to count of notes of the original file.");

                Assert.IsTrue(originalNotesIds.SetEquals(allNotesIds),
                              "Notes in new files differ from notes in the original file.");
            }
        }

        [Test]
        public void SplitByNotes_EmptyFile()
        {
            SplitByNotes(
                timedEvents: new TimedEvent[0],
                expectedTimedEvents: new[] { new TimedEvent[0] });
        }

        [Test]
        public void SplitByNotes_OneEvent_NonNote_1()
        {
            SplitByNotes(
                timedEvents: new[] { new TimedEvent(new TextEvent("A")) },
                expectedTimedEvents: new[] { new[] { new TimedEvent(new TextEvent("A")) } });
        }

        [Test]
        public void SplitByNotes_OneEvent_NonNote_2()
        {
            SplitByNotes(
                timedEvents: new[] { new TimedEvent(new ControlChangeEvent()) },
                expectedTimedEvents: new[] { new[] { new TimedEvent(new ControlChangeEvent()) } });
        }

        [Test]
        public void SplitByNotes_OneEvent_NoteOn()
        {
            SplitByNotes(
                timedEvents: new[] { new TimedEvent(new NoteOnEvent()) },
                expectedTimedEvents: new[] { new[] { new TimedEvent(new NoteOnEvent()) } });
        }

        [Test]
        public void SplitByNotes_OneEvent_NoteOff()
        {
            SplitByNotes(
                timedEvents: new[] { new TimedEvent(new NoteOffEvent()) },
                expectedTimedEvents: new[] { new[] { new TimedEvent(new NoteOffEvent()) } });
        }

        [Test]
        public void SplitByNotes_MultipleEvents_NonNote()
        {
            SplitByNotes(
                timedEvents: new[]
                {
                    new TimedEvent(new TextEvent("A")),
                    new TimedEvent(new ControlChangeEvent()),
                    new TimedEvent(new NormalSysExEvent(new byte[] { 0x5F })),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new ControlChangeEvent()),
                        new TimedEvent(new NormalSysExEvent(new byte[] { 0x5F })),
                    }
                });
        }

        [Test]
        public void SplitByNotes_MultipleEvents_Note_SingleChannel_SingleNoteNumber()
        {
            SplitByNotes(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent()),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new NoteOffEvent()),
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new NoteOffEvent()),
                    }
                });
        }

        [Test]
        public void SplitByNotes_MultipleEvents_Note_SingleChannel_DifferentNoteNumbers()
        {
            SplitByNotes(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
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
                        new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                        new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
                    }
                });
        }

        [Test]
        public void SplitByNotes_MultipleEvents_Note_DifferentChannels_SingleNoteNumber()
        {
            SplitByNotes(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)5 }),
                        new TimedEvent(new NoteOffEvent() { Channel = (FourBitNumber)5 }),
                    },
                    new[]
                    {
                        new TimedEvent(new NoteOffEvent()),
                        new TimedEvent(new NoteOnEvent()),
                    }
                });
        }

        [Test]
        public void SplitByNotes_MultipleEvents_Note_DifferentChannels_DifferentNoteNumbers()
        {
            SplitByNotes(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue) { Channel = (FourBitNumber)5 }),
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
                        new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                        new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
                    },
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                        new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                    },
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)5 }),
                        new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue) { Channel = (FourBitNumber)5 }),
                    },
                });
        }

        [Test]
        public void SplitByNotes_MultipleEvents_Mixed_SingleChannel_SingleNoteNumber()
        {
            SplitByNotes(
                timedEvents: new[]
                {
                    new TimedEvent(new TextEvent("A")),
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new ControlChangeEvent()),
                    new TimedEvent(new NoteOffEvent()),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new NoteOffEvent()),
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new ControlChangeEvent()),
                        new TimedEvent(new NoteOffEvent()),
                    }
                });
        }

        [Test]
        public void SplitByNotes_MultipleEvents_Mixed_SingleChannel_DifferentNoteNumbers()
        {
            SplitByNotes(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new TextEvent("A")),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
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
                        new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                        new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
                    }
                });
        }

        [Test]
        public void SplitByNotes_MultipleEvents_Mixed_DifferentChannels_SingleNoteNumber()
        {
            SplitByNotes(
                timedEvents: new[]
                {
                    new TimedEvent(new TextEvent("A")),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)5 }),
                        new TimedEvent(new NoteOffEvent() { Channel = (FourBitNumber)5 }),
                    },
                    new[]
                    {
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new NoteOffEvent()),
                        new TimedEvent(new NoteOnEvent()),
                    }
                });
        }

        [Test]
        public void SplitByNotes_MultipleEvents_Mixed_DifferentChannels_DifferentNoteNumbers()
        {
            SplitByNotes(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new TextEvent("A")),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
                    new TimedEvent(new TextEvent("B")),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue) { Channel = (FourBitNumber)5 }),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new NoteOffEvent()),
                        new TimedEvent(new TextEvent("B")),
                    },
                    new[]
                    {
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                        new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
                        new TimedEvent(new TextEvent("B")),
                    },
                    new[]
                    {
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new TextEvent("B")),
                        new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                        new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                    },
                    new[]
                    {
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new TextEvent("B")),
                        new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)5 }),
                        new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue) { Channel = (FourBitNumber)5 }),
                    },
                });
        }

        [Test]
        public void SplitByNotes_MultipleEvents_Note_AllNoteNumbers_AllChannels()
        {
            SplitByNotes(
                timedEvents: FourBitNumber.Values.SelectMany(channel => SevenBitNumber.Values.SelectMany(noteNumber => new[]
                {
                    new TimedEvent(new NoteOnEvent(noteNumber, SevenBitNumber.MaxValue) { Channel = channel }),
                    new TimedEvent(new NoteOffEvent(noteNumber, SevenBitNumber.MinValue) { Channel = channel }),
                })).ToArray(),
                expectedTimedEvents: FourBitNumber.Values.SelectMany(channel => SevenBitNumber.Values.Select(noteNumber => new[]
                {
                    new TimedEvent(new NoteOnEvent(noteNumber, SevenBitNumber.MaxValue) { Channel = channel }),
                    new TimedEvent(new NoteOffEvent(noteNumber, SevenBitNumber.MinValue) { Channel = channel }),
                }).ToArray()).ToArray());
        }

        [Test]
        public void SplitByNotes_IgnoreChannel_EmptyFile([Values] bool ignoreChannel)
        {
            SplitByNotes_IgnoreChannel(
                timedEvents: new TimedEvent[0],
                expectedTimedEvents: new[] { new TimedEvent[0] },
                ignoreChannel: ignoreChannel);
        }

        [Test]
        public void SplitByNotes_IgnoreChannel_OneEvent_NonNote_1([Values] bool ignoreChannel)
        {
            SplitByNotes_IgnoreChannel(
                timedEvents: new[] { new TimedEvent(new TextEvent("A")) },
                expectedTimedEvents: new[] { new[] { new TimedEvent(new TextEvent("A")) } },
                ignoreChannel: ignoreChannel);
        }

        [Test]
        public void SplitByNotes_IgnoreChannel_OneEvent_NonNote_2([Values] bool ignoreChannel)
        {
            SplitByNotes_IgnoreChannel(
                timedEvents: new[] { new TimedEvent(new ControlChangeEvent()) },
                expectedTimedEvents: new[] { new[] { new TimedEvent(new ControlChangeEvent()) } },
                ignoreChannel: ignoreChannel);
        }

        [Test]
        public void SplitByNotes_IgnoreChannel_OneEvent_NoteOn([Values] bool ignoreChannel)
        {
            SplitByNotes_IgnoreChannel(
                timedEvents: new[] { new TimedEvent(new NoteOnEvent()) },
                expectedTimedEvents: new[] { new[] { new TimedEvent(new NoteOnEvent()) } },
                ignoreChannel);
        }

        [Test]
        public void SplitByNotes_IgnoreChannel_OneEvent_NoteOff([Values] bool ignoreChannel)
        {
            SplitByNotes_IgnoreChannel(
                timedEvents: new[] { new TimedEvent(new NoteOffEvent()) },
                expectedTimedEvents: new[] { new[] { new TimedEvent(new NoteOffEvent()) } },
                ignoreChannel: ignoreChannel);
        }

        [Test]
        public void SplitByNotes_IgnoreChannel_MultipleEvents_NonNote([Values] bool ignoreChannel)
        {
            SplitByNotes_IgnoreChannel(
                timedEvents: new[]
                {
                    new TimedEvent(new TextEvent("A")),
                    new TimedEvent(new ControlChangeEvent()),
                    new TimedEvent(new NormalSysExEvent(new byte[] { 0x5F })),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new ControlChangeEvent()),
                        new TimedEvent(new NormalSysExEvent(new byte[] { 0x5F })),
                    }
                },
                ignoreChannel: ignoreChannel);
        }

        [Test]
        public void SplitByNotes_IgnoreChannel_MultipleEvents_Note_SingleChannel_SingleNoteNumber([Values] bool ignoreChannel)
        {
            SplitByNotes_IgnoreChannel(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent()),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new NoteOffEvent()),
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new NoteOffEvent()),
                    }
                },
                ignoreChannel: ignoreChannel);
        }

        [Test]
        public void SplitByNotes_IgnoreChannel_MultipleEvents_Note_SingleChannel_DifferentNoteNumbers([Values] bool ignoreChannel)
        {
            SplitByNotes_IgnoreChannel(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
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
                        new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                        new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
                    }
                },
                ignoreChannel: ignoreChannel);
        }

        [Test]
        public void SplitByNotes_IgnoreChannel_MultipleEvents_Note_DifferentChannels_SingleNoteNumber_DontIgnore()
        {
            SplitByNotes_IgnoreChannel(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)5 }),
                        new TimedEvent(new NoteOffEvent() { Channel = (FourBitNumber)5 }),
                    },
                    new[]
                    {
                        new TimedEvent(new NoteOffEvent()),
                        new TimedEvent(new NoteOnEvent()),
                    }
                },
                ignoreChannel: false);
        }

        [Test]
        public void SplitByNotes_IgnoreChannel_MultipleEvents_Note_DifferentChannels_SingleNoteNumber_Ignore()
        {
            SplitByNotes_IgnoreChannel(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                        new TimedEvent(new NoteOffEvent()),
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                    }
                },
                ignoreChannel: true);
        }

        [Test]
        public void SplitByNotes_IgnoreChannel_MultipleEvents_Note_DifferentChannels_DifferentNoteNumbers_DontIgnore()
        {
            SplitByNotes_IgnoreChannel(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue) { Channel = (FourBitNumber)5 }),
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
                        new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                        new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
                    },
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                        new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                    },
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)5 }),
                        new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue) { Channel = (FourBitNumber)5 }),
                    },
                },
                ignoreChannel: false);
        }

        [Test]
        public void SplitByNotes_IgnoreChannel_MultipleEvents_Note_DifferentChannels_DifferentNoteNumbers_Ignore()
        {
            SplitByNotes_IgnoreChannel(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue) { Channel = (FourBitNumber)5 }),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new NoteOffEvent()),
                        new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                        new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                    },
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                        new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
                        new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)5 }),
                        new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue) { Channel = (FourBitNumber)5 }),
                    }
                },
                ignoreChannel: true);
        }

        [Test]
        public void SplitByNotes_IgnoreChannel_MultipleEvents_Mixed_SingleChannel_SingleNoteNumber([Values] bool ignoreChannel)
        {
            SplitByNotes_IgnoreChannel(
                timedEvents: new[]
                {
                    new TimedEvent(new TextEvent("A")),
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new ControlChangeEvent()),
                    new TimedEvent(new NoteOffEvent()),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new NoteOffEvent()),
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new ControlChangeEvent()),
                        new TimedEvent(new NoteOffEvent()),
                    }
                },
                ignoreChannel: ignoreChannel);
        }

        [Test]
        public void SplitByNotes_IgnoreChannel_MultipleEvents_Mixed_SingleChannel_DifferentNoteNumbers([Values] bool ignoreChannel)
        {
            SplitByNotes_IgnoreChannel(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new TextEvent("A")),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
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
                        new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                        new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
                    }
                },
                ignoreChannel: ignoreChannel);
        }

        [Test]
        public void SplitByNotes_IgnoreChannel_MultipleEvents_Mixed_DifferentChannels_SingleNoteNumber_DontIgnore()
        {
            SplitByNotes_IgnoreChannel(
                timedEvents: new[]
                {
                    new TimedEvent(new TextEvent("A")),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)5 }),
                        new TimedEvent(new NoteOffEvent() { Channel = (FourBitNumber)5 }),
                    },
                    new[]
                    {
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new NoteOffEvent()),
                        new TimedEvent(new NoteOnEvent()),
                    }
                },
                ignoreChannel: false);
        }

        [Test]
        public void SplitByNotes_IgnoreChannel_MultipleEvents_Mixed_DifferentChannels_SingleNoteNumber_Ignore()
        {
            SplitByNotes_IgnoreChannel(
                timedEvents: new[]
                {
                    new TimedEvent(new TextEvent("A")),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                        new TimedEvent(new NoteOffEvent()),
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                    }
                },
                ignoreChannel: true);
        }

        [Test]
        public void SplitByNotes_IgnoreChannel_MultipleEvents_Mixed_DifferentChannels_DifferentNoteNumbers_DontIgnore()
        {
            SplitByNotes_IgnoreChannel(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new TextEvent("A")),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
                    new TimedEvent(new TextEvent("B")),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue) { Channel = (FourBitNumber)5 }),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new NoteOffEvent()),
                        new TimedEvent(new TextEvent("B")),
                    },
                    new[]
                    {
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                        new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
                        new TimedEvent(new TextEvent("B")),
                    },
                    new[]
                    {
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new TextEvent("B")),
                        new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                        new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                    },
                    new[]
                    {
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new TextEvent("B")),
                        new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)5 }),
                        new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue) { Channel = (FourBitNumber)5 }),
                    },
                },
                ignoreChannel: false);
        }

        [Test]
        public void SplitByNotes_IgnoreChannel_IgnoreChannel_MultipleEvents_Mixed_DifferentChannels_DifferentNoteNumbers_DontIgnore()
        {
            SplitByNotes_IgnoreChannel(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new TextEvent("A")),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
                    new TimedEvent(new TextEvent("B")),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue) { Channel = (FourBitNumber)5 }),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new NoteOffEvent()),
                        new TimedEvent(new TextEvent("B")),
                        new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                        new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                    },
                    new[]
                    {
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                        new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
                        new TimedEvent(new TextEvent("B")),
                        new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)5 }),
                        new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue) { Channel = (FourBitNumber)5 }),
                    }
                },
                ignoreChannel: true);
        }

        [Test]
        public void SplitByNotes_IgnoreChannel_MultipleEvents_Note_AllNoteNumbers_AllChannels_DontIgnore()
        {
            SplitByNotes_IgnoreChannel(
                timedEvents: FourBitNumber.Values.SelectMany(channel => SevenBitNumber.Values.SelectMany(noteNumber => new[]
                {
                    new TimedEvent(new NoteOnEvent(noteNumber, SevenBitNumber.MaxValue) { Channel = channel }),
                    new TimedEvent(new NoteOffEvent(noteNumber, SevenBitNumber.MinValue) { Channel = channel }),
                })).ToArray(),
                expectedTimedEvents: FourBitNumber.Values.SelectMany(channel => SevenBitNumber.Values.Select(noteNumber => new[]
                {
                    new TimedEvent(new NoteOnEvent(noteNumber, SevenBitNumber.MaxValue) { Channel = channel }),
                    new TimedEvent(new NoteOffEvent(noteNumber, SevenBitNumber.MinValue) { Channel = channel }),
                }).ToArray()).ToArray(),
                ignoreChannel: false);
        }

        [Test]
        public void SplitByNotes_IgnoreChannel_MultipleEvents_Note_AllNoteNumbers_AllChannels_Ignore()
        {
            SplitByNotes_IgnoreChannel(
                timedEvents: FourBitNumber.Values.SelectMany(channel => SevenBitNumber.Values.SelectMany(noteNumber => new[]
                {
                    new TimedEvent(new NoteOnEvent(noteNumber, SevenBitNumber.MaxValue) { Channel = channel }),
                    new TimedEvent(new NoteOffEvent(noteNumber, SevenBitNumber.MinValue) { Channel = channel }),
                })).ToArray(),
                expectedTimedEvents: SevenBitNumber.Values.Select(noteNumber => FourBitNumber.Values.SelectMany(channel => new[]
                {
                    new TimedEvent(new NoteOnEvent(noteNumber, SevenBitNumber.MaxValue) { Channel = channel }),
                    new TimedEvent(new NoteOffEvent(noteNumber, SevenBitNumber.MinValue) { Channel = channel }),
                }).ToArray()).ToArray(),
                ignoreChannel: true);
        }

        [Test]
        public void SplitByNotes_CopyNonNoteEventsToEachFile_EmptyFile([Values] bool copyNonNoteEventsToEachFile)
        {
            SplitByNotes_CopyNonNoteEventsToEachFile(
                timedEvents: new TimedEvent[0],
                expectedTimedEvents: new[] { new TimedEvent[0] },
                copyNonNoteEventsToEachFile: copyNonNoteEventsToEachFile);
        }

        [Test]
        public void SplitByNotes_CopyNonNoteEventsToEachFile_OneEvent_NonNote_1([Values] bool copyNonNoteEventsToEachFile)
        {
            SplitByNotes_CopyNonNoteEventsToEachFile(
                timedEvents: new[] { new TimedEvent(new TextEvent("A")) },
                expectedTimedEvents: new[] { new[] { new TimedEvent(new TextEvent("A")) } },
                copyNonNoteEventsToEachFile: copyNonNoteEventsToEachFile);
        }

        [Test]
        public void SplitByNotes_CopyNonNoteEventsToEachFile_OneEvent_NonNote_2([Values] bool copyNonNoteEventsToEachFile)
        {
            SplitByNotes_CopyNonNoteEventsToEachFile(
                timedEvents: new[] { new TimedEvent(new ControlChangeEvent()) },
                expectedTimedEvents: new[] { new[] { new TimedEvent(new ControlChangeEvent()) } },
                copyNonNoteEventsToEachFile: copyNonNoteEventsToEachFile);
        }

        [Test]
        public void SplitByNotes_CopyNonNoteEventsToEachFile_OneEvent_NoteOn([Values] bool copyNonNoteEventsToEachFile)
        {
            SplitByNotes_CopyNonNoteEventsToEachFile(
                timedEvents: new[] { new TimedEvent(new NoteOnEvent()) },
                expectedTimedEvents: new[] { new[] { new TimedEvent(new NoteOnEvent()) } },
                copyNonNoteEventsToEachFile: copyNonNoteEventsToEachFile);
        }

        [Test]
        public void SplitByNotes_CopyNonNoteEventsToEachFile_OneEvent_NoteOff([Values] bool copyNonNoteEventsToEachFile)
        {
            SplitByNotes_CopyNonNoteEventsToEachFile(
                timedEvents: new[] { new TimedEvent(new NoteOffEvent()) },
                expectedTimedEvents: new[] { new[] { new TimedEvent(new NoteOffEvent()) } },
                copyNonNoteEventsToEachFile: copyNonNoteEventsToEachFile);
        }

        [Test]
        public void SplitByNotes_CopyNonNoteEventsToEachFile_MultipleEvents_NonNote([Values] bool copyNonNoteEventsToEachFile)
        {
            SplitByNotes_CopyNonNoteEventsToEachFile(
                timedEvents: new[]
                {
                    new TimedEvent(new TextEvent("A")),
                    new TimedEvent(new ControlChangeEvent()),
                    new TimedEvent(new NormalSysExEvent(new byte[] { 0x5F })),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new ControlChangeEvent()),
                        new TimedEvent(new NormalSysExEvent(new byte[] { 0x5F })),
                    }
                },
                copyNonNoteEventsToEachFile: copyNonNoteEventsToEachFile);
        }

        [Test]
        public void SplitByNotes_CopyNonNoteEventsToEachFile_MultipleEvents_Note_SingleChannel_SingleNoteNumber([Values] bool copyNonNoteEventsToEachFile)
        {
            SplitByNotes_CopyNonNoteEventsToEachFile(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent()),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new NoteOffEvent()),
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new NoteOffEvent()),
                    }
                },
                copyNonNoteEventsToEachFile: copyNonNoteEventsToEachFile);
        }

        [Test]
        public void SplitByNotes_CopyNonNoteEventsToEachFile_MultipleEvents_Note_SingleChannel_DifferentNoteNumbers([Values] bool copyNonNoteEventsToEachFile)
        {
            SplitByNotes_CopyNonNoteEventsToEachFile(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
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
                        new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                        new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
                    }
                },
                copyNonNoteEventsToEachFile: copyNonNoteEventsToEachFile);
        }

        [Test]
        public void SplitByNotes_CopyNonNoteEventsToEachFile_MultipleEvents_Note_DifferentChannels_SingleNoteNumber([Values] bool copyNonNoteEventsToEachFile)
        {
            SplitByNotes_CopyNonNoteEventsToEachFile(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)5 }),
                        new TimedEvent(new NoteOffEvent() { Channel = (FourBitNumber)5 }),
                    },
                    new[]
                    {
                        new TimedEvent(new NoteOffEvent()),
                        new TimedEvent(new NoteOnEvent()),
                    }
                },
                copyNonNoteEventsToEachFile: copyNonNoteEventsToEachFile);
        }

        [Test]
        public void SplitByNotes_CopyNonNoteEventsToEachFile_MultipleEvents_Note_DifferentChannels_DifferentNoteNumbers([Values] bool copyNonNoteEventsToEachFile)
        {
            SplitByNotes_CopyNonNoteEventsToEachFile(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue) { Channel = (FourBitNumber)5 }),
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
                        new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                        new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
                    },
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                        new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                    },
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)5 }),
                        new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue) { Channel = (FourBitNumber)5 }),
                    },
                },
                copyNonNoteEventsToEachFile: copyNonNoteEventsToEachFile);
        }

        [Test]
        public void SplitByNotes_CopyNonNoteEventsToEachFile_MultipleEvents_Mixed_SingleChannel_SingleNoteNumber_Copy()
        {
            SplitByNotes_CopyNonNoteEventsToEachFile(
                timedEvents: new[]
                {
                    new TimedEvent(new TextEvent("A")),
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new ControlChangeEvent()),
                    new TimedEvent(new NoteOffEvent()),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new NoteOffEvent()),
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new ControlChangeEvent()),
                        new TimedEvent(new NoteOffEvent()),
                    }
                },
                copyNonNoteEventsToEachFile: true);
        }

        [Test]
        public void SplitByNotes_CopyNonNoteEventsToEachFile_MultipleEvents_Mixed_SingleChannel_SingleNoteNumber_DontCopy()
        {
            SplitByNotes_CopyNonNoteEventsToEachFile(
                timedEvents: new[]
                {
                    new TimedEvent(new TextEvent("A")),
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new ControlChangeEvent()),
                    new TimedEvent(new NoteOffEvent()),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new NoteOffEvent()),
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new NoteOffEvent()),
                    }
                },
                copyNonNoteEventsToEachFile: false);
        }

        [Test]
        public void SplitByNotes_CopyNonNoteEventsToEachFile_MultipleEvents_Mixed_SingleChannel_DifferentNoteNumbers_Copy()
        {
            SplitByNotes_CopyNonNoteEventsToEachFile(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new TextEvent("A")),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
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
                        new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                        new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
                    }
                },
                copyNonNoteEventsToEachFile: true);
        }

        [Test]
        public void SplitByNotes_CopyNonNoteEventsToEachFile_MultipleEvents_Mixed_SingleChannel_DifferentNoteNumbers_DontCopy()
        {
            SplitByNotes_CopyNonNoteEventsToEachFile(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new TextEvent("A")),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
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
                        new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                        new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
                    }
                },
                copyNonNoteEventsToEachFile: false);
        }

        [Test]
        public void SplitByNotes_CopyNonNoteEventsToEachFile_MultipleEvents_Mixed_DifferentChannels_SingleNoteNumber_Copy()
        {
            SplitByNotes_CopyNonNoteEventsToEachFile(
                timedEvents: new[]
                {
                    new TimedEvent(new TextEvent("A")),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)5 }),
                        new TimedEvent(new NoteOffEvent() { Channel = (FourBitNumber)5 }),
                    },
                    new[]
                    {
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new NoteOffEvent()),
                        new TimedEvent(new NoteOnEvent()),
                    }
                },
                copyNonNoteEventsToEachFile: true);
        }

        [Test]
        public void SplitByNotes_CopyNonNoteEventsToEachFile_MultipleEvents_Mixed_DifferentChannels_SingleNoteNumber_DontCopy()
        {
            SplitByNotes_CopyNonNoteEventsToEachFile(
                timedEvents: new[]
                {
                    new TimedEvent(new TextEvent("A")),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)5 }),
                        new TimedEvent(new NoteOffEvent() { Channel = (FourBitNumber)5 }),
                    },
                    new[]
                    {
                        new TimedEvent(new NoteOffEvent()),
                        new TimedEvent(new NoteOnEvent()),
                    }
                },
                copyNonNoteEventsToEachFile: false);
        }

        [Test]
        public void SplitByNotes_CopyNonNoteEventsToEachFile_MultipleEvents_Mixed_DifferentChannels_DifferentNoteNumbers_Copy()
        {
            SplitByNotes_CopyNonNoteEventsToEachFile(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new TextEvent("A")),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
                    new TimedEvent(new TextEvent("B")),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue) { Channel = (FourBitNumber)5 }),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new NoteOffEvent()),
                        new TimedEvent(new TextEvent("B")),
                    },
                    new[]
                    {
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                        new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
                        new TimedEvent(new TextEvent("B")),
                    },
                    new[]
                    {
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new TextEvent("B")),
                        new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                        new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                    },
                    new[]
                    {
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new TextEvent("B")),
                        new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)5 }),
                        new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue) { Channel = (FourBitNumber)5 }),
                    },
                },
                copyNonNoteEventsToEachFile: true);
        }

        [Test]
        public void SplitByNotes_CopyNonNoteEventsToEachFile_MultipleEvents_Mixed_DifferentChannels_DifferentNoteNumbers_DontCopy()
        {
            SplitByNotes_CopyNonNoteEventsToEachFile(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new TextEvent("A")),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
                    new TimedEvent(new TextEvent("B")),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue) { Channel = (FourBitNumber)5 }),
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
                        new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                        new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
                    },
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                        new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                    },
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)5 }),
                        new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue) { Channel = (FourBitNumber)5 }),
                    },
                },
                copyNonNoteEventsToEachFile: false);
        }

        [Test]
        public void SplitByNotes_CopyNonNoteEventsToEachFile_MultipleEvents_Note_AllNoteNumbers_AllChannels([Values] bool copyNonNoteEventsToEachFile)
        {
            SplitByNotes_CopyNonNoteEventsToEachFile(
                timedEvents: FourBitNumber.Values.SelectMany(channel => SevenBitNumber.Values.SelectMany(noteNumber => new[]
                {
                    new TimedEvent(new NoteOnEvent(noteNumber, SevenBitNumber.MaxValue) { Channel = channel }),
                    new TimedEvent(new NoteOffEvent(noteNumber, SevenBitNumber.MinValue) { Channel = channel }),
                })).ToArray(),
                expectedTimedEvents: FourBitNumber.Values.SelectMany(channel => SevenBitNumber.Values.Select(noteNumber => new[]
                {
                    new TimedEvent(new NoteOnEvent(noteNumber, SevenBitNumber.MaxValue) { Channel = channel }),
                    new TimedEvent(new NoteOffEvent(noteNumber, SevenBitNumber.MinValue) { Channel = channel }),
                }).ToArray()).ToArray(),
                copyNonNoteEventsToEachFile: copyNonNoteEventsToEachFile);
        }

        [Test]
        public void SplitByNotes_Filter_EmptyFile()
        {
            SplitByNotes_Filter(
                timedEvents: new TimedEvent[0],
                expectedTimedEvents: new[] { new TimedEvent[0] },
                filter: e => false);
        }

        [Test]
        public void SplitByNotes_Filter_OneEvent_NonNote_1()
        {
            SplitByNotes_Filter(
                timedEvents: new[] { new TimedEvent(new TextEvent("A")) },
                expectedTimedEvents: new[] { new[] { new TimedEvent(new TextEvent("A")) } },
                filter: e => true);
        }

        [Test]
        public void SplitByNotes_Filter_OneEvent_NonNote_2()
        {
            SplitByNotes_Filter(
                timedEvents: new[] { new TimedEvent(new ControlChangeEvent()) },
                expectedTimedEvents: new[] { new TimedEvent[0] },
                filter: e => e.Event.EventType != MidiEventType.ControlChange);
        }

        [Test]
        public void SplitByNotes_Filter_OneEvent_NoteOn()
        {
            SplitByNotes_Filter(
                timedEvents: new[] { new TimedEvent(new NoteOnEvent()) },
                expectedTimedEvents: new[] { new[] { new TimedEvent(new NoteOnEvent()) } },
                filter: e => true);
        }

        [Test]
        public void SplitByNotes_Filter_OneEvent_NoteOff()
        {
            SplitByNotes_Filter(
                timedEvents: new[] { new TimedEvent(new NoteOffEvent()) },
                expectedTimedEvents: new[] { new TimedEvent[0] },
                filter: e => false);
        }

        [Test]
        public void SplitByNotes_Filter_MultipleEvents_NonNote()
        {
            SplitByNotes_Filter(
                timedEvents: new[]
                {
                    new TimedEvent(new TextEvent("A")),
                    new TimedEvent(new ControlChangeEvent()),
                    new TimedEvent(new NormalSysExEvent(new byte[] { 0x5F })),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new TextEvent("A")),
                    }
                },
                filter: e => e.Event.EventType == MidiEventType.Text);
        }

        [Test]
        public void SplitByNotes_Filter_MultipleEvents_Note_SingleChannel_SingleNoteNumber()
        {
            SplitByNotes_Filter(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent()),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new NoteOnEvent()),
                    }
                },
                filter: e => e.Event.EventType == MidiEventType.NoteOn);
        }

        [Test]
        public void SplitByNotes_Filter_MultipleEvents_Note_SingleChannel_DifferentNoteNumbers()
        {
            SplitByNotes_Filter(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
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
                        new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                        new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
                    }
                },
                filter: e => true);
        }

        [Test]
        public void SplitByNotes_Filter_MultipleEvents_Note_DifferentChannels_SingleNoteNumber()
        {
            SplitByNotes_Filter(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent() { Channel = (FourBitNumber)5 }),
                        new TimedEvent(new NoteOffEvent() { Channel = (FourBitNumber)5 }),
                    }
                },
                filter: e => e.Event is NoteEvent noteEvent && noteEvent.Channel == 5);
        }

        [Test]
        public void SplitByNotes_Filter_MultipleEvents_Note_DifferentChannels_DifferentNoteNumbers()
        {
            SplitByNotes_Filter(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue) { Channel = (FourBitNumber)5 }),
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
                        new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                        new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
                    },
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                        new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                    },
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)5 }),
                        new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue) { Channel = (FourBitNumber)5 }),
                    },
                },
                filter: e => true);
        }

        [Test]
        public void SplitByNotes_Filter_MultipleEvents_Mixed_SingleChannel_SingleNoteNumber()
        {
            SplitByNotes_Filter(
                timedEvents: new[]
                {
                    new TimedEvent(new TextEvent("A")),
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new ControlChangeEvent()),
                    new TimedEvent(new NoteOffEvent()),
                },
                expectedTimedEvents: new[]
                {
                    new[]
                    {
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new NoteOffEvent()),
                        new TimedEvent(new NoteOnEvent()),
                        new TimedEvent(new ControlChangeEvent()),
                        new TimedEvent(new NoteOffEvent()),
                    }
                },
                filter: e => e.Event.EventType != MidiEventType.Text);
        }

        [Test]
        public void SplitByNotes_Filter_MultipleEvents_Mixed_SingleChannel_DifferentNoteNumbers()
        {
            SplitByNotes_Filter(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new TextEvent("A")),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
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
                        new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                        new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
                    }
                },
                filter: e => true);
        }

        [Test]
        public void SplitByNotes_Filter_MultipleEvents_Mixed_DifferentChannels_SingleNoteNumber()
        {
            SplitByNotes_Filter(
                timedEvents: new[]
                {
                    new TimedEvent(new TextEvent("A")),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                },
                expectedTimedEvents: new[] { new TimedEvent[0] },
                filter: e => false);
        }

        [Test]
        public void SplitByNotes_Filter_MultipleEvents_Mixed_DifferentChannels_DifferentNoteNumbers()
        {
            SplitByNotes_Filter(
                timedEvents: new[]
                {
                    new TimedEvent(new NoteOnEvent()),
                    new TimedEvent(new TextEvent("A")),
                    new TimedEvent(new NoteOffEvent()),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
                    new TimedEvent(new TextEvent("B")),
                    new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue) { Channel = (FourBitNumber)5 }),
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
                        new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue)),
                        new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue)),
                    },
                    new[]
                    {
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new NoteOnEvent { Channel = (FourBitNumber)5 }),
                        new TimedEvent(new NoteOffEvent { Channel = (FourBitNumber)5 }),
                    },
                    new[]
                    {
                        new TimedEvent(new TextEvent("A")),
                        new TimedEvent(new NoteOnEvent((SevenBitNumber)30, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)5 }),
                        new TimedEvent(new NoteOffEvent((SevenBitNumber)30, SevenBitNumber.MinValue) { Channel = (FourBitNumber)5 }),
                    },
                },
                filter: e => !(e.Event is TextEvent textEvent) || textEvent.Text != "B");
        }

        [Test]
        public void SplitByNotes_Filter_MultipleEvents_Note_AllNoteNumbers_AllChannels()
        {
            SplitByNotes_Filter(
                timedEvents: FourBitNumber.Values.SelectMany(channel => SevenBitNumber.Values.SelectMany(noteNumber => new[]
                {
                    new TimedEvent(new NoteOnEvent(noteNumber, SevenBitNumber.MaxValue) { Channel = channel }),
                    new TimedEvent(new NoteOffEvent(noteNumber, SevenBitNumber.MinValue) { Channel = channel }),
                })).ToArray(),
                expectedTimedEvents: FourBitNumber.Values.Where(channel => channel < 5).SelectMany(channel => SevenBitNumber.Values.Select(noteNumber => new[]
                {
                    new TimedEvent(new NoteOnEvent(noteNumber, SevenBitNumber.MaxValue) { Channel = channel }),
                    new TimedEvent(new NoteOffEvent(noteNumber, SevenBitNumber.MinValue) { Channel = channel }),
                }).ToArray()).ToArray(),
                filter: e => e.Event is ChannelEvent channelEvent && channelEvent.Channel < 5);
        }

        #endregion

        #region Private methods

        private void SplitByNotes(
            ICollection<TimedEvent> timedEvents,
            ICollection<ICollection<TimedEvent>> expectedTimedEvents,
            TimeDivision timeDivision = null)
        {
            var midiFile = timedEvents.ToFile();
            if (timeDivision != null)
                midiFile.TimeDivision = timeDivision;

            var midiFilesByNotes = midiFile.SplitByNotes().ToList();

            Assert.AreEqual(expectedTimedEvents.Count, midiFilesByNotes.Count, "Invalid count of new files.");

            var expectedTimedEventsEnumerator = expectedTimedEvents.GetEnumerator();
            var newMidiFilesEnumerator = midiFilesByNotes.GetEnumerator();

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

        private void SplitByNotes_IgnoreChannel(
            ICollection<TimedEvent> timedEvents,
            ICollection<ICollection<TimedEvent>> expectedTimedEvents,
            bool ignoreChannel)
        {
            SplitByNotes_WithSettings(
                timedEvents,
                expectedTimedEvents,
                true,
                null,
                ignoreChannel);
        }

        private void SplitByNotes_CopyNonNoteEventsToEachFile(
            ICollection<TimedEvent> timedEvents,
            ICollection<ICollection<TimedEvent>> expectedTimedEvents,
            bool copyNonNoteEventsToEachFile)
        {
            SplitByNotes_WithSettings(
                timedEvents,
                expectedTimedEvents,
                copyNonNoteEventsToEachFile,
                null,
                false);
        }

        private void SplitByNotes_Filter(
            ICollection<TimedEvent> timedEvents,
            ICollection<ICollection<TimedEvent>> expectedTimedEvents,
            Predicate<TimedEvent> filter)
        {
            SplitByNotes_WithSettings(
                timedEvents,
                expectedTimedEvents,
                true,
                filter,
                false);
        }

        private void SplitByNotes_WithSettings(
            ICollection<TimedEvent> timedEvents,
            ICollection<ICollection<TimedEvent>> expectedTimedEvents,
            bool copyNonNoteEventsToEachFile,
            Predicate<TimedEvent> filter,
            bool ignoreChannel)
        {
            var midiFile = timedEvents.ToFile();

            var midiFilesByNotes = midiFile
                .SplitByNotes(new SplitFileByNotesSettings
                {
                    CopyNonNoteEventsToEachFile = copyNonNoteEventsToEachFile,
                    Filter = filter,
                    IgnoreChannel = ignoreChannel
                })
                .ToList();

            Assert.AreEqual(expectedTimedEvents.Count, midiFilesByNotes.Count, "Invalid count of new files.");

            var expectedTimedEventsEnumerator = expectedTimedEvents.GetEnumerator();
            var newMidiFilesEnumerator = midiFilesByNotes.GetEnumerator();

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
