using Melanchall.DryWetMidi.Multimedia;
using System;
using System.Threading;

namespace Melanchall.CheckDwmApi
{
    internal sealed class CreateVirtualDeviceTask : ITask
    {
        private sealed class DeviceEventsFlags
        {
            public bool DeviceAdded { get; set; }

            public bool DeviceRemoved { get; set; }
        }

        private const string DeviceName = "TestVirtualMidiDevice";

        private static readonly TimeSpan DeviceAddedRemovedTimeout = TimeSpan.FromSeconds(5);

        public string GetTitle() =>
            "Create a virtual MIDI device";

        public string GetDescription() => @"
A virtual device will be created to check corresponding API and to test a device
plugged/unplugged monitoring.";

        public void Execute(
            ToolOptions toolOptions,
            ReportWriter reportWriter)
        {
            var deviceEventsFlags = new DeviceEventsFlags();

            SubscibeToDeviceAddedEvent(reportWriter, deviceEventsFlags);
            SubscibeToDeviceRemovedEvent(reportWriter, deviceEventsFlags);

            var virtualDevice = CreateVirtualDevice(reportWriter);
            CheckDeviceAdded(reportWriter, deviceEventsFlags);

            DisposeVirtualDevice(reportWriter, virtualDevice);
            CheckDeviceRemoved(reportWriter, deviceEventsFlags);
        }

        private void DisposeVirtualDevice(
            ReportWriter reportWriter,
            VirtualDevice virtualDevice)
        {
            try
            {
                reportWriter.WriteOperationTitle($"Disposing the virtual MIDI device '{DeviceName}'...");
                virtualDevice.Dispose();
                reportWriter.WriteOperationSubTitle("disposed");
            }
            catch (Exception ex)
            {
                throw new TaskFailedException("Failed to dispose the virtual MIDI device.", ex);
            }
        }

        private void CheckDeviceAdded(
            ReportWriter reportWriter,
            DeviceEventsFlags deviceEventsFlags)
        {
            reportWriter.WriteOperationTitle("Waiting for device addition detected...");
            var added = SpinWait.SpinUntil(() => deviceEventsFlags.DeviceAdded, DeviceAddedRemovedTimeout);
            if (!added)
                throw new TaskFailedException("Virtual MIDI device addition was not detected.");

            reportWriter.WriteOperationSubTitle("detected");
        }

        private void CheckDeviceRemoved(
            ReportWriter reportWriter,
            DeviceEventsFlags deviceEventsFlags)
        {
            reportWriter.WriteOperationTitle("Waiting for device removal detected...");
            var removed = SpinWait.SpinUntil(() => deviceEventsFlags.DeviceRemoved, DeviceAddedRemovedTimeout);
            if (!removed)
                throw new TaskFailedException("Virtual MIDI device removal was not detected.");

            reportWriter.WriteOperationSubTitle("detected");
        }

        private void SubscibeToDeviceAddedEvent(
            ReportWriter reportWriter,
            DeviceEventsFlags deviceEventsFlags)
        {
            try
            {
                reportWriter.WriteOperationTitle("Subscribing to DeviceAdded event...");
                DevicesWatcher.Instance.DeviceAdded += (_, e) =>
                    deviceEventsFlags.DeviceAdded = true;
                reportWriter.WriteOperationSubTitle("subscribed");
            }
            catch (Exception ex)
            {
                throw new TaskFailedException("Failed to subscribe to DeviceAdded event.", ex);
            }
        }

        private void SubscibeToDeviceRemovedEvent(
            ReportWriter reportWriter,
            DeviceEventsFlags deviceEventsFlags)
        {
            try
            {
                reportWriter.WriteOperationTitle("Subscribing to DeviceRemoved event...");
                DevicesWatcher.Instance.DeviceRemoved += (_, e) =>
                    deviceEventsFlags.DeviceRemoved = true;
                reportWriter.WriteOperationSubTitle("subscribed");
            }
            catch (Exception ex)
            {
                throw new TaskFailedException("Failed to subscribe to DeviceRemoved event.", ex);
            }
        }

        private VirtualDevice CreateVirtualDevice(ReportWriter reportWriter)
        {
            try
            {
                reportWriter.WriteOperationTitle($"Creating a virtual MIDI device '{DeviceName}'...");
                var result = VirtualDevice.Create(DeviceName);
                reportWriter.WriteOperationSubTitle("created");
                return result;
            }
            catch (Exception ex)
            {
                throw new TaskFailedException("Failed to create a virtual MIDI device.", ex);
            }
        }
    }
}
