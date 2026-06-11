using Melanchall.DryWetMidi.Multimedia;
using NUnit.Framework.Legacy;
using System;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    internal static class DevicesUtilities
    {
        #region Constants

        public static readonly TimeSpan EndpointsSearchTimeout = TimeSpan.FromSeconds(10);

        #endregion

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
            var inputEndpoint = default(InputEndpoint);
            
            var success = WaitOperations.Wait(
                () =>
                {
                    inputEndpoint = InputEndpoint.GetByName(name);
                    return inputEndpoint != null;
                },
                EndpointsSearchTimeout);

            ClassicAssert.IsTrue(success, $"Input endpoint with name '{name}' was not found for {EndpointsSearchTimeout}.");

            return inputEndpoint;
        }

        public static OutputEndpoint GetOutputEndpoint(string name)
        {
            var outputEndpoint = default(OutputEndpoint);

            var success = WaitOperations.Wait(
                () =>
                {
                    outputEndpoint = OutputEndpoint.GetByName(name);
                    return outputEndpoint != null;
                },
                EndpointsSearchTimeout);

            ClassicAssert.IsTrue(success, $"Output endpoint with name '{name}' was not found for {EndpointsSearchTimeout}.");

            return outputEndpoint;
        }

        #endregion
    }
}
