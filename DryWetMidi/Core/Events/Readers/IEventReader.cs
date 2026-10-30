using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Core
{
    internal interface IEventReader
    {
        #region Methods

        MidiEvent? Read(MidiReader reader, ReadingSettings settings, byte currentStatusByte);

        Task<MidiEvent?> ReadAsync(
            MidiReader reader,
            ReadingSettings settings,
            byte currentStatusByte,
            CancellationToken cancellationToken);

        #endregion
    }
}
