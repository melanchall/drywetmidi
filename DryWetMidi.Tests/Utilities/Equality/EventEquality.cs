using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    internal static class EventEquality
    {
        private static class ArrayUtilities
        {
            public static bool Equals<T>(T[] array1, T[] array2)
            {
                if (ReferenceEquals(array1, array2))
                    return true;

                if (ReferenceEquals(array1, null) || ReferenceEquals(array2, null))
                    return false;

                if (array1.Length != array2.Length)
                    return false;

                return array1.SequenceEqual(array2);
            }
        }

        private static readonly Dictionary<Type, Func<MidiEvent, MidiEvent, bool>> _comparers =
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
                    return ArrayUtilities.Equals(((SequencerSpecificEvent)e1).Data, ((SequencerSpecificEvent)e2).Data);
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
                           timeSignatureEvent1.NumberOf32ndNotesPerBeat == timeSignatureEvent2.NumberOf32ndNotesPerBeat;
                },
                [typeof(UnknownMetaEvent)] = (e1, e2) =>
                {
                    return ArrayUtilities.Equals(((UnknownMetaEvent)e1).Data, ((UnknownMetaEvent)e2).Data);
                },
            };

        public static bool Equals(MidiEvent e1, MidiEvent e2)
        {
            if (ReferenceEquals(e1, e2))
                return true;

            if (ReferenceEquals(null, e1) || ReferenceEquals(null, e2))
                return false;

            if (e1.DeltaTime != e2.DeltaTime)
                return false;

            if (e1.GetType() != e2.GetType())
                return false;

            if (e1 is ChannelEvent)
            {
                var parametersField = typeof(ChannelEvent).GetField("_parameters", BindingFlags.Instance | BindingFlags.NonPublic);
                var e1Parameters = (SevenBitNumber[])parametersField.GetValue(e1);
                var e2Parameters = (SevenBitNumber[])parametersField.GetValue(e2);
                return e1Parameters.SequenceEqual(e2Parameters);
            }

            var sysExEvent1 = e1 as SysExEvent;
            if (sysExEvent1 != null)
            {
                var sysExEvent2 = e2 as SysExEvent;
                return sysExEvent1.Completed == sysExEvent2.Completed &&
                       ArrayUtilities.Equals(sysExEvent1.Data, sysExEvent2.Data);
            }

            var baseTextEvent = e1 as BaseTextEvent;
            if (baseTextEvent != null)
                return baseTextEvent.Text == ((BaseTextEvent)e2).Text;

            Func<MidiEvent, MidiEvent, bool> comparer;
            if (_comparers.TryGetValue(e1.GetType(), out comparer))
                return comparer(e1, e2);

            return true;
        }
    }
}
