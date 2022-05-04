using Melanchall.DryWetMidi.Interaction;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Tools
{
    public sealed class PartProcessingContext
    {
        #region Properties

        public IEnumerable<ITimedObject> SourceObjects { get; internal set; }
        
        public IList<ITimedObject> PartObjects { get; internal set; }
        
        public int PartIndex { get; internal set; }
        
        public long Shift { get; internal set; }
        
        public TempoMap SourceTempoMap { get; internal set; }
        
        public RepeatingSettings Settings { get; internal set; }

        internal TimedEvent SourceFirstSetTempoEvent { get; set; }

        internal TimedEvent SourceFirstTimeSignatureEvent { get; set; }

        #endregion
    }
}
