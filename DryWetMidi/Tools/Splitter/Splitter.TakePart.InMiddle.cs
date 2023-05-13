using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Tools
{
    public static partial class Splitter
    {
        #region Methods

        /// <summary>
        /// Takes a part of the specified length of a MIDI file (starting at the specified time within the file)
        /// and returns it as an instance of <see cref="MidiFile"/>. More info in the
        /// <see href="xref:a_file_splitting#takepart">MIDI file splitting: TakePart</see> article.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to take part of.</param>
        /// <param name="partStart">The start time of part to take.</param>
        /// <param name="partLength">The length of part to take.</param>
        /// <param name="settings">Settings according to which <paramref name="midiFile"/>
        /// should be split.</param>
        /// <returns><see cref="MidiFile"/> which is part of the <paramref name="midiFile"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="midiFile"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="partStart"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="partLength"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static MidiFile TakePart(this MidiFile midiFile, ITimeSpan partStart, ITimeSpan partLength, SliceMidiFileSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(partStart), partStart);
            ThrowIfArgument.IsNull(nameof(partLength), partLength);

            var grid = new ArbitraryGrid(partStart, partStart.Add(partLength, TimeSpanMode.TimeLength));

            settings = settings ?? new SliceMidiFileSettings();
            midiFile = PrepareMidiFileForSlicing(midiFile, grid, settings);

            var tempoMap = midiFile.GetTempoMap();
            var times = grid.GetTimes(tempoMap).ToArray();

            using (var slicer = MidiFileSlicer.CreateFromFile(midiFile))
            {
                slicer.GetNextSlice(times[0], settings);
                return slicer.GetNextSlice(times[1], settings);
            }
        }

        #endregion
    }
}
