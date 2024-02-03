using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Multimedia
{
    public sealed class MidiEventRecordedEventArgs
    {
        #region Constructor

        internal MidiEventRecordedEventArgs(MidiEvent midiEvent)
        {
            ThrowIfArgument.IsNull(nameof(midiEvent), midiEvent);

            Event = midiEvent;
        }

        #endregion

        #region Properties

        public MidiEvent Event { get; }

        #endregion
    }
}
