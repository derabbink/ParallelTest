using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parallel.Worker.Interface.Events
{
    internal class OperationCompletedEventArgs : EventArgs
    {
        internal OperationCompletedEventArgs(OperationResult result, Guid operationId)
        {
            Result = result;
            OperationId = operationId;
        }

        internal OperationResult Result { get; private set; }

        internal Guid OperationId { get; private set; }
    }
}
