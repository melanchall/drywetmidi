using Melanchall.DryWetMidi.Configuration;

namespace Melanchall.CheckDwmApi
{
    internal sealed class WriteLibraryInfoTask : ITask
    {
        public string GetTitle() =>
            "Write library information";

        public string GetDescription() =>
            "Writes information about DryWetMIDI.";

        public void Execute(ToolOptions toolOptions, ReportWriter reportWriter)
        {
            reportWriter.WriteOperationTitle(LibraryConfiguration.GetConfigurationSummary());
        }
    }
}
