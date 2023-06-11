namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Defines a strategy for result track chunks creation when merging MIDI files sequentially. The
    /// default value is <see cref="CreateForEachFile"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Let's see how <see cref="MinimizeCount"/> option works. For example, we have three files with
    /// following track chunks:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <description>A, B and C;</description>
    /// </item>
    /// <item>
    /// <description>D and E;</description>
    /// </item>
    /// <item>
    /// <description>F, G, H, I and J.</description>
    /// </item>
    /// </list>
    /// <para>
    /// Merging these files will lead to following result track chunks creation and usage:
    /// </para>
    /// <list type="number">
    /// <item>
    /// <description>Three track chunks (1, 2, 3) are created with the contents of A, B, C;</description>
    /// </item>
    /// <item>
    /// <description>Content of D is written to the chunk 1; content of E is written to the chunk 2;</description>
    /// </item>
    /// <item>
    /// <description>Content of F is written to the chunk 1; content of G is written to the chunk 2;
    /// content of H is written to the chunk 3; two new track chunks (4, 5) are created with the contents of I and J.</description>
    /// </item>
    /// </list>
    /// </remarks>
    /// <seealso cref="Merger.MergeSequentially(System.Collections.Generic.IEnumerable{Core.MidiFile}, SequentialMergingSettings)"/>
    /// <seealso cref="Merger"/>
    public enum ResultTrackChunksCreationPolicy
    {
        /// <summary>
        /// Each track chunk of every file is being merged will lead to separate track chunk
        /// creation in the result file.
        /// </summary>
        CreateForEachFile = 0,

        /// <summary>
        /// Track chunks will be reused.
        /// </summary>
        MinimizeCount
    }
}
