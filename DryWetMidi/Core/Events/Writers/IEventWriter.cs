using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Core
{
    internal interface IEventWriter
    {
        #region Methods

        void Write(MidiEvent midiEvent, MidiWriter writer, WritingSettings settings, bool writeStatusByte);

        Task WriteAsync(
            MidiEvent midiEvent,
            MidiWriter writer,
            WritingSettings settings,
            bool writeStatusByte,
            CancellationToken cancellationToken);

        int CalculateSize(MidiEvent midiEvent, WritingSettings settings, bool writeStatusByte);

        byte GetStatusByte(MidiEvent midiEvent);

        #endregion
    }
}
