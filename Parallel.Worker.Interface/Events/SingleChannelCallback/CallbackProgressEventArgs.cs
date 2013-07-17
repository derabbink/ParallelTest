using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parallel.Worker.Interface.Events.SingleChannelCallback
{
    public class CallbackProgressEventArgs : EventArgs
    {
        public CallbackProgressEventArgs(Guid operationId)
        {
            OperationId = operationId;
        }

        public Guid OperationId { get; private set; }
    }
}
