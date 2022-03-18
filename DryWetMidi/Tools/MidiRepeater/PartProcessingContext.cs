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
        
        public MidiRepeaterSettings Settings { get; internal set; }

        #endregion
    }
}
