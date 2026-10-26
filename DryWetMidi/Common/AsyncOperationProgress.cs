namespace Melanchall.DryWetMidi.Common
{
    /// <summary>
    /// Describes progress of an asynchronous operation. Item counts are operation-specific,
    /// and a total is not inferred by enumerating an input sequence.
    /// </summary>
    /// <remarks>
    /// MIDI I/O reports bytes read from or written to the transport. CSV I/O reports
    /// work checkpoints for records and object processing, not bytes or a reliable count
    /// of processed notes. Timed-object writers report accepted input objects, not emitted
    /// MIDI events. Reports are throttled, so
    /// not every checkpoint produces a notification. Successful completion produces a
    /// final report with <see cref="IsCompleted"/> set to <c>true</c>.
    /// </remarks>
    /// <example>
    /// The following asynchronous method reads a complete MIDI file, then writes it
    /// to a different path, reporting transported bytes separately for each operation:
    /// <code language="csharp"><![CDATA[
    /// using System;
    /// using System.Threading.Tasks;
    /// using Melanchall.DryWetMidi.Common;
    /// using Melanchall.DryWetMidi.Core;
    ///
    /// static async Task CopyMidiAsync(string inputPath, string outputPath)
    /// {
    ///     var progress = new Progress<AsyncOperationProgress>(value =>
    ///     {
    ///         Console.WriteLine(
    ///             $"{value.Phase}: {value.ProcessedItems} bytes; completed: {value.IsCompleted}");
    ///     });
    ///
    ///     var file = await MidiFile.ReadAsync(inputPath, progress: progress);
    ///     await file.WriteAsync(outputPath, progress: progress);
    /// }
    /// ]]></code>
    /// Each operation starts its count at zero and leaves <see cref="TotalItems"/>
    /// unknown. These bulk APIs do not yield MIDI objects lazily. A
    /// <c>Progress&lt;T&gt;</c> handler can run asynchronously after a report is issued;
    /// await the operation itself to determine when it has finished.
    /// </example>
    public sealed class AsyncOperationProgress
    {
        /// <summary>
        /// Initializes progress information.
        /// </summary>
        /// <param name="processedItems">Number of processed work items.</param>
        /// <param name="isCompleted">Whether the operation completed successfully.</param>
        /// <param name="phase">Operation phase, for example processing, reading, or writing.</param>
        /// <param name="totalItems">Known total work items, or null if indeterminate.</param>
        public AsyncOperationProgress(
            long processedItems,
            bool isCompleted = false,
            string phase = "Processing",
            long? totalItems = null)
        {
            ProcessedItems = processedItems;
            IsCompleted = isCompleted;
            Phase = phase;
            TotalItems = totalItems;
        }

        /// <summary>
        /// Gets the number of processed work items: transported bytes for MIDI I/O,
        /// record/object work checkpoints for CSV I/O, or accepted input objects for
        /// timed-object writers. CSV checkpoints are not a count of processed notes.
        /// </summary>
        public long ProcessedItems { get; }

        /// <summary>
        /// Gets whether the operation completed successfully.
        /// </summary>
        public bool IsCompleted { get; }

        /// <summary>
        /// Gets the operation phase. Counts from different phases need not use the same unit.
        /// </summary>
        public string Phase { get; }

        /// <summary>
        /// Gets the known total work items, or null when the total is indeterminate.
        /// </summary>
        public long? TotalItems { get; }

        /// <inheritdoc cref="object.ToString" path="/summary" />
        /// <returns>A string containing the phase, processed and total work items, and completion state.</returns>
        public override string ToString()
        {
            return $"{Phase}: {ProcessedItems}/{(TotalItems.HasValue ? TotalItems.Value.ToString() : "?")}; completed: {IsCompleted}";
        }
    }
}
