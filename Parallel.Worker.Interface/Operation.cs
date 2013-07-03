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
        private readonly Guid _token;
        private readonly object _arg;
        private object _result;

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

        /// <summary>
        /// Starts execution of the internal operation method
        /// </summary>
        internal void Execute()
        {
            RaiseStartedEvent();
            ExecuteContained();
            RaiseCompletedEvent();
        }

        /// <summary>
        /// Executes internal operation safely
        /// contains all possible exceptions
        /// </summary>
        private void ExecuteContained()
        {
            try
            {
                _result = _operation(_arg);
            }
            catch
            {
                
            }
        }

        #region Raising events

        private void RaiseStartedEvent()
        {
            if (Started != null)
                Started(this, new OperationStartedEventArgs(_token));
        }

        private void RaiseCompletedEvent()
        {
            if (Completed != null)
                Completed(this, new OperationCompletedEventArgs(_token));
        }

        #endregion
    }
}
