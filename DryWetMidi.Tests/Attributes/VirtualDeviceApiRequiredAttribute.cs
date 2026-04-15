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

            var name = Guid.NewGuid().ToString();
            var sessionName = Marshal.StringToHGlobalAuto(name);

            var result = MidiDevicesSessionApi.Api_OpenSession(
                sessionName,
                InputDeviceCallback,
                OutputDeviceCallback,
                out var rawHandle,
                out _);

            Marshal.FreeHGlobal(sessionName);

            if (result != MidiDevicesSessionApi.SESSION_OPENRESULT.SESSION_OPENRESULT_OK)
                return "Failed to create session.";

            var handle = new MidiDevicesSessionHandle(rawHandle);

            try
            {
                if (!VirtualDeviceApi.Api_IsAvailable(handle))
                    return "Virtual device API is not available on current Windows.";
            }
            finally
            {
                handle.Dispose();
            }

            return null;
        }

        private static void InputDeviceCallback(IntPtr info, bool operation)
        {
        }

        private static void OutputDeviceCallback(IntPtr info, bool operation)
        {
        }
    }
}
