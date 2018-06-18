using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed class MidiFileSplitterTests
    {
        #region Test methods

        [Test]
        [Description("Split valid MIDI files by channel.")]
        public void SplitByChannel_ValidFiles()
        {
            foreach (var filePath in TestFilesProvider.GetValidFiles())
            {
                var midiFile = MidiFile.Read(filePath);
                var originalChannels = midiFile.GetTrackChunks()
                                               .SelectMany(c => c.Events)
                                               .OfType<ChannelEvent>()
                                               .Select(e => e.Channel)
                                               .Distinct()
                                               .ToArray();

                var filesByChannel = midiFile.SplitByChannel().ToList();
                var allChannels = new List<FourBitNumber>(FourBitNumber.MaxValue + 1);

                foreach (var fileByChannel in filesByChannel)
                {
                    Assert.AreEqual(fileByChannel.TimeDivision,
                                    midiFile.TimeDivision,
                                    $"Time division of new file doesn't equal to the time division of '{filePath}'.");

                    var channels = fileByChannel.GetTrackChunks()
                                                .SelectMany(c => c.Events)
                                                .OfType<ChannelEvent>()
                                                .Select(e => e.Channel)
                                                .Distinct()
                                                .ToArray();
                    Assert.AreEqual(1,
                                    channels.Length,
                                    $"File from '{filePath}' contains channel events for different channels.");

                    allChannels.Add(channels.First());
                }

                allChannels.Sort();

                CollectionAssert.AreEqual(originalChannels.OrderBy(c => c),
                                          allChannels,
                                          $"Channels from new files differs from those from original file '{filePath}'.");
            }
        }

        #endregion
    }
}
