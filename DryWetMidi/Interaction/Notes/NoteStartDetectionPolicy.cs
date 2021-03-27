namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Defines how start event of a note should be found in case of overlapping notes with
    /// the same note number and channel. The default value is <see cref="FirstNoteOn"/>.
    /// </summary>
    /// <remarks>
    /// <para>To understand how this policy works let's take a look at following events sequence:</para>
    /// <para><c>ON1...ON2...OFF1...OFF2</c></para>
    /// <para>where <c>ON</c> and <c>OFF</c> means Note On and Note Off events correspondingly
    /// with the same note number and channel; <c>.</c> is any other event. So we have two
    /// overlapped notes here.</para>
    /// <para>
    /// If we use <see cref="FirstNoteOn"/> for the policy, notes will be constructed in
    /// following way:
    /// </para>
    /// <para>
    /// <code>
    ///   +----------------+
    ///   |                |
    /// [ON1]...(ON2)...[OFF1]...(OFF2)
    ///           |                 |
    ///           +-----------------+
    /// </code>
    /// </para>
    /// <para>
    /// So every Note Off event will be combined with first free Note On event into a note.
    /// <c>[...]</c> and <c>(...)</c> denote two different instances of <see cref="Note"/>.
    /// </para>
    /// <para>
    /// But if we use <see cref="LastNoteOn"/> value, notes will be constructed in
    /// a new way:
    /// </para>
    /// <para>
    /// <code>
    ///   +-------------------------+
    ///   |                         |
    /// [ON1]...(ON2)...(OFF1)...[OFF2]
    ///           |        |
    ///           +--------+
    /// </code>
    /// </para>
    /// <para>
    /// So Note Off events will be combined with last free Note On event into a note.
    /// </para>
    /// </remarks>
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
