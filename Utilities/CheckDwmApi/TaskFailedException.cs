using System;

namespace Melanchall.CheckDwmApi
{
    internal sealed class TaskFailedException : Exception
    {
        public TaskFailedException(string message)
            : base(message)
        {
        }

        public TaskFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
