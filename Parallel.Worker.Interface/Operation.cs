using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parallel.Worker.Interface.Events;

namespace Parallel.Worker.Interface
{
    internal class Operation
    {
        private readonly Func<object, object> _operation;
        private readonly object _arg;
        private readonly Guid _token;

        internal event EventHandler<OperationStartedEventArgs> Started;
        internal event EventHandler<OperationCompletedEventArgs> Completed;

        internal Operation(Func<object, object> operation, object arg, Guid token)
        {
            _operation = operation;
            _arg = arg;
            _token = token;
        }

        internal static Operation CreateWithListeners(Func<object, object> operation, object arg,
                                                      Guid token,
                                                      EventHandler<OperationStartedEventArgs> startedHandler,
                                                      EventHandler<OperationCompletedEventArgs> completedHandler)
        {
            Operation result = new Operation(operation, arg, token);
            result.Started += startedHandler;
            result.Completed += completedHandler;
            return result;
        }

        internal void Execute()
        {
            Started(this, new OperationStartedEventArgs(_token));
            _operation(_arg);
            Completed(this, new OperationCompletedEventArgs(_token));
        }
    }
}
