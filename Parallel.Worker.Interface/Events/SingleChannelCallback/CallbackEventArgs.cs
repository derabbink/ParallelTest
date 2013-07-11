using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker.Interface.Events.SingleChannelCallback
{
    public class CallbackEventArgs<TResult> : EventArgs
        where TResult : class
    {
        public CallbackEventArgs(Guid operationId, SafeInstructionResult<TResult> result)
        {
            OperationId = operationId;
            Result = result;
        }

        public Guid OperationId { get; private set; }

        public SafeInstructionResult<TResult> Result { get; private set; }
    }
}
