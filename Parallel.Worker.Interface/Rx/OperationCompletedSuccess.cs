using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parallel.Worker.Interface.Rx
{
    /// <summary>
    /// Rx message for successfully completed operations
    /// contains return value
    /// </summary>
    public class OperationCompletedSuccess : OperationProgress
    {
        public OperationCompletedSuccess(object result, Guid operationId) : base(operationId)
        {
            Result = result;
        }

        public object Result { get; private set; }
    }
}
