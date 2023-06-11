namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Defines a hint which tells the timed event processing algorithm how it can optimize its performance.
    /// </summary>
    /// <remarks>
    /// If you want to get the maximum performance of a timed event processing (for example,
    /// <see cref="TimedEventsManagingUtilities.ProcessTimedEvents(Core.MidiFile, System.Action{TimedEvent}, System.Predicate{TimedEvent}, TimedEventDetectionSettings, TimedEventProcessingHint)"/>),
    /// choose a hint carefully. Note that you can always use <see href="xref:a_managers">an object manager</see> to
    /// perform any manipulations with timed events but dedicated methods of the <see cref="TimedEventsManagingUtilities"/> will
    /// always be faster and will consume less memory.
    /// </remarks>
    public enum TimedEventProcessingHint
    {
        /// <summary>
        /// <see cref="TimedEvent.Time"/> of a timed event can be changed.
        /// </summary>
        TimeCanBeChanged = 1,

        /// <summary>
        /// The processing algorithm will consider that only properties that don't affect underlying MIDI
        /// events positions will be changed. This hint means maximum performance, i.e. the processing will
        /// take less time and consume less memory.
        /// </summary>
        None = 0,

        /// <summary>
        /// Default hint. Equals to <see cref="TimeCanBeChanged"/>.
        /// </summary>
        Default = TimeCanBeChanged,
    }
}
