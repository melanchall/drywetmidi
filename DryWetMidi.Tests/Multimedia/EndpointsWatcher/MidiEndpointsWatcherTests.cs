using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
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
    [EndpointsWatcherApiRequired]
    [VirtualDeviceApiRequired]
    public sealed class MidiEndpointsWatcherTests
    {
        #region Test methods

        [Test]
        public void CheckEndpointAddedRemoved()
        {
            Action<TestCheckpoints> check = checkpoints =>
            {
                var addedDevices = new List<MidiEndpoint>();
                var removedDevices = new List<MidiEndpoint>();

                EventHandler<EndpointAddedRemovedEventArgs> addedHandler = (_, e) =>
                {
                    addedDevices.Add(e.Endpoint);

#if TEST
                    e.Endpoint.TestCheckpoints = checkpoints;
#endif
                };

                EndpointsWatcher.Instance.EndpointAdded += addedHandler;

                EventHandler<EndpointAddedRemovedEventArgs> removedHandler = (_, e) =>
                {
                    removedDevices.Add(e.Endpoint);

#if TEST
                    e.Endpoint.TestCheckpoints = checkpoints;
#endif
                };

                EndpointsWatcher.Instance.EndpointRemoved += removedHandler;

                var deviceName = "VD10";
                var timeout = TimeSpan.FromSeconds(5);

                using (var virtualDevice = VirtualDevice.Create(deviceName))
                {
                    var added = WaitOperations.Wait(() => addedDevices.Count >= 2, timeout);
                    ClassicAssert.IsTrue(added, $"Endpoints weren't added for [{timeout}].");

                    ClassicAssert.AreEqual(2, addedDevices.Count, $"Invalid count of added endpoints ({string.Join(", ", addedDevices.Select(d => $"{d.Context}"))}).");

                    var firstAddedDevice = addedDevices.First();
                    ClassicAssert.IsInstanceOf<InputEndpoint>(firstAddedDevice, "Invalid type of the first added endpoint.");
                    ClassicAssert.AreEqual(deviceName, firstAddedDevice.Name, "Invalid name of the first added endpoint.");
                    ClassicAssert.AreEqual("Input endpoint (from 'Device added' notification)", firstAddedDevice.ToString(), "Added input endpoint string representation is invalid.");

                    var lastAddedDevice = addedDevices.Last();
                    ClassicAssert.IsInstanceOf<OutputEndpoint>(lastAddedDevice, "Invalid type of the last added endpoint.");
                    ClassicAssert.AreEqual(deviceName, lastAddedDevice.Name, "Invalid name of the last added endpoint.");
                    ClassicAssert.AreEqual("Output endpoint (from 'Device added' notification)", lastAddedDevice.ToString(), "Added output endpoint string representation is invalid.");
                }

                var removed = WaitOperations.Wait(() => removedDevices.Count >= 2, timeout);
                ClassicAssert.IsTrue(removed, $"Endpoints weren't removed for [{timeout}].");

                ClassicAssert.AreEqual(2, removedDevices.Count, "Invalid count of removed endpoints.");

                var firstRemovedDevice = removedDevices.First();
                ClassicAssert.IsInstanceOf<InputEndpoint>(firstRemovedDevice, "Invalid type of the first removed endpoint.");
                ClassicAssert.AreEqual("Input endpoint (from 'Device removed' notification)", firstRemovedDevice.ToString(), "Removed input endpoint string representation is invalid.");
                ClassicAssert.Throws<InvalidOperationException>(
                    () => { var name = firstRemovedDevice.Name; },
                    "Can get name of removed input endpoint.");
                ClassicAssert.Throws<InvalidOperationException>(
                    () => { var name = ((InputEndpoint)firstRemovedDevice).GetProperty(InputEndpointProperty.Product); },
                    "Can get property value of removed input endpoint.");
                ClassicAssert.Throws<InvalidOperationException>(
                    () => ((InputEndpoint)firstRemovedDevice).StartEventsListening(),
                    "Can start events listening on removed input endpoint.");

                var lastRemovedDevice = removedDevices.Last();
                ClassicAssert.IsInstanceOf<OutputEndpoint>(lastRemovedDevice, "Invalid type of the last removed endpoint.");
                ClassicAssert.AreEqual("Output endpoint (from 'Device removed' notification)", lastRemovedDevice.ToString(), "Removed output endpoint string representation is invalid.");
                ClassicAssert.Throws<InvalidOperationException>(
                    () => { var name = lastRemovedDevice.Name; },
                    "Can get name of removed output endpoint.");
                ClassicAssert.Throws<InvalidOperationException>(
                    () => { var name = ((OutputEndpoint)lastRemovedDevice).GetProperty(OutputEndpointProperty.Product); },
                    "Can get property value of removed output endpoint.");
                ClassicAssert.Throws<InvalidOperationException>(
                    () => ((OutputEndpoint)lastRemovedDevice).SendEvent(new NoteOnEvent()),
                    "Can send event via removed output endpoint.");

                EndpointsWatcher.Instance.EndpointAdded -= addedHandler;
                EndpointsWatcher.Instance.EndpointRemoved -= removedHandler;
            };

#if TEST
            var testCheckpoints = new TestCheckpoints();
            check(testCheckpoints);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var inA = testCheckpoints.GetCheckpointDataList(InputEndpointCheckpointsNames.ReleaseInfoHandleEntered);
            ClassicAssert.AreEqual(2, inA.Count, $"Invalid count of reached checkpoint [{InputEndpointCheckpointsNames.ReleaseInfoHandleEntered}].");

            var inB = testCheckpoints.GetCheckpointDataList(InputEndpointCheckpointsNames.InfoDeletedInReleaseInfoHandle);
            ClassicAssert.AreEqual(2, inB.Count, $"Invalid count of reached checkpoint [{InputEndpointCheckpointsNames.InfoDeletedInReleaseInfoHandle}].");

            var outA = testCheckpoints.GetCheckpointDataList(OutputEndpointCheckpointsNames.ReleaseInfoHandleEntered);
            ClassicAssert.AreEqual(2, outA.Count, $"Invalid count of reached checkpoint [{OutputEndpointCheckpointsNames.ReleaseInfoHandleEntered}].");
            
            var outB = testCheckpoints.GetCheckpointDataList(OutputEndpointCheckpointsNames.InfoDeletedInReleaseInfoHandle);
            ClassicAssert.AreEqual(2, outB.Count, $"Invalid count of reached checkpoint [{OutputEndpointCheckpointsNames.InfoDeletedInReleaseInfoHandle}].");
#else
            check(null);
#endif
        }

        [Test]
        public void CheckEndpointAdded()
        {
            var addedDevices1 = new List<MidiEndpoint>();
            var addedDevices2 = new List<MidiEndpoint>();

            EventHandler<EndpointAddedRemovedEventArgs> addedHandler1 = (_, e) => addedDevices1.Add(e.Endpoint);
            EventHandler<EndpointAddedRemovedEventArgs> addedHandler2 = (_, e) => addedDevices2.Add(e.Endpoint);
            
            EndpointsWatcher.Instance.EndpointAdded += addedHandler1;
            EndpointsWatcher.Instance.EndpointAdded += addedHandler2;

            var deviceName = "VD8";
            var timeout = TimeSpan.FromSeconds(5);

            using (var virtualDevice = VirtualDevice.Create(deviceName))
            {
                var added1 = WaitOperations.Wait(() => addedDevices1.Count >= 2, timeout);
                var added2 = WaitOperations.Wait(() => addedDevices2.Count >= 2, timeout);
                ClassicAssert.IsTrue(added1, $"[A] Endpoints weren't added for [{timeout}] on first collection.");
                ClassicAssert.AreEqual(2, addedDevices1.Count, $"[A] Invalid first count of added endpoints.");
                ClassicAssert.IsTrue(added2, $"[A] Endpoints weren't added for [{timeout}] on second collection.");
                ClassicAssert.AreEqual(2, addedDevices2.Count, $"[A] Invalid second count of added endpoints.");
            }

            EndpointsWatcher.Instance.EndpointAdded -= addedHandler1;
            addedDevices1.Clear();
            addedDevices2.Clear();

            using (var virtualDevice = VirtualDevice.Create(deviceName))
            {
                var added1 = WaitOperations.Wait(() => addedDevices1.Count > 0, timeout);
                var added2 = WaitOperations.Wait(() => addedDevices2.Count >= 2, timeout);
                ClassicAssert.IsFalse(added1, $"[B] Endpoints were added on first collection.");
                ClassicAssert.AreEqual(0, addedDevices1.Count, $"[B] Invalid first count of added endpoints.");
                ClassicAssert.IsTrue(added2, $"[B] Endpoints weren't added for [{timeout}] on second collection.");
                ClassicAssert.AreEqual(2, addedDevices2.Count, $"[B] Invalid second count of added endpoints.");
            }

            EndpointsWatcher.Instance.EndpointAdded -= addedHandler2;
            addedDevices1.Clear();
            addedDevices2.Clear();

            using (var virtualDevice = VirtualDevice.Create(deviceName))
            {
                var added1 = WaitOperations.Wait(() => addedDevices1.Count > 0, timeout);
                var added2 = WaitOperations.Wait(() => addedDevices2.Count > 0, timeout);
                ClassicAssert.IsFalse(added1, $"[C] Endpoints were added on first collection.");
                ClassicAssert.AreEqual(0, addedDevices1.Count, $"[C] Invalid first count of added endpoints.");
                ClassicAssert.IsFalse(added2, $"[C] Endpoints were added on second collection.");
                ClassicAssert.AreEqual(0, addedDevices2.Count, $"[C] Invalid second count of added endpoints.");
            }
        }

        [Test]
        public void CheckEndpointRemoved()
        {
            var removedEndpoints1 = new List<MidiEndpoint>();
            var removedEndpoints2 = new List<MidiEndpoint>();

            EventHandler<EndpointAddedRemovedEventArgs> removedHandler1 = (_, e) => removedEndpoints1.Add(e.Endpoint);
            EventHandler<EndpointAddedRemovedEventArgs> removedHandler2 = (_, e) => removedEndpoints2.Add(e.Endpoint);

            EndpointsWatcher.Instance.EndpointRemoved += removedHandler1;
            EndpointsWatcher.Instance.EndpointRemoved += removedHandler2;

            var deviceName = "VD8";
            var timeout = TimeSpan.FromSeconds(5);

            Thread.Sleep(5000);
            removedEndpoints1.Clear();
            removedEndpoints2.Clear();

            using (var virtualDevice = VirtualDevice.Create(deviceName))
            {
                Thread.Sleep(5000);
            }

            var removed1 = WaitOperations.Wait(() => removedEndpoints1.Count >= 2, timeout);
            var removed2 = WaitOperations.Wait(() => removedEndpoints2.Count >= 2, timeout);
            ClassicAssert.IsTrue(removed1, $"[A] Endpoints weren't removed for [{timeout}] on first collection.");
            ClassicAssert.AreEqual(2, removedEndpoints1.Count, $"[A] Invalid first count of removed endpoints.");
            ClassicAssert.IsTrue(removed2, $"[A] Endpoints weren't removed for [{timeout}] on second collection.");
            ClassicAssert.AreEqual(2, removedEndpoints2.Count, $"[A] Invalid second count of removed endpoints.");

            EndpointsWatcher.Instance.EndpointRemoved -= removedHandler1;

            Thread.Sleep(5000);
            removedEndpoints1.Clear();
            removedEndpoints2.Clear();

            using (var virtualDevice = VirtualDevice.Create(deviceName))
            {
                Thread.Sleep(5000);
            }

            removed1 = WaitOperations.Wait(() => removedEndpoints1.Count > 0, timeout);
            removed2 = WaitOperations.Wait(() => removedEndpoints2.Count >= 2, timeout);
            ClassicAssert.IsFalse(removed1, $"[B] Endpoints were removed on first collection.");
            ClassicAssert.AreEqual(0, removedEndpoints1.Count, $"[B] Invalid first count of removed endpoints.");
            ClassicAssert.IsTrue(removed2, $"[B] Endpoints weren't removed for [{timeout}] on second collection.");
            ClassicAssert.AreEqual(2, removedEndpoints2.Count, $"[B] Invalid second count of removed endpoints.");

            EndpointsWatcher.Instance.EndpointRemoved -= removedHandler2;

            Thread.Sleep(5000);
            removedEndpoints1.Clear();
            removedEndpoints2.Clear();

            using (var virtualDevice = VirtualDevice.Create(deviceName))
            {
                Thread.Sleep(5000);
            }

            removed1 = WaitOperations.Wait(() => removedEndpoints1.Count > 0, timeout);
            removed2 = WaitOperations.Wait(() => removedEndpoints2.Count > 0, timeout);
            ClassicAssert.IsFalse(removed1, $"[C] Endpoints were removed on first collection.");
            ClassicAssert.AreEqual(0, removedEndpoints1.Count, $"[C] Invalid first count of removed endpoints.");
            ClassicAssert.IsFalse(removed2, $"[C] Endpoints were removed on second collection.");
            ClassicAssert.AreEqual(0, removedEndpoints2.Count, $"[C] Invalid second count of removed endpoints.");
        }

        [Test]
        public void CheckEndpointsEqualityFromNotifications_StandaloneEndpoints()
        {
            var addedEndpoints = new List<MidiEndpoint>();
            var removedEndpoints = new List<MidiEndpoint>();

            EventHandler<EndpointAddedRemovedEventArgs> addedHandler = (_, e) => addedEndpoints.Add(e.Endpoint);
            EndpointsWatcher.Instance.EndpointAdded += addedHandler;

            EventHandler<EndpointAddedRemovedEventArgs> removedHandler = (_, e) => removedEndpoints.Add(e.Endpoint);
            EndpointsWatcher.Instance.EndpointRemoved += removedHandler;

            var deviceName = "VD8";
            var timeout = TimeSpan.FromSeconds(5);

            using (var virtualDevice = VirtualDevice.Create(deviceName))
            {
                var added = WaitOperations.Wait(() => addedEndpoints.Count >= 2, timeout);
                ClassicAssert.IsTrue(added, $"Endpoints weren't added for [{timeout}].");
                ClassicAssert.AreEqual(2, addedEndpoints.Count, $"Invalid count of added endpoints.");

                using (var inputEndpoint = InputEndpoint.GetByName(deviceName))
                using (var outputEndpoint = OutputEndpoint.GetByName(deviceName))
                {
                    ClassicAssert.IsTrue(addedEndpoints.Contains(inputEndpoint), "Added endpoints don't contain input endpoint.");
                    ClassicAssert.IsTrue(addedEndpoints.Contains(outputEndpoint), "Added endpoints don't contain output endpoint.");
                }
            }
        }

        [Test]
        public void CheckEndpointsEqualityFromNotifications_VirtualEndpoints()
        {
            var addedEndpoints = new List<MidiEndpoint>();
            var removedEndpoints = new List<MidiEndpoint>();

            EventHandler<EndpointAddedRemovedEventArgs> addedHandler = (_, e) => addedEndpoints.Add(e.Endpoint);
            EndpointsWatcher.Instance.EndpointAdded += addedHandler;

            EventHandler<EndpointAddedRemovedEventArgs> removedHandler = (_, e) => removedEndpoints.Add(e.Endpoint);
            EndpointsWatcher.Instance.EndpointRemoved += removedHandler;

            var deviceName = "VD8";
            var timeout = TimeSpan.FromSeconds(5);

            using (var virtualDevice = VirtualDevice.Create(deviceName))
            {
                var added = WaitOperations.Wait(() => addedEndpoints.Count >= 2, timeout);
                ClassicAssert.IsTrue(added, $"Endpoints weren't added for [{timeout}].");
                ClassicAssert.AreEqual(2, addedEndpoints.Count, $"Invalid count of added endpoints.");

                ClassicAssert.IsTrue(addedEndpoints.Contains(virtualDevice.InputEndpoint), "Added endpoints don't contain input endpoint.");
                ClassicAssert.IsTrue(addedEndpoints.Contains(virtualDevice.OutputEndpoint), "Added endpoints don't contain output endpoint.");
            }
        }

        #endregion
    }
}
