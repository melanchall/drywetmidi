using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Tools
{
    public static partial class Sanitizer
    {
        /// <summary>
        /// Asynchronously sanitizes a MIDI file.
        /// </summary>
        /// <inheritdoc cref="Sanitize(MidiFile, SanitizingSettings)" path="/member/exception"/>
        /// <param name="midiFile">File to sanitize.</param>
        /// <param name="settings">Sanitizing settings.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the sanitization.</returns>
        /// <exception cref="InvalidOperationException">Completing an orphaned note requires a maximum note length,
        /// but <see cref="SanitizingSettings.NoteMaxLengthForOrphanedNoteOnEvent"/> is null.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task SanitizeAsync(
            this MidiFile midiFile,
            SanitizingSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => Sanitize(midiFile, settings), cancellationToken, progress);
    }
}
