using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parallel.Worker.Interface.Rx
{
    /// <summary>
    /// Rx message for any kind of executed operation,
    /// including those that failed with exceptions
    /// </summary>
    internal class OperationCompleted : OperationProgress
    {
        internal OperationCompleted(OperationResult result, Guid operationId) : base(operationId)
        {
            Result = result;
        }

        internal OperationResult Result { get; private set; }
    }
}
