using Melanchall.DryWetMidi.Configuration;
using Melanchall.DryWetMidi.Multimedia;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using System;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Tests.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class VirtualDeviceApiRequiredAttribute : NUnitAttribute, IApplyToTest
    {
        public void ApplyToTest(Test test)
        {
            var skipReason = GetSkipReason();
            if (!string.IsNullOrEmpty(skipReason))
            {
                test.RunState = RunState.Skipped;
                test.Properties.Set(PropertyNames.SkipReason, skipReason);
            }
        }

        private static string GetSkipReason()
        {
            if (RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
                return null;

            if (!RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                return "Test requires macOS or Windows.";

            var result = MidiConfigurationApi.Api_GetConfiguration(
                true,
                out var rawHandle,
                out _);

            if (result != MidiConfigurationApi.CONFIGURATION_GETRESULT.CONFIGURATION_GETRESULT_OK)
                return "Failed to create configuration.";

            var handle = new MidiConfigurationHandle(rawHandle);

            try
            {
                if (!MidiConfigurationApi.Api_IsVirtualDeviceApiAvailable(handle))
                    return "Virtual device API is not available on current Windows.";
            }
            finally
            {
                handle.Dispose();
            }

            return null;
        }
    }
}
