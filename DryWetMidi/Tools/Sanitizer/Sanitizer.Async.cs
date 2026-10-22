using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Tools
{
    public static partial class Sanitizer
    {
        /// <summary>Asynchronously sanitizes a MIDI file.</summary>
        public static Task SanitizeAsync(this MidiFile midiFile, SanitizingSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => Sanitize(midiFile, settings), cancellationToken, progress);
    }
}
