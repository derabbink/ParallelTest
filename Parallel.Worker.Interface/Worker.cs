using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parallel.Worker.Interface.Events;
using Parallel.Worker.Interface.Parallel;

namespace Parallel.Worker.Interface
{
    public abstract class Worker
    {
        private IExecutor _executor;

        public Worker()
        {
            _executor = new ThreadExecutor();
        }

        public event EventHandler<OperationStartedEventArgs> OperationStarted;
        public event EventHandler<OperationCompletedEventArgs> OperationCompleted;

        /// <summary>
        /// Executes an operation
        /// </summary>
        /// <param name="operation">operation to be executed by worker</param>
        /// <param name="arg">argument to supply to operation</param>
        /// <returns>id identifying the submitted operation</returns>
        public Guid Execute(Func<object, object> operation, object arg)
        {
            Guid id = new Guid();

            Operation op = new Operation(operation, arg);
            _executor.Execute(op);

            return id;
        }

        private EventHandler<OperationStartedEventArgs> CreateStartedListener()
        {
            Worker workerClosure = this;
            return (sender, args) => OperationStarted(workerClosure, args);
        }

        private EventHandler<OperationCompletedEventArgs> CreateCompletedListener()
        {
            Worker workerClosure = this;
            return (sender, args) => OperationCompleted(workerClosure, args);
        }
    }
}
