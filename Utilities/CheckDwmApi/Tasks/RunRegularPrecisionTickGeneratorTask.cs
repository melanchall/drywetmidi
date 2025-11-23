using Melanchall.DryWetMidi.Multimedia;

namespace Melanchall.CheckDwmApi
{
    internal sealed class RunRegularPrecisionTickGeneratorTask : RunTickGeneratorTask<RegularPrecisionTickGenerator>
    {
        public RunRegularPrecisionTickGeneratorTask()
            : base(100)
        {
        }

        public override string GetTitle() =>
            "Run regular precision tick generator";

        public override string GetDescription() => @"
The tool will run RegularPrecisionTickGenerator collecting tick times to check
how stable the requested interval is.";
    }
}
