using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parallel.Worker.Interface.Rx
{
    /// <summary>
    /// Rx message for reporting operation progress
    /// </summary>
    public abstract class OperationProgress
    {
        public OperationProgress(Guid operationId)
        {
            OperationId = operationId;
        }

        public Guid OperationId { get; private set; }
    }
}
