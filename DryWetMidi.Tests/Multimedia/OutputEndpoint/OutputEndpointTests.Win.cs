using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Attributes;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class OutputEndpointTests
    {
        #region Constants

        public const string MicrosoftGsWavetableSynth = "Microsoft GS Wavetable Synth";

        #endregion

        #region Test methods

        // TODO: failed on WMS enabled
        // [Test]
        [WinOnly]
        public void OutputEndpointIsInUse()
        {
            using (var outputEndpoint1 = OutputEndpoint.GetByName(MidiEndpoints.A))
            {
                outputEndpoint1.SendEvent(new NoteOnEvent());

                using (var outputEndpoint2 = OutputEndpoint.GetByName(MidiEndpoints.A))
                {
                    ClassicAssert.Throws<NativeApiException>(() => outputEndpoint2.SendEvent(new NoteOnEvent()));
                }
            }
        }

        [MultimediaTestRetry]
        [Test]
        public void SendEvent_SysEx_SysExBufferSettings(
            [Values(32, 64, 128, 256, 1024, 4096)] int bufferSize,
            [Values(2, 4, 10, 100)] int buffersCount)
        {
            var bytes = Enumerable
                .Range(0, 10000)
                .Select(_ => (byte)DryWetMidi.Common.Random.Instance.Next(127))
                .Concat(new byte[] { 0xF7 })
                .ToArray();

            SendEvents(
                new[] { new NormalSysExEvent(bytes) },
                setupInputEndpoint: inputEndpoint =>
                {
                    inputEndpoint.SysExBufferSize = bufferSize;
                    inputEndpoint.SysExBuffersCount = buffersCount;
                });
        }

        [Test]
        [WinOnly]
        public void CheckMicrosoftGsWavetableSynth()
        {
            using (var outputEndpoint = DevicesUtilities.GetOutputEndpoint(MicrosoftGsWavetableSynth))
            {
                try
                {
                    outputEndpoint.PrepareForEventsSending();
                    outputEndpoint.SendEvent(new NoteOnEvent());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to work with the '{MicrosoftGsWavetableSynth}' synth: {ex.Message}");
                }
            }
        }

        [Test]
        [WinOnly]
        public void CheckMicrosoftGsWavetableSynthEquality_SameEndpoints()
        {
            var outputEndpoint1 = DevicesUtilities.GetOutputEndpoint(MicrosoftGsWavetableSynth);
            var outputEndpoint2 = DevicesUtilities.GetOutputEndpoint(MicrosoftGsWavetableSynth);
            ClassicAssert.AreEqual(outputEndpoint1, outputEndpoint2, "Endpoints are not equal.");
        }

        [Test]
        [WinOnly]
        public void CheckMicrosoftGsWavetableSynthEquality_DifferentEndpoints()
        {
            var outputEndpoint1 = DevicesUtilities.GetOutputEndpoint(MicrosoftGsWavetableSynth);
            var outputEndpoint2 = OutputEndpoint.GetByName(MidiEndpoints.A);
            ClassicAssert.AreNotEqual(outputEndpoint1, outputEndpoint2, "Endpoints are equal.");
        }

        #endregion
    }
}
