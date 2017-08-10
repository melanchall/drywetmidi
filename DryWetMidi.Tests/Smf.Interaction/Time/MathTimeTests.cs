using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestClass]
    public class MathTimeTests
    {
        [TestMethod]
        [Description("Subtract metric length from metric time.")]
        public void Conversion_Metric_Metric_Subtract()
        {
            var midiFile = new MidiFile(new TrackChunk());
            var tempoMap = midiFile.GetTempoMap();

            var mathTime = new MathTime(new MetricTime(0, 1, 30),
                                        new MetricLength(0, 1, 0),
                                        MathOperation.Subtract);

            Assert.AreEqual(TimeConverter.ConvertFrom(new MetricTime(0, 0, 30), tempoMap),
                            TimeConverter.ConvertFrom(mathTime, tempoMap));
        }

        [TestMethod]
        [Description("Subtract metric length from metric time crossing point of tempo change.")]
        public void Conversion_Metric_Metric_Subtract_TempoChanged()
        {
            var midiFile = new MidiFile(new TrackChunk());

            using (var tempoMapManager = midiFile.ManageTempoMap())
            {
                tempoMapManager.SetTempo(new MetricTime(0, 1, 20), new Tempo(20000));
            }

            var tempoMap = midiFile.GetTempoMap();

            var mathTime = new MathTime(new MetricTime(0, 1, 30),
                                        new MetricLength(0, 1, 0),
                                        MathOperation.Subtract);

            Assert.AreEqual(TimeConverter.ConvertFrom(new MetricTime(0, 0, 30), tempoMap),
                            TimeConverter.ConvertFrom(mathTime, tempoMap));
        }

        [TestMethod]
        [Description("Subtract math length (made up from metric lengths) from metric time crossing points of tempo change.")]
        public void Conversion_Metric_Math_Subtract_TempoChanged()
        {
            var midiFile = new MidiFile(new TrackChunk());

            using (var tempoMapManager = midiFile.ManageTempoMap())
            {
                tempoMapManager.SetTempo(new MetricTime(0, 1, 5), new Tempo(20000));
                tempoMapManager.SetTempo(new MetricTime(0, 1, 50), new Tempo(2400));
            }

            var tempoMap = midiFile.GetTempoMap();

            var mathLength = new MathLength(new MetricLength(0, 0, 20),
                                            new MetricLength(0, 0, 50));
            var mathTime = new MathTime(new MetricTime(0, 2, 0),
                                        mathLength,
                                        MathOperation.Subtract);

            Assert.AreEqual(TimeConverter.ConvertFrom(new MetricTime(0, 0, 50), tempoMap),
                            TimeConverter.ConvertFrom(mathTime, tempoMap));
        }
    }
}
