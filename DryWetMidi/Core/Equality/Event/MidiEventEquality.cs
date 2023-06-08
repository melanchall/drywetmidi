using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Core
{
    internal static class MidiEventEquality
    {
        #region Constants

        private static readonly Dictionary<MidiEventType, Func<MidiEvent, MidiEvent, bool>> Comparers =
            new Dictionary<MidiEventType, Func<MidiEvent, MidiEvent, bool>>
            {
                // Meta

                [MidiEventType.ChannelPrefix] = (e1, e2) =>
                {
                    return ((ChannelPrefixEvent)e1).Channel == ((ChannelPrefixEvent)e2).Channel;
                },
                [MidiEventType.KeySignature] = (e1, e2) =>
                {
                    var keySignatureEvent1 = (KeySignatureEvent)e1;
                    var keySignatureEvent2 = (KeySignatureEvent)e2;
                    return keySignatureEvent1.Key == keySignatureEvent2.Key &&
                           keySignatureEvent1.Scale == keySignatureEvent2.Scale;
                },
                [MidiEventType.PortPrefix] = (e1, e2) =>
                {
                    return ((PortPrefixEvent)e1).Port == ((PortPrefixEvent)e2).Port;
                },
                [MidiEventType.SequenceNumber] = (e1, e2) =>
                {
                    return ((SequenceNumberEvent)e1).Number == ((SequenceNumberEvent)e2).Number;
                },
                [MidiEventType.SetTempo] = (e1, e2) =>
                {
                    return ((SetTempoEvent)e1).MicrosecondsPerQuarterNote == ((SetTempoEvent)e2).MicrosecondsPerQuarterNote;
                },
                [MidiEventType.SmpteOffset] = (e1, e2) =>
                {
                    var smpteOffsetEvent1 = (SmpteOffsetEvent)e1;
                    var smpteOffsetEvent2 = (SmpteOffsetEvent)e2;
                    return smpteOffsetEvent1.Format == smpteOffsetEvent2.Format &&
                           smpteOffsetEvent1.Hours == smpteOffsetEvent2.Hours &&
                           smpteOffsetEvent1.Minutes == smpteOffsetEvent2.Minutes &&
                           smpteOffsetEvent1.Seconds == smpteOffsetEvent2.Seconds &&
                           smpteOffsetEvent1.Frames == smpteOffsetEvent2.Frames &&
                           smpteOffsetEvent1.SubFrames == smpteOffsetEvent2.SubFrames;
                },
                [MidiEventType.TimeSignature] = (e1, e2) =>
                {
                    var timeSignatureEvent1 = (TimeSignatureEvent)e1;
                    var timeSignatureEvent2 = (TimeSignatureEvent)e2;
                    return timeSignatureEvent1.Numerator == timeSignatureEvent2.Numerator &&
                           timeSignatureEvent1.Denominator == timeSignatureEvent2.Denominator &&
                           timeSignatureEvent1.ClocksPerClick == timeSignatureEvent2.ClocksPerClick &&
                           timeSignatureEvent1.ThirtySecondNotesPerBeat == timeSignatureEvent2.ThirtySecondNotesPerBeat;
                },
                [MidiEventType.EndOfTrack] = (e1, e2) => true,

                // System common

                [MidiEventType.MidiTimeCode] = (e1, e2) =>
                {
                    var midiTimeCodeEvent1 = (MidiTimeCodeEvent)e1;
                    var midiTimeCodeEvent2 = (MidiTimeCodeEvent)e2;
                    return midiTimeCodeEvent1.Component == midiTimeCodeEvent2.Component &&
                           midiTimeCodeEvent1.ComponentValue == midiTimeCodeEvent2.ComponentValue;
                },
                [MidiEventType.SongPositionPointer] = (e1, e2) =>
                {
                    return ((SongPositionPointerEvent)e1).PointerValue == ((SongPositionPointerEvent)e2).PointerValue;
                },
                [MidiEventType.SongSelect] = (e1, e2) =>
                {
                    return ((SongSelectEvent)e1).Number == ((SongSelectEvent)e2).Number;
                },
                [MidiEventType.TuneRequest] = (e1, e2) => true
            };

        #endregion

        #region Methods

        public static bool Equals(MidiEvent midiEvent1, MidiEvent midiEvent2, MidiEventEqualityCheckSettings settings, out string message)
        {
            message = null;

            if (ReferenceEquals(midiEvent1, midiEvent2))
                return true;

            if (ReferenceEquals(null, midiEvent1) || ReferenceEquals(null, midiEvent2))
            {
                message = "One of events is null.";
                return false;
            }

            if (settings.CompareDeltaTimes)
            {
                var deltaTime1 = midiEvent1.DeltaTime;
                var deltaTime2 = midiEvent2.DeltaTime;

                if (deltaTime1 != deltaTime2)
                {
                    message = $"Delta-times are different ({deltaTime1} vs {deltaTime2}).";
                    return false;
                }
            }

            var type1 = midiEvent1.GetType();
            var type2 = midiEvent2.GetType();
            if (type1 != type2)
            {
                message = $"Types of events are different ({type1} vs {type2}).";
                return false;
            }

            if (midiEvent1 is SystemRealTimeEvent)
                return true;

            var channelEvent1 = midiEvent1 as ChannelEvent;
            if (channelEvent1 != null)
            {
                var channelEvent2 = (ChannelEvent)midiEvent2;

                var channel1 = channelEvent1.Channel;
                var channel2 = channelEvent2.Channel;

                if (channel1 != channel2)
                {
                    message = $"Channels of events are different ({channel1} vs {channel2}).";
                    return false;
                }

                if (channelEvent1._dataByte1 != channelEvent2._dataByte1)
                {
                    message = $"First data bytes of events are different ({channelEvent1._dataByte1} vs {channelEvent2._dataByte1}).";
                    return false;
                }

                if (channelEvent1._dataByte2 != channelEvent2._dataByte2)
                {
                    message = $"Second data bytes of events are different ({channelEvent1._dataByte2} vs {channelEvent2._dataByte2}).";
                    return false;
                }

                return true;
            }

            var sysExEvent1 = midiEvent1 as SysExEvent;
            if (sysExEvent1 != null)
            {
                var sysExEvent2 = (SysExEvent)midiEvent2;

                var completed1 = sysExEvent1.Completed;
                var completed2 = sysExEvent2.Completed;

                if (completed1 != completed2)
                {
                    message = $"'Completed' state of system exclusive events are different ({completed1} vs {completed2}).";
                    return false;
                }

                if (!ArrayUtilities.Equals(sysExEvent1.Data, sysExEvent2.Data))
                {
                    message = "System exclusive events data are different.";
                    return false;
                }

                return true;
            }

            var sequencerSpecificEvent1 = midiEvent1 as SequencerSpecificEvent;
            if (sequencerSpecificEvent1 != null)
            {
                var sequencerSpecificEvent2 = (SequencerSpecificEvent)midiEvent2;

                if (!ArrayUtilities.Equals(sequencerSpecificEvent1.Data, sequencerSpecificEvent2.Data))
                {
                    message = "Sequencer specific events data are different.";
                    return false;
                }

                return true;
            }

            var unknownMetaEvent1 = midiEvent1 as UnknownMetaEvent;
            if (unknownMetaEvent1 != null)
            {
                var unknownMetaEvent2 = (UnknownMetaEvent)midiEvent2;

                var statusByte1 = unknownMetaEvent1.StatusByte;
                var statusByte2 = unknownMetaEvent2.StatusByte;

                if (statusByte1 != statusByte2)
                {
                    message = $"Unknown meta events status bytes are different ({statusByte1} vs {statusByte2}).";
                    return false;
                }

                if (!ArrayUtilities.Equals(unknownMetaEvent1.Data, unknownMetaEvent2.Data))
                {
                    message = "Unknown meta events data are different.";
                    return false;
                }

                return true;
            }

            var baseTextEvent1 = midiEvent1 as BaseTextEvent;
            if (baseTextEvent1 != null)
            {
                var baseTextEvent2 = (BaseTextEvent)midiEvent2;

                var text1 = baseTextEvent1.Text;
                var text2 = baseTextEvent2.Text;

                if (!string.Equals(text1, text2, settings.TextComparison))
                {
                    message = $"Meta events texts are different ({text1} vs {text2}).";
                    return false;
                }

                return true;
            }

            Func<MidiEvent, MidiEvent, bool> comparer;
            if (Comparers.TryGetValue(midiEvent1.EventType, out comparer))
                return comparer(midiEvent1, midiEvent2);

            var result = midiEvent1.Equals(midiEvent2);
            if (!result)
                message = $"Events {midiEvent1} and {midiEvent2} are not equal by result of Equals call on first event.";

            return result;
        }

        #endregion
    }
}
