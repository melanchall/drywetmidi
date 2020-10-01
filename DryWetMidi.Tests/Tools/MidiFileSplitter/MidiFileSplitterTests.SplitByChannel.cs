using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class MidiFileSplitterTests
    {
        #region Test methods

        [Test]
        public void SplitByChannel_ValidFiles()
        {
            foreach (var filePath in TestFilesProvider.GetValidFilesPaths())
            {
                var midiFile = MidiFile.Read(filePath);
                var originalChannels = midiFile
                    .GetTrackChunks()
                    .SelectMany(c => c.Events)
                    .OfType<ChannelEvent>()
                    .Select(e => e.Channel)
                    .Distinct()
                    .ToArray();

                var filesByChannel = midiFile.SplitByChannel().ToList();
                var allChannels = new List<FourBitNumber>(FourBitNumber.Values.Length);

                foreach (var fileByChannel in filesByChannel)
                {
                    Assert.AreEqual(
                        fileByChannel.TimeDivision,
                        midiFile.TimeDivision,
                        "Time division of new file doesn't equal to the time division of the original one.");

                    var channels = fileByChannel
                        .GetTrackChunks()
                        .SelectMany(c => c.Events)
                        .OfType<ChannelEvent>()
                        .Select(e => e.Channel)
                        .Distinct()
                        .ToArray();

                    Assert.AreEqual(
                        1,
                        channels.Length,
                        "New file contains channel events for different channels.");

                    allChannels.Add(channels.First());
                }

                allChannels.Sort();

                CollectionAssert.AreEqual(
                    originalChannels.OrderBy(c => c),
                    allChannels,
                    "Channels from new files differs from those from original one.");
            }
        }

        #endregion
    }
}
