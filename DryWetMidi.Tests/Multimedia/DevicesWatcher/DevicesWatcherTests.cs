using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Attributes;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    [DevicesWatcherApiRequired]
    [VirtualDeviceApiRequired]
    public sealed class DevicesWatcherTests
    {
        #region Test methods

        [Test]
        public void CheckDeviceAddedRemoved()
        {
            Action<TestCheckpoints> check = checkpoints =>
            {
                var addedDevices = new List<MidiDevice>();
                var removedDevices = new List<MidiDevice>();

                EventHandler<DeviceAddedRemovedEventArgs> addedHandler = (_, e) =>
                {
                    addedDevices.Add(e.Device);

#if TEST
                    e.Device.TestCheckpoints = checkpoints;
#endif
                };

                DevicesWatcher.Instance.DeviceAdded += addedHandler;

                EventHandler<DeviceAddedRemovedEventArgs> removedHandler = (_, e) =>
                {
                    removedDevices.Add(e.Device);

#if TEST
                    e.Device.TestCheckpoints = checkpoints;
#endif
                };

                DevicesWatcher.Instance.DeviceRemoved += removedHandler;

                var deviceName = "VD7";
                var timeout = TimeSpan.FromSeconds(5);

                using (var virtualDevice = VirtualDevice.Create(deviceName))
                {
                    var added = WaitOperations.Wait(() => addedDevices.Count >= 2, timeout);
                    ClassicAssert.IsTrue(added, $"Devices weren't added for [{timeout}].");

                    ClassicAssert.AreEqual(2, addedDevices.Count, $"Invalid count of added devices ({string.Join(", ", addedDevices.Select(d => $"{d.Context}"))}).");

                    var firstAddedDevice = addedDevices.First();
                    ClassicAssert.IsInstanceOf<InputDevice>(firstAddedDevice, "Invalid type of the first added device.");
                    ClassicAssert.AreEqual(deviceName, firstAddedDevice.Name, "Invalid name of the first added device.");

                    var lastAddedDevice = addedDevices.Last();
                    ClassicAssert.IsInstanceOf<OutputDevice>(lastAddedDevice, "Invalid type of the last added device.");
                    ClassicAssert.AreEqual(deviceName, lastAddedDevice.Name, "Invalid name of the last added device.");
                }

                var removed = WaitOperations.Wait(() => removedDevices.Count >= 2, timeout);
                ClassicAssert.IsTrue(removed, $"Devices weren't removed for [{timeout}].");

                ClassicAssert.AreEqual(2, removedDevices.Count, "Invalid count of removed devices.");

                var firstRemovedDevice = removedDevices.First();
                ClassicAssert.IsInstanceOf<InputDevice>(firstRemovedDevice, "Invalid type of the first removed device.");

                var lastRemovedDevice = removedDevices.Last();
                ClassicAssert.IsInstanceOf<OutputDevice>(lastRemovedDevice, "Invalid type of the last removed device.");

                DevicesWatcher.Instance.DeviceAdded -= addedHandler;
                DevicesWatcher.Instance.DeviceRemoved -= removedHandler;
            };

#if TEST
            var testCheckpoints = new TestCheckpoints();
            check(testCheckpoints);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var inA = testCheckpoints.GetCheckpointDataList(InputDeviceCheckpointsNames.ReleaseInfoHandleEntered);
            ClassicAssert.AreEqual(2, inA.Count, $"Invalid count of reached checkpoint [{InputDeviceCheckpointsNames.ReleaseInfoHandleEntered}].");

            var inB = testCheckpoints.GetCheckpointDataList(InputDeviceCheckpointsNames.InfoDeletedInReleaseInfoHandle);
            ClassicAssert.AreEqual(2, inB.Count, $"Invalid count of reached checkpoint [{InputDeviceCheckpointsNames.InfoDeletedInReleaseInfoHandle}].");

            var outA = testCheckpoints.GetCheckpointDataList(OutputDeviceCheckpointsNames.ReleaseInfoHandleEntered);
            ClassicAssert.AreEqual(2, outA.Count, $"Invalid count of reached checkpoint [{OutputDeviceCheckpointsNames.ReleaseInfoHandleEntered}].");
            
            var outB = testCheckpoints.GetCheckpointDataList(OutputDeviceCheckpointsNames.InfoDeletedInReleaseInfoHandle);
            ClassicAssert.AreEqual(2, outB.Count, $"Invalid count of reached checkpoint [{OutputDeviceCheckpointsNames.InfoDeletedInReleaseInfoHandle}].");
#else
            check(null);
#endif
        }

        [Test]
        public void CheckDeviceAdded()
        {
            var addedDevices1 = new List<MidiDevice>();
            var addedDevices2 = new List<MidiDevice>();

            EventHandler<DeviceAddedRemovedEventArgs> addedHandler1 = (_, e) => addedDevices1.Add(e.Device);
            EventHandler<DeviceAddedRemovedEventArgs> addedHandler2 = (_, e) => addedDevices2.Add(e.Device);
            
            DevicesWatcher.Instance.DeviceAdded += addedHandler1;
            DevicesWatcher.Instance.DeviceAdded += addedHandler2;

            var deviceName = "VD8";
            var timeout = TimeSpan.FromSeconds(5);

            using (var virtualDevice = VirtualDevice.Create(deviceName))
            {
                var added1 = WaitOperations.Wait(() => addedDevices1.Count >= 2, timeout);
                var added2 = WaitOperations.Wait(() => addedDevices2.Count >= 2, timeout);
                ClassicAssert.IsTrue(added1, $"[A] Devices weren't added for [{timeout}] on first collection.");
                ClassicAssert.AreEqual(2, addedDevices1.Count, $"[A] Invalid first count of added devices.");
                ClassicAssert.IsTrue(added2, $"[A] Devices weren't added for [{timeout}] on second collection.");
                ClassicAssert.AreEqual(2, addedDevices2.Count, $"[A] Invalid second count of added devices.");
            }

            DevicesWatcher.Instance.DeviceAdded -= addedHandler1;
            addedDevices1.Clear();
            addedDevices2.Clear();

            using (var virtualDevice = VirtualDevice.Create(deviceName))
            {
                var added1 = WaitOperations.Wait(() => addedDevices1.Count > 0, timeout);
                var added2 = WaitOperations.Wait(() => addedDevices2.Count >= 2, timeout);
                ClassicAssert.IsFalse(added1, $"[B] Devices were added on first collection.");
                ClassicAssert.AreEqual(0, addedDevices1.Count, $"[B] Invalid first count of added devices.");
                ClassicAssert.IsTrue(added2, $"[B] Devices weren't added for [{timeout}] on second collection.");
                ClassicAssert.AreEqual(2, addedDevices2.Count, $"[B] Invalid second count of added devices.");
            }

            DevicesWatcher.Instance.DeviceAdded -= addedHandler2;
            addedDevices1.Clear();
            addedDevices2.Clear();

            using (var virtualDevice = VirtualDevice.Create(deviceName))
            {
                var added1 = WaitOperations.Wait(() => addedDevices1.Count > 0, timeout);
                var added2 = WaitOperations.Wait(() => addedDevices2.Count > 0, timeout);
                ClassicAssert.IsFalse(added1, $"[C] Devices were added on first collection.");
                ClassicAssert.AreEqual(0, addedDevices1.Count, $"[C] Invalid first count of added devices.");
                ClassicAssert.IsFalse(added2, $"[C] Devices were added on second collection.");
                ClassicAssert.AreEqual(0, addedDevices2.Count, $"[C] Invalid second count of added devices.");
            }
        }

        [Test]
        public void CheckDeviceRemoved()
        {
            var removedDevices1 = new List<MidiDevice>();
            var removedDevices2 = new List<MidiDevice>();

            EventHandler<DeviceAddedRemovedEventArgs> removedHandler1 = (_, e) => removedDevices1.Add(e.Device);
            EventHandler<DeviceAddedRemovedEventArgs> removedHandler2 = (_, e) => removedDevices2.Add(e.Device);

            DevicesWatcher.Instance.DeviceRemoved += removedHandler1;
            DevicesWatcher.Instance.DeviceRemoved += removedHandler2;

            var deviceName = "VD8";
            var timeout = TimeSpan.FromSeconds(5);

            Thread.Sleep(5000);
            removedDevices1.Clear();
            removedDevices2.Clear();

            using (var virtualDevice = VirtualDevice.Create(deviceName))
            {
                // TODO: microsoft/MIDI/issues/997
                Thread.Sleep(5000);
            }

            var removed1 = WaitOperations.Wait(() => removedDevices1.Count >= 2, timeout);
            var removed2 = WaitOperations.Wait(() => removedDevices2.Count >= 2, timeout);
            ClassicAssert.IsTrue(removed1, $"[A] Devices weren't removed for [{timeout}] on first collection.");
            ClassicAssert.AreEqual(2, removedDevices1.Count, $"[A] Invalid first count of removed devices.");
            ClassicAssert.IsTrue(removed2, $"[A] Devices weren't removed for [{timeout}] on second collection.");
            ClassicAssert.AreEqual(2, removedDevices2.Count, $"[A] Invalid second count of removed devices.");

            DevicesWatcher.Instance.DeviceRemoved -= removedHandler1;

            Thread.Sleep(5000);
            removedDevices1.Clear();
            removedDevices2.Clear();

            using (var virtualDevice = VirtualDevice.Create(deviceName))
            {
                // TODO: microsoft/MIDI/issues/997
                Thread.Sleep(5000);
            }

            removed1 = WaitOperations.Wait(() => removedDevices1.Count > 0, timeout);
            removed2 = WaitOperations.Wait(() => removedDevices2.Count >= 2, timeout);
            ClassicAssert.IsFalse(removed1, $"[B] Devices were removed on first collection.");
            ClassicAssert.AreEqual(0, removedDevices1.Count, $"[B] Invalid first count of removed devices.");
            ClassicAssert.IsTrue(removed2, $"[B] Devices weren't removed for [{timeout}] on second collection.");
            ClassicAssert.AreEqual(2, removedDevices2.Count, $"[B] Invalid second count of removed devices.");

            DevicesWatcher.Instance.DeviceRemoved -= removedHandler2;

            Thread.Sleep(5000);
            removedDevices1.Clear();
            removedDevices2.Clear();

            using (var virtualDevice = VirtualDevice.Create(deviceName))
            {
                // TODO: microsoft/MIDI/issues/997
                Thread.Sleep(5000);
            }

            removed1 = WaitOperations.Wait(() => removedDevices1.Count > 0, timeout);
            removed2 = WaitOperations.Wait(() => removedDevices2.Count > 0, timeout);
            ClassicAssert.IsFalse(removed1, $"[C] Devices were removed on first collection.");
            ClassicAssert.AreEqual(0, removedDevices1.Count, $"[C] Invalid first count of removed devices.");
            ClassicAssert.IsFalse(removed2, $"[C] Devices were removed on second collection.");
            ClassicAssert.AreEqual(0, removedDevices2.Count, $"[C] Invalid second count of removed devices.");
        }

        #endregion
    }
}
