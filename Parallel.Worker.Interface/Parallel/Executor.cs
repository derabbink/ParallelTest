using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parallel.Worker.Interface.Events;
using Parallel.Worker.Interface.Rx;
using Parallel.Worker.Interface.Util;

namespace Parallel.Worker.Interface.Parallel
{
    internal abstract class Executor
    {
        internal event EventHandler<OperationStartedEventArgs> OperationStarted;
        internal event EventHandler<OperationCompletedEventArgs> OperationCompleted;

        internal abstract void Execute(Operation operation, Guid id);

        protected void ExecuteWithEvents(Operation operation, Guid id)
        {
            RaiseStartedEvent(id);
            var result = operation.Execute();
            RaiseCompletedEvent(result, id);
        }

        private void RaiseStartedEvent(Guid operationId)
        {
            if (OperationStarted != null)
                OperationStarted.Raise(this, new OperationStartedEventArgs(operationId));
        }

        private void RaiseCompletedEvent(OperationResult result, Guid operationId)
        {
            if (OperationCompleted != null)
                OperationCompleted.Raise(this, new OperationCompletedEventArgs(result, operationId));
        }
    }
}
