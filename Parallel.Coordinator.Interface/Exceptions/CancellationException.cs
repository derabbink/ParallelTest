using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parallel.Coordinator.Interface.Exceptions
{
    public class CancellationException : Exception
    {
        public CancellationException() : base() {}

        public CancellationException(string message) : base(message) {}

        public CancellationException(string message, Exception innerException) : base(message, innerException) {}
    }
}
