namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Defines how start event of a note should be found in case of overlapping notes with
    /// the same note number and channel. The default value is <see cref="FirstNoteOn"/>.
    /// </summary>
    /// <example>
    /// <para>
    /// To understand how this policy works let's take a look at following events sequence:
    /// </para>
    /// <code language="image">
    /// A...B...X...Y
    /// </code>
    /// <para>
    /// where <c>A</c> and <c>B</c> mean Note On events, and <c>X</c> and <c>Y</c> mean
    /// Note Off ones with the same note number and channel; <c>.</c> is any other event. So we
    /// have two overlapped notes here.
    /// </para>
    /// <para>
    /// If we use <see cref="FirstNoteOn"/> for the policy, notes will be constructed in
    /// following way:
    /// </para>
    /// <code language="image">
    /// ┌───────┐
    /// A...B...X...Y
    ///     └───────┘
    /// </code>
    /// <para>
    /// or, if highlight notes:
    /// </para>
    /// <code language="image">
    /// [         ]
    ///     [         ]
    /// </code>
    /// <para>
    /// So every Note Off event will be combined with first free Note On event into a note.
    /// </para>
    /// <para>
    /// But if we use <see cref="LastNoteOn"/> value, notes will be constructed in
    /// a new way:
    /// </para>
    /// <code language="image">
    /// ┌───────────┐
    /// A...B...X...Y
    ///      └───┘
    /// </code>
    /// <para>
    /// or, if highlight notes:
    /// </para>
    /// <code language="image">
    /// [             ]
    ///     [     ]
    /// </code>
    /// <para>
    /// So Note Off events will be combined with last free Note On event into a note.
    /// </para>
    /// </example>
    /// <seealso cref="NoteDetectionSettings"/>
    /// <seealso cref="NotesManagingUtilities"/>
    public enum NoteStartDetectionPolicy
    {
        /// <summary>
        /// First Note On event with corresponding note number and channel should be taken.
        /// </summary>
        FirstNoteOn = 0,

        /// <summary>
        /// Last Note On event with corresponding note number and channel should be taken.
        /// </summary>
        LastNoteOn,
    }
}
