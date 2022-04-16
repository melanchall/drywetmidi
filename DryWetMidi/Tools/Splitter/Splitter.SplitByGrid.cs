using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tools
{
    public static partial class Splitter
    {
        #region Methods

        /// <summary>
        /// Splits <see cref="MidiFile"/> by the specified grid.
        /// </summary>
        /// <remarks>
        /// Non-track chunks will not be copied to any of the new files.
        /// </remarks>
        /// <param name="midiFile"><see cref="MidiFile"/> to split.</param>
        /// <param name="grid">Grid to split <paramref name="midiFile"/> by.</param>
        /// <param name="settings">Settings according to which file should be split.</param>
        /// <returns>Collection of <see cref="MidiFile"/> produced during splitting the input file by grid.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="midiFile"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="grid"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <example>
        /// <para>
        /// Given the MIDI file (vertical lines show the grid the file should be split by):
        /// </para>
        /// <code language="image">
        /// +-----║-------║-----------+
        /// |┌────║───────║──────────┐|
        /// |│  A ║  B    ║     C    │|
        /// |└──⁞─║──⁞────║─────⁞────┘|
        /// |┌──⁞─║──⁞────║─────⁞────┐|
        /// |│  ⁞ ║  ⁞ D  ║  E  ⁞    │|
        /// |└──⁞─║──⁞─⁞──║──⁞──⁞────┘|
        /// +---⁞-║--⁞-⁞--║--⁞--⁞-----+
        /// </code>
        /// <para>
        /// where <c>A</c>, <c>B</c>, <c>C</c>, <c>D</c> and <c>E</c> are some MIDI events.
        /// </para>
        /// <para>
        /// We'll get three new files as the result of the split:
        /// </para>
        /// <code language="image">
        /// +---⁞--+ ⁞ ⁞ +---⁞--⁞-----+
        /// |┌──⁞─┐| ⁞ ⁞ |┌──⁞──⁞────┐|
        /// |│  A │| ⁞ ⁞ |│  ⁞  C    │|
        /// |└────┘| ⁞ ⁞ |└──⁞───────┘|
        /// |┌────┐| ⁞ ⁞ |┌──⁞───────┐|
        /// |│    │| ⁞ ⁞ |│  E       │|
        /// |└────┘| ⁞ ⁞ |└──────────┘|
        /// +------+ ⁞ ⁞ +------------+
        ///      +---⁞-⁞---+
        ///      |┌──⁞─⁞──┐|
        ///      |│  B ⁞  │|
        ///      |└────⁞──┘|
        ///      |┌────⁞──┐|
        ///      |│    D  │|
        ///      |└───────┘|
        ///      +---------+
        /// </code>
        /// </example>
        public static IEnumerable<MidiFile> SplitByGrid(this MidiFile midiFile, IGrid grid, SliceMidiFileSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(grid), grid);

            if (!midiFile.GetEvents().Any())
                yield break;

            settings = settings ?? new SliceMidiFileSettings();
            midiFile = PrepareMidiFileForSlicing(midiFile, grid, settings);

            var tempoMap = midiFile.GetTempoMap();

            using (var slicer = MidiFileSlicer.CreateFromFile(midiFile))
            {
                foreach (var time in grid.GetTimes(tempoMap))
                {
                    if (time == 0)
                        continue;

                    yield return slicer.GetNextSlice(time, settings);

                    if (slicer.AllEventsProcessed)
                        break;
                }

                if (!slicer.AllEventsProcessed)
                    yield return slicer.GetNextSlice(long.MaxValue, settings);
            }
        }

        #endregion
    }
}
