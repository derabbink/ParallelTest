using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parallel.Worker.Interface.Events
{
    internal class OperationStartedEventArgs : EventArgs
    {
        internal OperationStartedEventArgs(Guid operationId)
        {
            OperationId = operationId;
        }

        internal Guid OperationId { get; private set; }
    }
}
