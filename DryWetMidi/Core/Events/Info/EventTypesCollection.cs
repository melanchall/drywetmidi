using Melanchall.DryWetMidi.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Collection of <see cref="EventType"/> objects which provide identity information of an event.
    /// </summary>
    public sealed class EventTypesCollection : IEnumerable<EventType>
    {
        #region Fields

        private readonly Dictionary<Type, byte> _typesToStatusBytes = new Dictionary<Type, byte>();
        private readonly Dictionary<byte, Type> _statusBytesToTypes = new Dictionary<byte, Type>();

        #endregion

        #region Methods

        /// <summary>
        /// Adds event type along with the corresponding status byte.
        /// </summary>
        /// <param name="type">Type of event.</param>
        /// <param name="statusByte">Status byte of event.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Event type specified by <paramref name="type"/> and
        /// <paramref name="statusByte"/> already exists in the <see cref="EventsCollection"/>.</exception>
        public void Add([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type, byte statusByte)
        {
            ThrowIfArgument.IsNull(nameof(type), type);

            if (_typesToStatusBytes.ContainsKey(type) || _statusBytesToTypes.ContainsKey(statusByte))
                throw new ArgumentException($"Event type '{type.Name}' or status byte '{statusByte}' already exists in the collection.");

            _typesToStatusBytes.Add(type, statusByte);
            _statusBytesToTypes.Add(statusByte, type);
        }

        // TODO: maybe internal?
        /// <summary>
        /// Gets the event type associated with the specified status byte.
        /// </summary>
        /// <param name="statusByte">The status byte of the event type to get.</param>
        /// <param name="type">When this method returns, contains the event type associated with
        /// the specified status byte, if the status byte is found; otherwise, <c>null</c>. This parameter
        /// is passed uninitialized.</param>
        /// <returns><c>true</c> if the <see cref="EventTypesCollection"/> contains an event type with the
        /// specified status byte; otherwise, <c>false</c>.</returns>
        [UnconditionalSuppressMessage("Trimming", "IL2067", Justification = "All types stored in this collection are guaranteed to have public parameterless constructors via the Add method's annotation.")]
        public bool TryGetType(byte statusByte, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)][NotNullWhen(true)] out Type? type)
        {
            return _statusBytesToTypes.TryGetValue(statusByte, out type);
        }

        // TODO: maybe internal?
        /// <summary>
        /// Gets the status byte associated with the specified event type.
        /// </summary>
        /// <param name="type">Event type to get status byte for.</param>
        /// <param name="statusByte">When this method returns, contains the status byte associated with
        /// the specified event type, if the type is found; otherwise, 0. This parameter is passed
        /// uninitialized.</param>
        /// <returns><c>true</c> if the <see cref="EventTypesCollection"/> contains a status byte for the
        /// specified event type; otherwise, <c>false</c>.</returns>
        public bool TryGetStatusByte([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type, out byte statusByte)
        {
            return _typesToStatusBytes.TryGetValue(type, out statusByte);
        }

        #endregion

        #region IEnumerable<EventType>

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<EventType> GetEnumerator()
        {
            return _typesToStatusBytes
                .Select(kv => new EventType(kv.Key, kv.Value))
                .GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
