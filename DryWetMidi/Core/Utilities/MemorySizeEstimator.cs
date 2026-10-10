using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Estimates size of managed objects in memory (64-bit layout) by inspecting their instance fields.
    /// </summary>
    internal static class MemorySizeEstimator
    {
        #region Nested classes

        private sealed class TypeLayout
        {
            public TypeLayout(long size, FieldInfo[] fieldsToWalk)
            {
                Size = size;
                FieldsToWalk = fieldsToWalk;
            }

            public long Size { get; }

            public FieldInfo[] FieldsToWalk { get; }
        }

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);

            public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
        }

        #endregion

        #region Constants

        private const int PointerSize = 8;
        private const int ObjectHeaderSize = 16;
        private const int ArrayHeaderSize = 24;
        private const int StringHeaderSize = 22;
        private const int MaxDepth = 64;

        // Size of a slot in a collection (list, array) referencing an object
        internal const int ReferenceSlotSize = PointerSize;

        #endregion

        #region Fields

        private static readonly ConcurrentDictionary<Type, TypeLayout> _layouts = new ConcurrentDictionary<Type, TypeLayout>();
        private static readonly ConcurrentDictionary<Type, long> _valueTypeSizes = new ConcurrentDictionary<Type, long>();

        #endregion

        #region Methods

        /// <summary>
        /// Estimates size of the object itself without objects it references.
        /// </summary>
        internal static long EstimateShallow(object obj)
        {
            return GetLayout(obj.GetType()).Size;
        }

        /// <summary>
        /// Estimates size of the object including all objects reachable from it (strings, arrays, nested objects).
        /// </summary>
        internal static long EstimateDeep(object obj)
        {
            var layout = GetLayout(obj.GetType());
            if (layout.FieldsToWalk.Length == 0)
                return layout.Size;

            return Walk(obj, new HashSet<object>(new ReferenceComparer()), 0);
        }

        internal static long EstimateArray(Type elementType, long length)
        {
            var elementSize = elementType.IsValueType ? GetValueTypeSize(elementType) : PointerSize;
            return Align(ArrayHeaderSize + elementSize * length);
        }

        internal static long EstimateString(int length)
        {
            return Align(StringHeaderSize + 2L * length);
        }

        private static long Walk(object obj, HashSet<object> visited, int depth)
        {
            if (!visited.Add(obj))
                return 0;

            if (obj is string str)
                return EstimateString(str.Length);

            var type = obj.GetType();

            if (obj is Array array)
            {
                var elementType = type.GetElementType()!;
                var result = EstimateArray(elementType, array.LongLength);
                if (depth >= MaxDepth)
                    return result;

                if (!elementType.IsValueType)
                {
                    foreach (var item in array)
                    {
                        if (item != null)
                            result += Walk(item, visited, depth + 1);
                    }
                }
                else if (GetFieldsToWalk(elementType).Length > 0)
                {
                    foreach (var item in array)
                    {
                        result += WalkFields(item, visited, depth + 1);
                    }
                }

                return result;
            }

            if (obj is Delegate || obj is Type || obj is MemberInfo)
                return ObjectHeaderSize + PointerSize;

            var layout = GetLayout(type);
            return layout.Size + (depth >= MaxDepth ? 0 : WalkFields(obj, visited, depth));
        }

        private static long WalkFields(object obj, HashSet<object> visited, int depth)
        {
            long result = 0;

            foreach (var field in GetFieldsToWalk(obj.GetType()))
            {
                var value = field.GetValue(obj);
                if (value == null)
                    continue;

                result += field.FieldType.IsValueType
                    ? WalkFields(value, visited, depth + 1)
                    : Walk(value, visited, depth + 1);
            }

            return result;
        }

        private static TypeLayout GetLayout(Type type)
        {
            return _layouts.GetOrAdd(type, CreateLayout);
        }

        private static FieldInfo[] GetFieldsToWalk(Type type)
        {
            return GetLayout(type).FieldsToWalk;
        }

        private static TypeLayout CreateLayout(Type type)
        {
            long size = 0;
            var fieldsToWalk = new List<FieldInfo>();

            foreach (var field in GetInstanceFields(type))
            {
                var fieldType = field.FieldType;
                size += fieldType.IsValueType ? GetValueTypeSize(fieldType) : PointerSize;

                if (!fieldType.IsValueType || (!fieldType.IsPrimitive && !fieldType.IsEnum && GetFieldsToWalk(fieldType).Length > 0))
                    fieldsToWalk.Add(field);
            }

            return new TypeLayout(
                type.IsValueType ? size : Math.Max(ObjectHeaderSize + PointerSize, Align(ObjectHeaderSize + size)),
                fieldsToWalk.ToArray());
        }

        private static long GetValueTypeSize(Type type)
        {
            if (type.IsEnum)
                type = Enum.GetUnderlyingType(type);

            if (type.IsPrimitive)
            {
                if (type == typeof(bool) || type == typeof(byte) || type == typeof(sbyte))
                    return 1;
                if (type == typeof(char) || type == typeof(short) || type == typeof(ushort))
                    return 2;
                if (type == typeof(int) || type == typeof(uint) || type == typeof(float))
                    return 4;
                return 8;
            }

            return _valueTypeSizes.GetOrAdd(type, t =>
            {
                long size = 0;
                long maxFieldSize = 1;

                foreach (var field in GetInstanceFields(t))
                {
                    var fieldSize = field.FieldType.IsValueType ? GetValueTypeSize(field.FieldType) : PointerSize;
                    size += fieldSize;
                    maxFieldSize = Math.Max(maxFieldSize, Math.Min(fieldSize, PointerSize));
                }

                size = Math.Max(size, 1);
                return (size + maxFieldSize - 1) / maxFieldSize * maxFieldSize;
            });
        }

        [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Fields are used only to estimate size; missing fields just make the estimate less accurate.")]
        [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Fields are used only to estimate size; missing fields just make the estimate less accurate.")]
        private static IEnumerable<FieldInfo> GetInstanceFields(Type type)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            var types = new Stack<Type>();
            for (var t = type; t != null && t != typeof(object) && t != typeof(ValueType); t = t.BaseType)
            {
                types.Push(t);
            }

            while (types.Count > 0)
            {
                foreach (var field in types.Pop().GetFields(flags))
                {
                    yield return field;
                }
            }
        }

        private static long Align(long size)
        {
            return (size + 7) / 8 * 8;
        }

        #endregion
    }
}
