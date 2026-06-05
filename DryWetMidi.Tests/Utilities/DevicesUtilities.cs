using Melanchall.DryWetMidi.Multimedia;
using NUnit.Framework.Legacy;
using System;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    internal static class DevicesUtilities
    {
        #region Methods

        public static string GetVirtualDeviceName()
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var random = DryWetMidi.Common.Random.Instance;
            var name = new char[8];
            
            for (int i = 0; i < name.Length; i++)
            {
                name[i] = alphabet[random.Next(alphabet.Length)];
            }
            
            return "VIRT_" + new string(name);
        }

        public static InputEndpoint GetInputEndpoint(string name)
        {
            var timeout = TimeSpan.FromSeconds(5);
            var inputEndpoint = default(InputEndpoint);
            
            var success = WaitOperations.Wait(
                () =>
                {
                    inputEndpoint = InputEndpoint.GetByName(name);
                    return inputEndpoint != null;
                },
                timeout);

            ClassicAssert.IsTrue(success, $"Input endpoint with name '{name}' was not found for {timeout}.");

            return inputEndpoint;
        }

        public static OutputEndpoint GetOutputEndpoint(string name)
        {
            var timeout = TimeSpan.FromSeconds(5);
            var outputEndpoint = default(OutputEndpoint);

            var success = WaitOperations.Wait(
                () =>
                {
                    outputEndpoint = OutputEndpoint.GetByName(name);
                    return outputEndpoint != null;
                },
                timeout);

            ClassicAssert.IsTrue(success, $"Output endpoint with name '{name}' was not found for {timeout}.");

            return outputEndpoint;
        }

        #endregion
    }
}
