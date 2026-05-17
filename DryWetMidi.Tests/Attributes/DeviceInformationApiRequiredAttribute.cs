using Melanchall.DryWetMidi.Configuration;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using System;

namespace Melanchall.DryWetMidi.Tests.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class DeviceInformationApiRequiredAttribute : NUnitAttribute, IApplyToTest
    {
        public void ApplyToTest(Test test)
        {
            if (!LibraryConfiguration.IsDeviceInformationApiAvailable())
            {
                test.RunState = RunState.Skipped;
                test.Properties.Set(PropertyNames.SkipReason, "Parent device API is not supported on the current operating system.");
            }
        }
    }
}
