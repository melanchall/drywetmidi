using System;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Attributes;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class OutputEndpointTests
    {
        #region Test methods

        [Test]
        [MacOnly]
        public void GetOutputEndpointSupportedProperties_Mac()
        {
            CollectionAssert.AreEquivalent(
                new[]
                {
                    OutputEndpointProperty.Product,
                    OutputEndpointProperty.Manufacturer,
                    OutputEndpointProperty.DriverVersion,
                    OutputEndpointProperty.UniqueId,
                    OutputEndpointProperty.DriverOwner,
                },
                OutputEndpoint.GetSupportedProperties(),
                "Invalid collection of supported properties.");
        }

        [Test]
        [MacOnly]
        public void GetOutputEndpointProperty_Product_Mac()
        {
            var outputEndpoint = DevicesUtilities.GetOutputEndpoint(MidiEndpoints.A);
            ClassicAssert.AreEqual("OutputProduct", outputEndpoint.GetProperty(OutputEndpointProperty.Product), "Product is invalid.");
        }

        [Test]
        [MacOnly]
        public void GetOutputEndpointProperty_Manufacturer_Mac()
        {
            var outputEndpoint = DevicesUtilities.GetOutputEndpoint(MidiEndpoints.A);
            ClassicAssert.AreEqual("OutputManufacturer", outputEndpoint.GetProperty(OutputEndpointProperty.Manufacturer), "Manufacturer is invalid.");
        }

        [Test]
        [MacOnly]
        public void GetOutputEndpointProperty_DriverVersion_Mac()
        {
            var outputEndpoint = DevicesUtilities.GetOutputEndpoint(MidiEndpoints.A);
            ClassicAssert.AreEqual(200, outputEndpoint.GetProperty(OutputEndpointProperty.DriverVersion), "Driver version is invalid.");
        }

        [Test]
        [MacOnly]
        public void GetOutputEndpointProperty_Technology_Mac()
        {
            var outputEndpoint = DevicesUtilities.GetOutputEndpoint(MidiEndpoints.A);
            ClassicAssert.Throws<ArgumentException>(() => outputEndpoint.GetProperty(OutputEndpointProperty.Technology), "Technology is supported.");
        }

        [Test]
        [MacOnly]
        public void GetOutputEndpointProperty_UniqueId_Mac()
        {
            var outputEndpoint = DevicesUtilities.GetOutputEndpoint(MidiEndpoints.A);
            ClassicAssert.IsNotNull(outputEndpoint.GetProperty(OutputEndpointProperty.UniqueId), "Device unique ID is null.");
        }

        [Test]
        [MacOnly]
        public void GetOutputEndpointProperty_VoicesNumber_Mac()
        {
            var outputEndpoint = DevicesUtilities.GetOutputEndpoint(MidiEndpoints.A);
            ClassicAssert.Throws<ArgumentException>(() => outputEndpoint.GetProperty(OutputEndpointProperty.VoicesNumber), "Voices number is supported.");
        }

        [Test]
        [MacOnly]
        public void GetOutputEndpointProperty_NotesNumber_Mac()
        {
            var outputEndpoint = DevicesUtilities.GetOutputEndpoint(MidiEndpoints.A);
            ClassicAssert.Throws<ArgumentException>(() => outputEndpoint.GetProperty(OutputEndpointProperty.NotesNumber), "Notes number is supported.");
        }

        [Test]
        [MacOnly]
        public void GetOutputEndpointProperty_Channels_Mac()
        {
            var outputEndpoint = DevicesUtilities.GetOutputEndpoint(MidiEndpoints.A);
            ClassicAssert.Throws<ArgumentException>(() => outputEndpoint.GetProperty(OutputEndpointProperty.Channels), "Channels is supported.");
        }

        [Test]
        [MacOnly]
        public void GetOutputEndpointProperty_Options_Mac()
        {
            var outputEndpoint = DevicesUtilities.GetOutputEndpoint(MidiEndpoints.A);
            ClassicAssert.Throws<ArgumentException>(() => outputEndpoint.GetProperty(OutputEndpointProperty.Options), "Options is supported.");
        }

        [Test]
        [MacOnly]
        public void GetOutputEndpointProperty_DriverOwner_Mac()
        {
            var outputEndpoint = DevicesUtilities.GetOutputEndpoint(MidiEndpoints.A);
            ClassicAssert.AreEqual("OutputDriverOwner", outputEndpoint.GetProperty(OutputEndpointProperty.DriverOwner), "Driver owner is invalid.");
        }

        #endregion
    }
}
