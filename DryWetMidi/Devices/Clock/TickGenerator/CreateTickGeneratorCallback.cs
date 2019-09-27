using System;

namespace Melanchall.DryWetMidi.Devices
{
    public delegate ITickGenerator CreateTickGeneratorCallback(TimeSpan interval);
}
