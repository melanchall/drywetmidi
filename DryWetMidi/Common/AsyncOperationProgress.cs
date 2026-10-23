namespace Melanchall.DryWetMidi.Common
{
    /// <summary>
    /// Describes progress of a bulk asynchronous operation. Item counts are operation-specific,
    /// and a total is not inferred by enumerating an input sequence.
    /// </summary>
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
        /// Gets the number of processed work items. A work item can represent an event,
        /// object, chunk, or algorithm step; this is not necessarily a count of output objects.
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
    }
}
