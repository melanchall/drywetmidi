using Melanchall.DryWetMidi.Core;
using System;
using System.IO;

namespace Melanchall.CheckDwmApi
{
    internal sealed class ReadWriteMidiFileTask : ITask
    {
        private readonly MidiFile _midiFile;

        public ReadWriteMidiFileTask(MidiFile midiFile)
        {
            _midiFile = midiFile;
        }

        public string GetTitle() =>
            "Read and write a MIDI file";

        public string GetDescription() => @"
The tool will create a simple MIDI file in memory, write it to disk and
then read it back. You will be provided with the location of the file.";

        public void Execute(
            ToolOptions toolOptions,
            ReportWriter reportWriter)
        {
            var testFilePath = Path.Combine(Path.GetTempPath(), "TestFile.mid");

            try
            {
                WriteMidiFile(reportWriter, testFilePath);
                var readMidiFile = ReadMidiFile(reportWriter, testFilePath);
                CheckReadMidiFile(reportWriter, readMidiFile);
            }
            finally
            {
                reportWriter.WriteOperationTitle($"Deleting the test file '{testFilePath}'...");

                if (File.Exists(testFilePath))
                    File.Delete(testFilePath);

                reportWriter.WriteOperationSubTitle("deleted");
            }
        }

        private void CheckReadMidiFile(ReportWriter reportWriter, MidiFile readMidiFile)
        {
            var settings = new MidiFileEqualityCheckSettings
            {
                CompareOriginalFormat = false,
            };

            reportWriter.WriteOperationTitle("Comparing read file with the original one...");

            if (!MidiFile.Equals(_midiFile, readMidiFile, settings, out var message))
                throw new TaskFailedException("The read MIDI file is not equal to the written one: " + message);

            reportWriter.WriteOperationSubTitle("success");
        }

        private void WriteMidiFile(ReportWriter reportWriter, string filePath)
        {
            try
            {
                reportWriter.WriteOperationTitle($"Writing the file to '{filePath}'...");
                _midiFile.Write(filePath, true);
                reportWriter.WriteOperationSubTitle("success");
            }
            catch (Exception ex)
            {
                throw new TaskFailedException("Failed to write the MIDI file to disk: " + ex.Message, ex);
            }
        }

        private MidiFile ReadMidiFile(ReportWriter reportWriter, string filePath)
        {
            try
            {
                reportWriter.WriteOperationTitle($"Reading the file from '{filePath}'...");
                var result = MidiFile.Read(filePath);
                reportWriter.WriteOperationSubTitle("success");
                return result;
            }
            catch (Exception ex)
            {
                throw new TaskFailedException("Failed to read the MIDI file from disk: " + ex.Message, ex);
            }
        }
    }
}
