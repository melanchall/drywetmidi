using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Melanchall.CheckDwmApi
{
    internal class Program
    {
        private static readonly string SectionsSmallSeparator = new string('-', 80);
        private static readonly string SectionsLargeSeparator = new string('=', 80);

        static void Main(string[] args)
        {
            var toolOptions = new ToolOptions();
            toolOptions.NonInteractive = args.Any(a => a.Equals("-noninteractive", StringComparison.OrdinalIgnoreCase));

            Console.WriteLine(@"
Thank you for your willing to help make DryWetMIDI better by running this program!
It will take just several minutes to run all tests guiding you through the process.".Trim());

            //

            var midiFile = CreateTestMidiFile();

            var tasks = new List<ITask>
            {
                new WriteSystemInfoTask(),
                new ReadWriteMidiFileTask(midiFile),
            };

            if (OperatingSystem.IsMacOS())
                tasks.Add(new CreateVirtualDeviceTask());

            if (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS())
            {
                tasks.Add(new SendReceiveViaVirtualDeviceTask());
                tasks.Add(new RunHighPrecisionTickGeneratorTask());
            }

            tasks.Add(new RunRegularPrecisionTickGeneratorTask());
            tasks.Add(new CheckPlaybackTask(midiFile, OperatingSystem.IsLinux()));

            //

            Console.WriteLine(SectionsSmallSeparator);
            Console.WriteLine("Following tasks will be executed:");
            Console.WriteLine(string.Join(Environment.NewLine, tasks.Select((t, i) => $"[{i + 1}] {t.GetTitle()}")));

            //

            Console.WriteLine(SectionsSmallSeparator);
            var reportFilePath = Path.GetFullPath("CheckDwmApiReport.txt");

            Console.WriteLine($@"
The tool will produce the report file containing the log of tasks execution. In fact,
that report is just what you'll see in the console. The file will be written to
'{reportFilePath}'.".Trim());

            //

            using var reportWriter = new ReportWriter(reportFilePath);

            for (var i = 0; i < tasks.Count; i++)
            {
                var task = tasks[i];

                reportWriter.WriteLine(SectionsLargeSeparator);
                reportWriter.WriteLine($"[{i + 1}] {task.GetTitle()}");
                reportWriter.WriteLine(SectionsSmallSeparator);
                reportWriter.WriteLine(task.GetDescription().Trim());
                reportWriter.WriteLine(SectionsLargeSeparator);

                try
                {
                    task.Execute(toolOptions, reportWriter);

                    reportWriter.WriteLine(SectionsSmallSeparator);
                    reportWriter.WriteLine("SUCCESS");
                }
                catch (TaskFailedException ex)
                {
                    reportWriter.WriteLine($"FAILED: {ex.Message}");
                }
                catch (Exception ex)
                {
                    reportWriter.WriteLine($"UNEXPECTED FAILURE: {ex}");
                }
            }

            //

            reportWriter.Close();

            Console.WriteLine(SectionsLargeSeparator);
            Console.WriteLine($@"
All tasks finished. Report has been saved at
{reportFilePath}".Trim());

            if (!toolOptions.NonInteractive)
            {
                OpenReportFile(reportFilePath);

                Console.WriteLine(SectionsSmallSeparator);
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        private static MidiFile CreateTestMidiFile()
        {
            var patternBuilder = new PatternBuilder()
                .SetNoteLength(MusicalTimeSpan.Quarter);

            var notes = Scale
                .Parse("C Major")
                .GetAscendingNotes(DryWetMidi.MusicTheory.Note.Get(NoteName.C, 3))
                .Take(5)
                .ToArray();

            foreach (var note in notes)
            {
                patternBuilder.Note(note);
            }

            return patternBuilder.Build().ToFile(TempoMap.Default);
        }

        private static void OpenReportFile(string reportFilePath)
        {
            try
            {
                Console.WriteLine("Opening the report file...");

                using var process = new System.Diagnostics.Process();
                process.StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = reportFilePath,
                    UseShellExecute = true
                };
                process.Start();
            }
            catch
            {
                Console.WriteLine("Cannot open the report file automatically. Please open it manually.");
            }
        }
    }
}
