using Melanchall.DryWetMidi.Multimedia;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using System;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Tests.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class DevicesWatcherApiRequiredAttribute : NUnitAttribute, IApplyToTest
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

            CommonApi.Api_GetNativeEnvironmentInfo_Win(
                out var comInitializationResult,
                out var registryCheckResult,
                out var comCheckResult,
                out var serviceCheckResult,
                out var sdkCheckResult);

            var wmsAvailable =
                comInitializationResult &&
                registryCheckResult &&
                comCheckResult &&
                serviceCheckResult == CommonApi.WMSSERVICECHECKRESULT.WMSSERVICECHECKRESULT_OK &&
                sdkCheckResult;

            if (!wmsAvailable)
                return "Windows MIDI Service are not available.";

            return null;
        }
    }
}
