using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    internal static class MidiEventEquality
    {
        #region Nested classes

        public sealed class EqualityComparer : IEqualityComparer<MidiEvent>
        {
            private readonly bool _compareDeltaTimes;

            public EqualityComparer(bool compareDeltaTimes)
            {
                _compareDeltaTimes = compareDeltaTimes;
            }

            public bool Equals(MidiEvent x, MidiEvent y)
            {
                return AreEqual(x, y, _compareDeltaTimes);
            }

            public int GetHashCode(MidiEvent obj)
            {
                return 0;
            }
        }

        #endregion

        #region Constants

        private static readonly Dictionary<Type, Func<MidiEvent, MidiEvent, bool>> Comparers =
            new Dictionary<Type, Func<MidiEvent, MidiEvent, bool>>
            {
                [typeof(ChannelPrefixEvent)] = (e1, e2) =>
                {
                    return ((ChannelPrefixEvent)e1).Channel == ((ChannelPrefixEvent)e2).Channel;
                },
                [typeof(KeySignatureEvent)] = (e1, e2) =>
                {
                    var keySignatureEvent1 = (KeySignatureEvent)e1;
                    var keySignatureEvent2 = (KeySignatureEvent)e2;
                    return keySignatureEvent1.Key == keySignatureEvent2.Key &&
                           keySignatureEvent1.Scale == keySignatureEvent2.Scale;
                },
                [typeof(PortPrefixEvent)] = (e1, e2) =>
                {
                    return ((PortPrefixEvent)e1).Port == ((PortPrefixEvent)e2).Port;
                },
                [typeof(SequenceNumberEvent)] = (e1, e2) =>
                {
                    return ((SequenceNumberEvent)e1).Number == ((SequenceNumberEvent)e2).Number;
                },
                [typeof(SequencerSpecificEvent)] = (e1, e2) =>
                {
                    return ArrayEquality.AreEqual(((SequencerSpecificEvent)e1).Data, ((SequencerSpecificEvent)e2).Data);
                },
                [typeof(SetTempoEvent)] = (e1, e2) =>
                {
                    return ((SetTempoEvent)e1).MicrosecondsPerQuarterNote == ((SetTempoEvent)e2).MicrosecondsPerQuarterNote;
                },
                [typeof(SmpteOffsetEvent)] = (e1, e2) =>
                {
                    var smpteOffsetEvent1 = (SmpteOffsetEvent)e1;
                    var smpteOffsetEvent2 = (SmpteOffsetEvent)e2;
                    return smpteOffsetEvent1.Hours == smpteOffsetEvent2.Hours &&
                           smpteOffsetEvent1.Minutes == smpteOffsetEvent2.Minutes &&
                           smpteOffsetEvent1.Seconds == smpteOffsetEvent2.Seconds &&
                           smpteOffsetEvent1.Frames == smpteOffsetEvent2.Frames &&
                           smpteOffsetEvent1.SubFrames == smpteOffsetEvent2.SubFrames;
                },
                [typeof(TimeSignatureEvent)] = (e1, e2) =>
                {
                    var timeSignatureEvent1 = (TimeSignatureEvent)e1;
                    var timeSignatureEvent2 = (TimeSignatureEvent)e2;
                    return timeSignatureEvent1.Numerator == timeSignatureEvent2.Numerator &&
                           timeSignatureEvent1.Denominator == timeSignatureEvent2.Denominator &&
                           timeSignatureEvent1.ClocksPerClick == timeSignatureEvent2.ClocksPerClick &&
                           timeSignatureEvent1.ThirtySecondNotesPerBeat == timeSignatureEvent2.ThirtySecondNotesPerBeat;
                },
                [typeof(UnknownMetaEvent)] = (e1, e2) =>
                {
                    return ArrayEquality.AreEqual(((UnknownMetaEvent)e1).Data, ((UnknownMetaEvent)e2).Data);
                },

                [typeof(MidiTimeCodeEvent)] = (e1, e2) =>
                {
                    var midiTimeCodeEvent1 = (MidiTimeCodeEvent)e1;
                    var midiTimeCodeEvent2 = (MidiTimeCodeEvent)e2;
                    return midiTimeCodeEvent1.Component == midiTimeCodeEvent2.Component &&
                           midiTimeCodeEvent1.ComponentValue == midiTimeCodeEvent2.ComponentValue;
                },
                [typeof(SongPositionPointerEvent)] = (e1, e2) =>
                {
                    return ((SongPositionPointerEvent)e1).PointerValue == ((SongPositionPointerEvent)e2).PointerValue;
                },
                [typeof(SongSelectEvent)] = (e1, e2) =>
                {
                    return ((SongSelectEvent)e1).Number == ((SongSelectEvent)e2).Number;
                },
                [typeof(TuneRequestEvent)] = (e1, e2) => true
            };

        #endregion

        #region Methods

        public static bool AreEqual(MidiEvent event1, MidiEvent event2, bool compareDeltaTimes)
        {
            if (ReferenceEquals(event1, event2))
                return true;

            if (ReferenceEquals(null, event1) || ReferenceEquals(null, event2))
                return false;

            if (compareDeltaTimes && event1.DeltaTime != event2.DeltaTime)
                return false;

            if (event1.GetType() != event2.GetType())
                return false;

            if (event1 is SystemRealTimeEvent)
                return true;

            if (event1 is ChannelEvent)
            {
                var parametersField = typeof(ChannelEvent).GetField("_parameters", BindingFlags.Instance | BindingFlags.NonPublic);
                var e1Parameters = (SevenBitNumber[])parametersField.GetValue(event1);
                var e2Parameters = (SevenBitNumber[])parametersField.GetValue(event2);
                return e1Parameters.SequenceEqual(e2Parameters) && ((ChannelEvent)event1).Channel == ((ChannelEvent)event2).Channel;
            }

            var sysExEvent1 = event1 as SysExEvent;
            if (sysExEvent1 != null)
            {
                var sysExEvent2 = event2 as SysExEvent;
                return sysExEvent1.Completed == sysExEvent2.Completed &&
                       ArrayEquality.AreEqual(sysExEvent1.Data, sysExEvent2.Data);
            }

            var baseTextEvent = event1 as BaseTextEvent;
            if (baseTextEvent != null)
                return baseTextEvent.Text == ((BaseTextEvent)event2).Text;

            Func<MidiEvent, MidiEvent, bool> comparer;
            if (Comparers.TryGetValue(event1.GetType(), out comparer))
                return comparer(event1, event2);

            throw new NotImplementedException("Events comparing is not implemented.");
        }

        #endregion
    }
}
