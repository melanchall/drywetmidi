using System;
using System.Linq;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class RegisteredParametersTests
    {
        #region Test methods

        [Test]
        public void EnsureAllParametersHaveDifferentTypes()
        {
            var registeredParametersTypes = GetParametersTypes();
            Assert.AreEqual(Enum.GetValues(typeof(RegisteredParameterType)).Length, registeredParametersTypes.Length, "Invalid count of registered parameters types.");

            var types = registeredParametersTypes
                .Select(t => Activator.CreateInstance(t))
                .OfType<RegisteredParameter>()
                .Select(p => p.ParameterType)
                .Distinct()
                .ToArray();
            Assert.AreEqual(registeredParametersTypes.Length, types.Length, "Count of used parameters types is invalid.");
        }

        [Test]
        public void AllParametersTypesHaveParameterlessConstructor()
        {
            foreach (var type in GetParametersTypes())
            {
                Assert.IsNotNull(
                    type.GetConstructor(Type.EmptyTypes),
                    $"Type '{type.Name}' has no parameterless constructor.");
            }
        }

        [Test]
        public void CheckTimeChangedEvent_ZeroTime_NoChange()
        {
            CheckTimeChangedEvent_NoChange(0);
        }

        [Test]
        public void CheckTimeChangedEvent_NonZeroTime_NoChange()
        {
            CheckTimeChangedEvent_NoChange(100);
        }

        [Test]
        public void CheckTimeChangedEvent_ZeroTime_Changed()
        {
            CheckTimeChangedEvent_Changed(0);
        }

        [Test]
        public void CheckTimeChangedEvent_NonZeroTime_Changed()
        {
            CheckTimeChangedEvent_Changed(100);
        }

        #endregion

        #region Private methods

        private static void CheckTimeChangedEvent_NoChange(long initialTime)
        {
            var registeredParameters = GetParameters();
            registeredParameters.ToList().ForEach(p => p.Time = initialTime);

            foreach (var registeredParameter in registeredParameters)
            {
                object timeChangedSender = null;
                TimeChangedEventArgs timeChangedEventArgs = null;

                registeredParameter.TimeChanged += (sender, eventArgs) =>
                {
                    timeChangedSender = sender;
                    timeChangedEventArgs = eventArgs;
                };

                registeredParameter.Time = registeredParameter.Time;

                Assert.IsNull(timeChangedSender, "Sender is not null.");
                Assert.IsNull(timeChangedEventArgs, "Event args is not null.");
            }
        }

        private static void CheckTimeChangedEvent_Changed(long initialTime)
        {
            var registeredParameters = GetParameters();
            registeredParameters.ToList().ForEach(p => p.Time = initialTime);

            foreach (var registeredParameter in registeredParameters)
            {
                object timeChangedSender = null;
                TimeChangedEventArgs timeChangedEventArgs = null;

                registeredParameter.TimeChanged += (sender, eventArgs) =>
                {
                    timeChangedSender = sender;
                    timeChangedEventArgs = eventArgs;
                };

                var oldTime = registeredParameter.Time;
                registeredParameter.Time += 100;

                Assert.AreSame(registeredParameter, timeChangedSender, "Sender is invalid.");

                Assert.IsNotNull(timeChangedEventArgs, "Event args is null.");
                Assert.AreEqual(oldTime, timeChangedEventArgs.OldTime, "Old time is invalid.");
                Assert.AreEqual(registeredParameter.Time, timeChangedEventArgs.NewTime, "New time is invalid.");
                Assert.AreNotEqual(oldTime, registeredParameter.Time, "New time is equal to old one.");
            }
        }

        private static RegisteredParameter[] GetParameters()
        {
            return GetParametersTypes()
                .Select(t => (RegisteredParameter)Activator.CreateInstance(t))
                .ToArray();
        }

        private static Type[] GetParametersTypes()
        {
            var registeredParameterBaseType = typeof(RegisteredParameter);
            return registeredParameterBaseType
                .Assembly
                .GetTypes()
                .Where(t => registeredParameterBaseType.IsAssignableFrom(t) && !t.IsAbstract)
                .ToArray();
        }

        #endregion
    }
}
