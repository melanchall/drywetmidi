using Melanchall.DryWetMidi.Interaction;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Holds all the required data to process a part by the <see cref="Repeater"/>. More info in the
    /// <see href="xref:a_repeater#custom-repeater">Repeater: Custom repeater</see> article.
    /// </summary>
    public sealed class PartProcessingContext
    {
        #region Properties

        /// <summary>
        /// Gets source objects that should be repeated.
        /// </summary>
        public IEnumerable<ITimedObject> SourceObjects { get; internal set; }
        
        /// <summary>
        /// Gets the objects of a new part that should be appended to the previous ones.
        /// </summary>
        public IList<ITimedObject> PartObjects { get; internal set; }
        
        /// <summary>
        /// Gets the index of a part (zero means first new part).
        /// </summary>
        public int PartIndex { get; internal set; }
        
        /// <summary>
        /// Gets calculated shift value to apply to a part.
        /// </summary>
        public long Shift { get; internal set; }
        
        /// <summary>
        /// Gets the source tempo map.
        /// </summary>
        public TempoMap SourceTempoMap { get; internal set; }
        
        /// <summary>
        /// Gets the settings used to perform the process.
        /// </summary>
        public RepeatingSettings Settings { get; internal set; }

        internal TimedEvent SourceFirstSetTempoEvent { get; set; }

        internal TimedEvent SourceFirstTimeSignatureEvent { get; set; }

        #endregion
    }
}
