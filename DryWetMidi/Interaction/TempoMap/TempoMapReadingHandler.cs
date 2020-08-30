using System;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Collects tempo map during MIDI data reading.
    /// </summary>
    /// <remarks>
    /// This handler can be added to the <see cref="ReadingSettings.ReadingHandlers"/> collection to
    /// collect tempo map along with MIDI data reading. Scope of the handler is <c>TargetScope.Event | TargetScope.File</c>.
    /// </remarks>
    public sealed class TempoMapReadingHandler : ReadingHandler
    {
        #region Fields

        private TempoMap _tempoMap;
        private bool _tempoMapIsReadyForUsage;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TempoMapReadingHandler"/>.
        /// </summary>
        public TempoMapReadingHandler()
            : base(TargetScope.Event | TargetScope.File)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the tempo map collected during MIDI data reading.
        /// </summary>
        /// <exception cref="InvalidOperationException">Tempo map is not ready for usage.</exception>
        public TempoMap TempoMap
        {
            get
            {
                if (!_tempoMapIsReadyForUsage)
                    throw new InvalidOperationException("Tempo map is not ready for usage.");

                return _tempoMap;
            }
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Initializes handler. This method will be called before reading MIDI data.
        /// </summary>
        public override void Initialize()
        {
            _tempoMapIsReadyForUsage = false;
            _tempoMap = null;
        }

        /// <summary>
        /// Handles finish of header chunk reading. Called after header chunk is read.
        /// </summary>
        /// <param name="timeDivision">Time division of the file is being read.</param>
        public override void OnFinishHeaderChunkReading(TimeDivision timeDivision)
        {
            _tempoMap = new TempoMap(timeDivision)
            {
                IsTempoMapReady = false
            };
        }

        /// <summary>
        /// Handles finish of file reading. Called after file is read.
        /// </summary>
        /// <param name="midiFile">MIDI file read.</param>
        public override void OnFinishFileReading(MidiFile midiFile)
        {
            _tempoMap.IsTempoMapReady = true;
            _tempoMapIsReadyForUsage = true;
        }

        /// <summary>
        /// Handles finish of MIDI event reading. Called after MIDI event is read and before
        /// putting it to <see cref="TrackChunk.Events"/> collection.
        /// </summary>
        /// <param name="midiEvent">MIDI event read.</param>
        /// <param name="absoluteTime">Absolute time of <paramref name="midiEvent"/>.</param>
        public override void OnFinishEventReading(MidiEvent midiEvent, long absoluteTime)
        {
            switch (midiEvent.EventType)
            {
                case MidiEventType.SetTempo:
                    var setTempoEvent = (SetTempoEvent)midiEvent;
                    _tempoMap.TempoLine.SetValue(absoluteTime, new Tempo(setTempoEvent.MicrosecondsPerQuarterNote));
                    break;
                case MidiEventType.TimeSignature:
                    var timeSignatureEvent = (TimeSignatureEvent)midiEvent;
                    _tempoMap.TimeSignatureLine.SetValue(absoluteTime, new TimeSignature(timeSignatureEvent.Numerator, timeSignatureEvent.Denominator));
                    break;
            }
        }

        #endregion
    }
}
