using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed class BytesToMidiEventConverterTests
    {
        #region Test methods

        [Test]
        public void Convert_StatusByte_DataBytes()
        {
            using (var bytesToMidiEventConverter = new BytesToMidiEventConverter())
            {
                Convert_StatusByte_DataBytes(
                    bytesToMidiEventConverter,
                    0x92,
                    new byte[] { 0x12, 0x56 },
                    new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x56)
                    {
                        Channel = (FourBitNumber)0x2
                    });
                Convert_StatusByte_DataBytes(
                    bytesToMidiEventConverter,
                    0xB3,
                    new byte[] { 0x23, 0x7F },
                    new ControlChangeEvent((SevenBitNumber)0x23, (SevenBitNumber)0x7F)
                    {
                        Channel = (FourBitNumber)0x3
                    });
            }
        }

        [Test]
        public void Convert_Bytes()
        {
            using (var bytesToMidiEventConverter = new BytesToMidiEventConverter())
            {
                Convert_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0x92, 0x12, 0x56 },
                    new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x56)
                    {
                        Channel = (FourBitNumber)0x2
                    });
                Convert_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0x90, 0x12, 0x00 },
                    new NoteOffEvent((SevenBitNumber)0x12, (SevenBitNumber)0x00)
                    {
                        Channel = (FourBitNumber)0x0
                    });
                Convert_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0xB3, 0x23, 0x7F },
                    new ControlChangeEvent((SevenBitNumber)0x23, (SevenBitNumber)0x7F)
                    {
                        Channel = (FourBitNumber)0x3
                    });
            }
        }

        [Test]
        public void Convert_Bytes_SilentNoteOnAsNoteOn()
        {
            using (var bytesToMidiEventConverter = new BytesToMidiEventConverter())
            {
                bytesToMidiEventConverter.SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOn;

                Convert_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0x92, 0x12, 0x00 },
                    new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x00)
                    {
                        Channel = (FourBitNumber)0x2
                    });
                Convert_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0xB3, 0x23, 0x7F },
                    new ControlChangeEvent((SevenBitNumber)0x23, (SevenBitNumber)0x7F)
                    {
                        Channel = (FourBitNumber)0x3
                    });
            }
        }

        [Test]
        public void Convert_Bytes_Offset_Length()
        {
            using (var bytesToMidiEventConverter = new BytesToMidiEventConverter())
            {
                var bytes = new byte[] { 0x92, 0x12, 0x56, 0xB3, 0x23, 0x7F };

                Convert_Bytes_Offset_Length(
                    bytesToMidiEventConverter,
                    bytes,
                    0,
                    3,
                    new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x56)
                    {
                        Channel = (FourBitNumber)0x2
                    });
                Convert_Bytes_Offset_Length(
                    bytesToMidiEventConverter,
                    bytes,
                    3,
                    3,
                    new ControlChangeEvent((SevenBitNumber)0x23, (SevenBitNumber)0x7F)
                    {
                        Channel = (FourBitNumber)0x3
                    });
            }
        }

        [Test]
        public void Convert_ReadDeltaTimes()
        {
            using (var bytesToMidiEventConverter = new BytesToMidiEventConverter())
            {
                bytesToMidiEventConverter.ReadDeltaTimes = true;

                Convert_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0x7A, 0x92, 0x12, 0x00 },
                    new NoteOffEvent((SevenBitNumber)0x12, (SevenBitNumber)0x00)
                    {
                        Channel = (FourBitNumber)0x2,
                        DeltaTime = 0x7A
                    });
            }
        }

        [Test]
        public void Convert_ReadDeltaTimes_Settings()
        {
            using (var bytesToMidiEventConverter = new BytesToMidiEventConverter())
            {
                bytesToMidiEventConverter.ReadDeltaTimes = true;
                bytesToMidiEventConverter.SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOn;

                Convert_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0x7A, 0x92, 0x12, 0x00 },
                    new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x00)
                    {
                        Channel = (FourBitNumber)0x2,
                        DeltaTime = 0x7A
                    });
            }
        }

        [Test]
        public void Convert_Bytes_BytesFormat_Device()
        {
            using (var bytesToMidiEventConverter = new BytesToMidiEventConverter())
            {
                bytesToMidiEventConverter.BytesFormat = BytesFormat.Device;

                Convert_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0x92, 0x12, 0x56 },
                    new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x56)
                    {
                        Channel = (FourBitNumber)0x2
                    });
                Convert_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0xF0, 0x29 }.Concat(Enumerable.Range(0, 40).Select(_ => (byte)0xA7)).Concat(new byte[] { 0xF7 }).ToArray(),
                    new NormalSysExEvent(new byte[] { 0x29 }.Concat(Enumerable.Range(0, 40).Select(_ => (byte)0xA7)).Concat(new byte[] { 0xF7 }).ToArray()));
                Convert_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0xF0, 0x79 }.Concat(Enumerable.Range(0, 120).Select(_ => (byte)0xA7)).Concat(new byte[] { 0xF7 }).ToArray(),
                    new NormalSysExEvent(new byte[] { 0x79 }.Concat(Enumerable.Range(0, 120).Select(_ => (byte)0xA7)).Concat(new byte[] { 0xF7 }).ToArray()));
                Convert_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0xF0, 0x63 }.Concat(Enumerable.Range(0, 98).Select(_ => (byte)0xA7)).Concat(new byte[] { 0xF7 }).ToArray(),
                    new NormalSysExEvent(new byte[] { 0x63 }.Concat(Enumerable.Range(0, 98).Select(_ => (byte)0xA7)).Concat(new byte[] { 0xF7 }).ToArray()));
                Convert_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0xF0, 0x63 }.Concat(Enumerable.Range(0, 98).Select(_ => (byte)0xA7)).Concat(new byte[] { 0xF7, 0x92, 0x12, 0x56 }).ToArray(),
                    new NormalSysExEvent(new byte[] { 0x63 }.Concat(Enumerable.Range(0, 98).Select(_ => (byte)0xA7)).Concat(new byte[] { 0xF7 }).ToArray()));
            }
        }

        [Test]
        public void Convert_Bytes_BytesFormat_File()
        {
            using (var bytesToMidiEventConverter = new BytesToMidiEventConverter())
            {
                bytesToMidiEventConverter.BytesFormat = BytesFormat.File;

                Convert_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0x92, 0x12, 0x56 },
                    new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x56)
                    {
                        Channel = (FourBitNumber)0x2
                    });
                Convert_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0xF0, 0x29 }.Concat(Enumerable.Range(0, 40).Select(_ => (byte)0xA7)).Concat(new byte[] { 0xF7 }).ToArray(),
                    new NormalSysExEvent(Enumerable.Range(0, 40).Select(_ => (byte)0xA7).Concat(new byte[] { 0xF7 }).ToArray()));
                Convert_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0xF0, 0x79 }.Concat(Enumerable.Range(0, 120).Select(_ => (byte)0xA7)).Concat(new byte[] { 0xF7 }).ToArray(),
                    new NormalSysExEvent(Enumerable.Range(0, 120).Select(_ => (byte)0xA7).Concat(new byte[] { 0xF7 }).ToArray()));
                Convert_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0xF0, 0x63 }.Concat(Enumerable.Range(0, 98).Select(_ => (byte)0xA7)).Concat(new byte[] { 0xF7 }).ToArray(),
                    new NormalSysExEvent(Enumerable.Range(0, 98).Select(_ => (byte)0xA7).Concat(new byte[] { 0xF7 }).ToArray()));
                Convert_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0xF0, 0x63 }.Concat(Enumerable.Range(0, 98).Select(_ => (byte)0xA7)).Concat(new byte[] { 0xF7, 0x92, 0x12, 0x56 }).ToArray(),
                    new NormalSysExEvent(Enumerable.Range(0, 98).Select(_ => (byte)0xA7).Concat(new byte[] { 0xF7 }).ToArray()));
            }
        }

        [Test]
        public void ConvertMultiple_Bytes()
        {
            using (var bytesToMidiEventConverter = new BytesToMidiEventConverter())
            {
                ConvertMultiple_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0x92, 0x12, 0x56, 0x90, 0x12, 0x00, 0xB3, 0x23, 0x7F },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x56)
                        {
                            Channel = (FourBitNumber)0x2
                        },
                        new NoteOffEvent((SevenBitNumber)0x12, (SevenBitNumber)0x00)
                        {
                            Channel = (FourBitNumber)0x0
                        },
                        new ControlChangeEvent((SevenBitNumber)0x23, (SevenBitNumber)0x7F)
                        {
                            Channel = (FourBitNumber)0x3
                        }
                    });
            }
        }

        [Test]
        public void ConvertMultiple_Bytes_BytesFormat_Device()
        {
            using (var bytesToMidiEventConverter = new BytesToMidiEventConverter())
            {
                bytesToMidiEventConverter.BytesFormat = BytesFormat.Device;

                ConvertMultiple_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0x92, 0x12, 0x56 }
                        .Concat(new byte[] { 0xF0, 0x29 })
                        .Concat(Enumerable.Range(0, 40).Select(_ => (byte)0xA7))
                        .Concat(new byte[] { 0xF7 })
                        .Concat(new byte[] { 0xF0, 0x79 })
                        .Concat(Enumerable.Range(0, 120).Select(_ => (byte)0xA7))
                        .Concat(new byte[] { 0xF7 })
                        .ToArray(),
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x56)
                        {
                            Channel = (FourBitNumber)0x2
                        },
                        new NormalSysExEvent(new byte[] { 0x29 }.Concat(Enumerable.Range(0, 40).Select(_ => (byte)0xA7)).Concat(new byte[] { 0xF7 }).ToArray()),
                        new NormalSysExEvent(new byte[] { 0x79 }.Concat(Enumerable.Range(0, 120).Select(_ => (byte)0xA7)).Concat(new byte[] { 0xF7 }).ToArray())
                    });
            }
        }

        [Test]
        public void ConvertMultiple_Bytes_BytesFormat_File()
        {
            using (var bytesToMidiEventConverter = new BytesToMidiEventConverter())
            {
                bytesToMidiEventConverter.BytesFormat = BytesFormat.File;

                ConvertMultiple_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0x92, 0x12, 0x56 }
                        .Concat(new byte[] { 0xF0, 0x29 })
                        .Concat(Enumerable.Range(0, 40).Select(_ => (byte)0xA7))
                        .Concat(new byte[] { 0xF7 })
                        .Concat(new byte[] { 0xF0, 0x79 })
                        .Concat(Enumerable.Range(0, 120).Select(_ => (byte)0xA7))
                        .Concat(new byte[] { 0xF7 })
                        .ToArray(),
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x56)
                        {
                            Channel = (FourBitNumber)0x2
                        },
                        new NormalSysExEvent(Enumerable.Range(0, 40).Select(_ => (byte)0xA7).Concat(new byte[] { 0xF7 }).ToArray()),
                        new NormalSysExEvent(Enumerable.Range(0, 120).Select(_ => (byte)0xA7).Concat(new byte[] { 0xF7 }).ToArray())
                    });
            }
        }

        [Test]
        public void ConvertMultiple_Bytes_DeltaTimes()
        {
            using (var bytesToMidiEventConverter = new BytesToMidiEventConverter())
            {
                bytesToMidiEventConverter.ReadDeltaTimes = true;

                ConvertMultiple_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0x30, 0x92, 0x12, 0x56, 0x81, 0x00, 0x90, 0x12, 0x00, 0x00, 0xB3, 0x23, 0x7F },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x56)
                        {
                            Channel = (FourBitNumber)0x2,
                            DeltaTime = 0x30
                        },
                        new NoteOffEvent((SevenBitNumber)0x12, (SevenBitNumber)0x00)
                        {
                            Channel = (FourBitNumber)0x0,
                            DeltaTime = 128
                        },
                        new ControlChangeEvent((SevenBitNumber)0x23, (SevenBitNumber)0x7F)
                        {
                            Channel = (FourBitNumber)0x3
                        }
                    });
            }
        }

        [Test]
        public void ConvertMultiple_Bytes_RunningStatus()
        {
            using (var bytesToMidiEventConverter = new BytesToMidiEventConverter())
            {
                ConvertMultiple_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0x92, 0x12, 0x56, 0x12, 0x65, 0x90, 0x12, 0x00, 0xB3, 0x23, 0x7F },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x56)
                        {
                            Channel = (FourBitNumber)0x2
                        },
                        new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x65)
                        {
                            Channel = (FourBitNumber)0x2
                        },
                        new NoteOffEvent((SevenBitNumber)0x12, (SevenBitNumber)0x00)
                        {
                            Channel = (FourBitNumber)0x0
                        },
                        new ControlChangeEvent((SevenBitNumber)0x23, (SevenBitNumber)0x7F)
                        {
                            Channel = (FourBitNumber)0x3
                        }
                    });
            }
        }

        [Test]
        public void ConvertMultiple_Bytes_IncompleteEvent_Abort()
        {
            using (var bytesToMidiEventConverter = new BytesToMidiEventConverter())
            {
                Assert.Throws<NotEnoughBytesException>(
                    () => bytesToMidiEventConverter.ConvertMultiple(new byte[] { 0x92, 0x12, 0x56, 0x90 }),
                    "Exception is not thrown.");
            }
        }

        [Test]
        public void ConvertMultiple_Bytes_IncompleteEvent_Ignore()
        {
            using (var bytesToMidiEventConverter = new BytesToMidiEventConverter())
            {
                bytesToMidiEventConverter.NotEnoughBytesPolicy = NotEnoughBytesPolicy.Ignore;

                ConvertMultiple_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0x92, 0x12, 0x56, 0x90, 0x12, 0x00, 0xB3 },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x56)
                        {
                            Channel = (FourBitNumber)0x2
                        },
                        new NoteOffEvent((SevenBitNumber)0x12, (SevenBitNumber)0x00)
                        {
                            Channel = (FourBitNumber)0x0
                        }
                    });
            }
        }

        [Test]
        public void ConvertMultipleThenOne_Bytes()
        {
            using (var bytesToMidiEventConverter = new BytesToMidiEventConverter())
            {
                ConvertMultiple_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0x92, 0x12, 0x56, 0x90, 0x12, 0x00, 0xB3, 0x23, 0x7F },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x56)
                        {
                            Channel = (FourBitNumber)0x2
                        },
                        new NoteOffEvent((SevenBitNumber)0x12, (SevenBitNumber)0x00)
                        {
                            Channel = (FourBitNumber)0x0
                        },
                        new ControlChangeEvent((SevenBitNumber)0x23, (SevenBitNumber)0x7F)
                        {
                            Channel = (FourBitNumber)0x3
                        }
                    });

                Convert_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0x92, 0x12, 0x56 },
                    new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x56)
                    {
                        Channel = (FourBitNumber)0x2
                    });

                Convert_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0x90, 0x12, 0x00 },
                    new NoteOffEvent((SevenBitNumber)0x12, (SevenBitNumber)0x00)
                    {
                        Channel = (FourBitNumber)0x0
                    });

                Convert_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0xB3, 0x23, 0x7F },
                    new ControlChangeEvent((SevenBitNumber)0x23, (SevenBitNumber)0x7F)
                    {
                        Channel = (FourBitNumber)0x3
                    });
            }
        }

        [Test]
        public void ConvertMultiple_Bytes_ShortAfterLong()
        {
            using (var bytesToMidiEventConverter = new BytesToMidiEventConverter())
            {
                ConvertMultiple_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0x92, 0x12, 0x56, 0x90, 0x12, 0x00, 0xB3, 0x23, 0x7F },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)0x12, (SevenBitNumber)0x56)
                        {
                            Channel = (FourBitNumber)0x2
                        },
                        new NoteOffEvent((SevenBitNumber)0x12, (SevenBitNumber)0x00)
                        {
                            Channel = (FourBitNumber)0x0
                        },
                        new ControlChangeEvent((SevenBitNumber)0x23, (SevenBitNumber)0x7F)
                        {
                            Channel = (FourBitNumber)0x3
                        }
                    });

                ConvertMultiple_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0xB4, 0x23, 0x7A },
                    new MidiEvent[]
                    {
                        new ControlChangeEvent((SevenBitNumber)0x23, (SevenBitNumber)0x7A)
                        {
                            Channel = (FourBitNumber)0x4
                        }
                    });
            }
        }

        [Test]
        public void Convert_Bytes_ResetEvent()
        {
            using (var bytesToMidiEventConverter = new BytesToMidiEventConverter())
            {
                bytesToMidiEventConverter.BytesFormat = BytesFormat.Device;

                Convert_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0xFF },
                    new ResetEvent());
            }
        }

        [Test]
        public void Convert_Bytes_MetaEvent()
        {
            using (var bytesToMidiEventConverter = new BytesToMidiEventConverter())
            {
                bytesToMidiEventConverter.BytesFormat = BytesFormat.File;

                Convert_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] { 0xFF, 0x2F, 0x00 },
                    new EndOfTrackEvent());
            }
        }

        [Test]
        public void Convert_Bytes_StackOverflow()
        {
            using (var bytesToMidiEventConverter = new BytesToMidiEventConverter())
            {
                ConvertMultiple_Bytes(
                    bytesToMidiEventConverter,
                    new byte[] {
                        0xF5 , 0x02,
                        0x90, 0x3C, 0x7F,
                        0x40, 0x7F,
                        0x43, 0x7F,
                        0x80, 0x3C, 0x00,
                        0x40, 0x00,
                        0x43, 0x00
                    },
                    new MidiEvent[]
                    {
                        new SelectPartGroupEvent(2),
                        new NoteOnEvent((SevenBitNumber)0x3C, (SevenBitNumber)0x7F),
                        new NoteOnEvent((SevenBitNumber)0x40, (SevenBitNumber)0x7F),
                        new NoteOnEvent((SevenBitNumber)0x43, (SevenBitNumber)0x7F),
                        new NoteOffEvent((SevenBitNumber)0x3C, (SevenBitNumber)0x00),
                        new NoteOffEvent((SevenBitNumber)0x40, (SevenBitNumber)0x00),
                        new NoteOffEvent((SevenBitNumber)0x43, (SevenBitNumber)0x00),
                    });
            }
        }

        #endregion

        #region Private methods

        private void Convert_StatusByte_DataBytes(BytesToMidiEventConverter bytesToMidiEventConverter, byte statusByte, byte[] dataBytes, MidiEvent expectedMidiEvent)
        {
            var midiEvent = bytesToMidiEventConverter.Convert(statusByte, dataBytes);
            CompareEvents(expectedMidiEvent, midiEvent);
        }

        private void Convert_Bytes(BytesToMidiEventConverter bytesToMidiEventConverter, byte[] bytes, MidiEvent expectedMidiEvent)
        {
            var midiEvent = bytesToMidiEventConverter.Convert(bytes);
            CompareEvents(expectedMidiEvent, midiEvent);
        }

        private void ConvertMultiple_Bytes(BytesToMidiEventConverter bytesToMidiEventConverter, byte[] bytes, ICollection<MidiEvent> expectedMidiEvents)
        {
            var midiEvents = bytesToMidiEventConverter.ConvertMultiple(bytes);
            CompareEvents(expectedMidiEvents, midiEvents);
        }

        private void Convert_Bytes_Offset_Length(BytesToMidiEventConverter bytesToMidiEventConverter, byte[] bytes, int offset, int length, MidiEvent expectedMidiEvent)
        {
            var midiEvent = bytesToMidiEventConverter.Convert(bytes, offset, length);
            CompareEvents(expectedMidiEvent, midiEvent);
        }

        private static void CompareEvents(MidiEvent expectedMidiEvent, MidiEvent actualMidiEvent)
        {
            MidiAsserts.AreEqual(
                expectedMidiEvent,
                actualMidiEvent,
                true,
                "MIDI event read incorrectly.");
        }

        private static void CompareEvents(ICollection<MidiEvent> expectedMidiEvents, ICollection<MidiEvent> actualMidiEvents)
        {
            MidiAsserts.AreEqual(
                expectedMidiEvents,
                actualMidiEvents,
                true,
                "MIDI event read incorrectly.");
        }

        #endregion
    }
}
