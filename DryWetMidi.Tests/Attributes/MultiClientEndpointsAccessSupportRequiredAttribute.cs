using Melanchall.DryWetMidi.Configuration;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using System;

namespace Melanchall.DryWetMidi.Tests.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class MultiClientEndpointsAccessSupportRequiredAttribute : NUnitAttribute, IApplyToTest
    {
        public void ApplyToTest(Test test)
        {
            if (!LibraryConfiguration.IsEndpointsMultiClientAccessAvailable())
            {
                test.RunState = RunState.Skipped;
                test.Properties.Set(PropertyNames.SkipReason, "Endpoints multi-client access is not available on the current operating system.");
            }
        }
    }
}
