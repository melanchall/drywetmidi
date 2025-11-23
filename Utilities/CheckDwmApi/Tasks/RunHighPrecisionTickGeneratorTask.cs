using Melanchall.DryWetMidi.Multimedia;

namespace Melanchall.CheckDwmApi
{
    internal sealed class RunHighPrecisionTickGeneratorTask : RunTickGeneratorTask<HighPrecisionTickGenerator>
    {
        public RunHighPrecisionTickGeneratorTask()
            : base(100)
        {
        }

        public override string GetTitle() =>
            "Run high precision tick generator";

        public override string GetDescription() => @"
The tool will run HighPrecisionTickGenerator collecting tick times to check
how stable the requested interval is.";
    }
}
