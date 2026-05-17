using Melanchall.Common;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Configuration;
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
        static void Main(string[] args)
        {
            var toolOptions = new ToolOptions();
            toolOptions.NonInteractive = args.Any(a => a.Equals("-noninteractive", StringComparison.OrdinalIgnoreCase));
            toolOptions.ExitOnTaskFailure = args.Any(a => a.Equals("-exitontaskfailure", StringComparison.OrdinalIgnoreCase));

            Console.WriteLine(@"
Thank you for your willing to help make DryWetMIDI better by running this program!
It will take just several minutes or even less to run all tests guiding you through the process.".Trim());

            //

            var midiFile = CreateTestMidiFile();

            var tasks = new List<ITask>
            {
                new WriteLibraryInfoTask(),
                new WriteSystemInfoTask(),
                new ReadWriteMidiFileTask(midiFile),
            };

            if (LibraryConfiguration.IsVirtualDeviceApiAvailable() && LibraryConfiguration.IsEndpointsWatcherApiAvailable())
                tasks.Add(new CreateVirtualDeviceTask());

            if (NativeApiUtilities.IsOsSupported())
            {
                tasks.Add(new SendReceiveViaVirtualDeviceTask());
                tasks.Add(new RunHighPrecisionTickGeneratorTask());
            }

            tasks.Add(new RunRegularPrecisionTickGeneratorTask());
            tasks.Add(new CheckPlaybackTask(midiFile, OperatingSystem.IsLinux()));

            //

            Console.WriteLine(UiUtilities.SectionsSmallSeparator);
            Console.WriteLine("Following tasks will be executed:");
            Console.WriteLine(string.Join(Environment.NewLine, tasks.Select((t, i) => $"[{i + 1}] {t.GetTitle()}")));

            //

            Console.WriteLine(UiUtilities.SectionsSmallSeparator);
            var reportFilePath = Path.GetFullPath("CheckDwmApiReport.txt");

            Console.WriteLine($@"
The tool will produce the report file containing the log of tasks execution. In fact,
that report is just what you'll see in the console. The file will be written to
'{reportFilePath}'.".Trim());

            //

            var exceptions = new List<(string TaskTitle, Exception Exception)>();

            using var reportWriter = new ReportWriter(reportFilePath);

            for (var i = 0; i < tasks.Count; i++)
            {
                var task = tasks[i];
                var taskTitle = task.GetTitle();

                reportWriter.WriteLine(UiUtilities.SectionsLargeSeparator);
                reportWriter.WriteLine($"[{i + 1}] {taskTitle}");
                reportWriter.WriteLine(UiUtilities.SectionsSmallSeparator);
                reportWriter.WriteLine(task.GetDescription().Trim());
                reportWriter.WriteLine(UiUtilities.SectionsLargeSeparator);

                try
                {
                    task.Execute(toolOptions, reportWriter);

                    reportWriter.WriteLine(UiUtilities.SectionsSmallSeparator);
                    reportWriter.WriteLine("SUCCESS");
                }
                catch (TaskFailedException ex)
                {
                    reportWriter.WriteLine($"FAILED: {ex.Message}");
                    exceptions.Add((taskTitle, ex));
                }
                catch (Exception ex)
                {
                    reportWriter.WriteLine($"UNEXPECTED FAILURE: {ex}");
                    exceptions.Add((taskTitle, ex));
                }
            }

            //

            reportWriter.Close();

            Console.WriteLine(UiUtilities.SectionsLargeSeparator);
            Console.WriteLine($@"
All tasks finished. Report has been saved at
{reportFilePath}".Trim());

            if (exceptions.Count > 0 && toolOptions.ExitOnTaskFailure)
                throw new InvalidOperationException($"Following tasks failed:{Environment.NewLine}{string.Join(Environment.NewLine, exceptions.Select(e => $"- {e.TaskTitle}: {e.Exception.Message}"))}");

            if (!toolOptions.NonInteractive)
            {
                OpenReportFile(reportFilePath);

                Console.WriteLine(UiUtilities.SectionsSmallSeparator);
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
