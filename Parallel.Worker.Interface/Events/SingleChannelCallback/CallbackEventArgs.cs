using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker.Interface.Events.SingleChannelCallback
{
    public class CallbackEventArgs<TResult> : EventArgs
        where TResult : class
    {
        public CallbackEventArgs(Guid operationId, Future<TResult> result)
        {
            Contract.Requires(result.IsDone, "result Status should be Faulted or RanToCompletion");

            OperationId = operationId;
            Result = result;
        }

        public Guid OperationId { get; private set; }

        public Future<TResult> Result { get; private set; }
    }
}
