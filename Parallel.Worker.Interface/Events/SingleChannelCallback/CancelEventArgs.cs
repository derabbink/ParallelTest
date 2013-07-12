using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parallel.Worker.Interface.Events.SingleChannelCallback
{
    public class CancelEventArgs : EventArgs
    {
        public CancelEventArgs(Guid operationId)
        {
            OperationId = operationId;
        }

        public Guid OperationId { get; private set; }
    }
}
