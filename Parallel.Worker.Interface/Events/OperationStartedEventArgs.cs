using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parallel.Worker.Interface.Events
{
    public class OperationStartedEventArgs : EventArgs
    {
        public OperationStartedEventArgs(Guid operationId)
        {
            OperationId = operationId;
        }

        public Guid OperationId { get; private set; }
    }
}
