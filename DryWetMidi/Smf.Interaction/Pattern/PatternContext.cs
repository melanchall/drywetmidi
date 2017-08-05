using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class PatternContext
    {
        #region Fields

        private readonly Stack<long> _timeHistory = new Stack<long>();
        private readonly Dictionary<object, List<long>> _anchors = new Dictionary<object, List<long>>();
        private readonly List<long> _anchorsList = new List<long>();
        private readonly List<Note> _notes = new List<Note>();

        #endregion

        #region Constructor

        public PatternContext(TempoMap tempoMap, FourBitNumber channel)
        {
            TempoMap = tempoMap;
            Channel = channel;
        }

        #endregion

        #region Properties

        public TempoMap TempoMap { get; }

        public FourBitNumber Channel { get; }

        //public SevenBitNumber DefaultVelocity { get; set; } = Note.DefaultVelocity;

        //public ILength DefaultNoteLength { get; set; } = (MusicalLength)MusicalFraction.Quarter;

        //public ILength DefaultStep { get; set; } = (MusicalLength)MusicalFraction.Quarter;

        //public OctaveDefinition DefaultOctave { get; set; } = OctaveDefinition.Get(4);

        #endregion

        #region Methods

        public void SaveTime(long time)
        {
            _timeHistory.Push(time);
        }

        public long? RestoreTime()
        {
            return _timeHistory.Any() ?
                (long?)_timeHistory.Pop()
                : null;
        }

        public void AnchorTime(object anchor, long time)
        {
            GetAnchorTimesList(anchor).Add(time);

            if (anchor != null)
                _anchorsList.Add(time);
        }

        public IReadOnlyList<long> GetAnchorTimes(object anchor)
        {
            return GetAnchorTimesList(anchor).AsReadOnly();
        }

        private List<long> GetAnchorTimesList(object anchor)
        {
            if (anchor == null)
                return _anchorsList;

            List<long> times;
            if (!_anchors.TryGetValue(anchor, out times))
                _anchors.Add(anchor, times = new List<long>());

            return times;
        }

        #endregion
    }
}
