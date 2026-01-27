using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tools;
using System;
using System.IO;
using System.Linq;

namespace DwmNetConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"OS version: {Environment.OSVersion}");
            Console.WriteLine($"CLR version: {Environment.Version}");
            Console.WriteLine("---------------------------------");

            Console.WriteLine("Generating objects...");

            var tempoMap = TempoMap.Default;
            var noteLength = new BarBeatFractionTimeSpan(0, 1.5);

            var objects = Enumerable
                .Range(0, 100)
                .SelectMany(i =>
                {
                    var time = new BarBeatTicksTimeSpan(i);
                    return new ITimedObject[]
                    {
                        new TimedEvent(new TextEvent(i.ToString()))
                            .SetTime(time, tempoMap),
                        new Note((SevenBitNumber)i)
                            .SetTime(time, tempoMap)
                            .SetLength(noteLength, tempoMap),
                    };
                });

            Console.WriteLine("Processing objects...");
            var manyObjects = objects
                .SplitObjectsByPartsNumber(4, TimeSpanType.BarBeatTicks, tempoMap)
                .Repeat(9, tempoMap, new RepeatingSettings
                {
                    ShiftRoundingPolicy = TimeSpanRoundingPolicy.RoundUp,
                    ShiftRoundingStep = new BarBeatTicksTimeSpan(1)
                });

            var tempFilePath = Path.Combine(Path.GetTempPath(), "TestFileName");

            try
            {
                var midiFile = objects.ToFile();

                Console.WriteLine($"Writing file '{tempFilePath}'...");
                midiFile.Write(tempFilePath, true);

                Console.WriteLine("File has been written.");
            }
            finally
            {
                if (File.Exists(tempFilePath))
                    File.Delete(tempFilePath);
            }
        }
    }
}
