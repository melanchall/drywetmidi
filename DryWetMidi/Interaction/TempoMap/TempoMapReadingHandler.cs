using System;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    public sealed class TempoMapReadingHandler : ReadingHandler
    {
        #region Fields

        private TempoMap _tempoMap;
        private bool _tempoMapIsReadyForUsage;

        #endregion

        #region Constructor

        public TempoMapReadingHandler()
            : base(TargetScope.Event | TargetScope.File)
        {
        }

        #endregion

        #region Properties

        public TempoMap TempoMap
        {
            get
            {
                if (!_tempoMapIsReadyForUsage)
                    throw new InvalidOperationException("Tempo map is not ready for usage.");

                return _tempoMap;
            }
            private set { _tempoMap = value; }
        }

        #endregion

        #region Overrides

        public override void Initialize()
        {
            _tempoMapIsReadyForUsage = false;
            _tempoMap = null;
        }

        public override void OnFinishHeaderChunkReading(TimeDivision timeDivision)
        {
            _tempoMap = new TempoMap(timeDivision)
            {
                IsTempoMapReady = false
            };
        }

        public override void OnFinishFileReading(MidiFile midiFile)
        {
            _tempoMap.IsTempoMapReady = true;
            _tempoMapIsReadyForUsage = true;
        }

        public override void OnFinishEventReading(MidiEvent midiEvent, long absoluteTime)
        {
            switch (midiEvent.EventType)
            {
                case MidiEventType.SetTempo:
                    var setTempoEvent = (SetTempoEvent)midiEvent;
                    _tempoMap.Tempo.SetValue(absoluteTime, new Tempo(setTempoEvent.MicrosecondsPerQuarterNote));
                    break;
                case MidiEventType.TimeSignature:
                    var timeSignatureEvent = (TimeSignatureEvent)midiEvent;
                    _tempoMap.TimeSignature.SetValue(absoluteTime, new TimeSignature(timeSignatureEvent.Numerator, timeSignatureEvent.Denominator));
                    break;
            }
        }

        #endregion
    }
}
