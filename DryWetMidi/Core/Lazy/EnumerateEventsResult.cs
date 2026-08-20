using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents a result of the <see cref="MidiTokensReaderUtilities.EnumerateEvents(MidiTokensReader)"/>
    /// method.
    /// </summary>
    public sealed class EnumerateEventsResult
    {
        #region Properties

        /// <summary>
        /// Gets a lazy collection of MIDI events.
        /// </summary>
        public IEnumerable<MidiEvent> Events { get; internal set; } = Enumerable.Empty<MidiEvent>();

        /// <summary>
        /// Gets a MIDI token following the last MIDI event iterated by the
        /// <see cref="MidiTokensReaderUtilities.EnumerateEvents(MidiTokensReader)"/> method.
        /// </summary>
        public MidiToken? NextToken { get; internal set; }

        #endregion
    }
}
