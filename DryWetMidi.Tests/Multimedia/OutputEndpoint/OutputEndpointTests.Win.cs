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

        [TimingCritical]
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

        [Test]
        [WinOnly]
        public void DisposeOutputEndpoint_SysExInFlight_Stress([Values(100, 1000, 20000)] int dataSize)
        {
            var bytes = Enumerable
                .Range(0, dataSize)
                .Select(_ => (byte)DryWetMidi.Common.Random.Instance.Next(127))
                .Concat(new byte[] { 0xF7 })
                .ToArray();

            var errors = new System.Collections.Concurrent.ConcurrentBag<Exception>();

            for (var i = 0; i < 100; i++)
            {
                var outputEndpoint = OutputEndpoint.GetByName(MidiEndpoints.A, forceWinMM: true);
                outputEndpoint.ErrorOccurred += (_, e) => errors.Add(e.Exception);

#if TEST
                var checkpoints = new TestCheckpoints();
                outputEndpoint.TestCheckpoints = checkpoints;
#endif

                try
                {
                    ClassicAssert.DoesNotThrow(
                        () => outputEndpoint.PrepareForEventsSending(),
                        $"Failed to open endpoint on iteration {i} (data size {dataSize}).");
                    ClassicAssert.DoesNotThrow(
                        () => outputEndpoint.SendEvent(new NormalSysExEvent(bytes)),
                        $"Failed to send SysEx on iteration {i} (data size {dataSize}).");
                }
                finally
                {
                    ClassicAssert.IsTrue(
                        System.Threading.Tasks.Task.Run(() => outputEndpoint.Dispose()).Wait(TimeSpan.FromSeconds(30)),
                        $"Dispose hung on iteration {i}.");
                }

#if TEST
                CollectionAssert.IsEmpty(
                    checkpoints.GetErrors(),
                    $"Native cleanup failed on iteration {i} (data size {dataSize}).");
                checkpoints.CheckCheckpointsReached(OutputEndpointCheckpointsNames.CloseEndpointSuccessInReleaseHandle);
#endif

                CollectionAssert.IsEmpty(
                    errors,
                    $"Errors occurred while disposing endpoint on iteration {i} (data size {dataSize}).");
            }
        }

        #endregion
    }
}
