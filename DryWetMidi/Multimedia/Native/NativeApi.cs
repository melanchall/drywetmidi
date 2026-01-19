using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal static class NativeApi
    {
        #region Nested classes

        [AttributeUsage(AttributeTargets.Field)]
        public sealed class NativeErrorTypeAttribute : Attribute
        {
            public NativeErrorTypeAttribute(NativeErrorType errorType)
            {
                ErrorType = errorType;
            }

            public NativeErrorType ErrorType { get; }
        }

        #endregion

        #region Delegates

        public delegate TException CreateException<TException>(
            string message,
            int mainErrorCode,
            int additionalErrorCode)
            where TException : Exception;

        #endregion

        #region Enums

        public enum NativeErrorType
        {
            NoMemory,
            InUse,
            NotPermitted,
            Busy
        }

        public enum MidiMessage
        {
            MIM_CLOSE = 962,
            MIM_DATA = 963,
            MIM_ERROR = 965,
            MIM_LONGDATA = 964,
            MIM_LONGERROR = 966,
            MIM_MOREDATA = 972,
            MIM_OPEN = 961,
            MOM_CLOSE = 968,
            MOM_DONE = 969,
            MOM_OPEN = 967,
            MOM_POSITIONCB = 970
        }

        #endregion

        #region Constants

        public const string LibraryName = "Melanchall_DryWetMidi_Native";

        private static readonly Dictionary<NativeErrorType, string> ErrorsDescriptions = new Dictionary<NativeErrorType, string>
        {
            [NativeErrorType.NoMemory] = "There is no memory in the system to complete the operation",
            [NativeErrorType.InUse] = "Device is already in use",
            [NativeErrorType.NotPermitted] = "The process doesn’t have privileges for the requested operation",
            [NativeErrorType.Busy] = "The hardware is busy with other data"
        };

        #endregion

        #region Methods

        public static string GetStringFromPointer(IntPtr stringPointer)
        {
            return stringPointer != IntPtr.Zero
                ? Marshal.PtrToStringAnsi(stringPointer)
                : string.Empty;
        }

        public static void HandleResult<TResult, TException>(
            TResult result,
            int errorCode,
            CreateException<TException> createException)
            where TException : Exception
        {
            var resultCode = Convert.ToInt32(result);
            if (resultCode == 0)
                return;

            var attribute = typeof(TResult)
                .GetFields(BindingFlags.Static | BindingFlags.Public)
                .First(f => f.GetValue(null).Equals(result))
                .GetCustomAttributes(typeof(NativeErrorTypeAttribute))
                .FirstOrDefault() as NativeErrorTypeAttribute;

            var errorDescription = attribute != null
                ? ErrorsDescriptions[attribute.ErrorType]
                : "Internal error";
            throw createException($"{errorDescription} ({result}, {errorCode}).", resultCode, errorCode);
        }

        #endregion
    }
}
