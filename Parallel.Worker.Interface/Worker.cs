using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Parallel.Worker.Interface.Events;
using Parallel.Worker.Interface.Parallel;
using Parallel.Worker.Interface.Rx;

namespace Parallel.Worker.Interface
{
    /// <summary>
    /// Executes an operation, and wraps the execution in an observable sequence
    /// </summary>
    public abstract class Worker
    {
        private Executor _executor;
        private IObservable<OperationProgress> _obsStarted;
        private IObservable<OperationProgress> _obsCompleted;

        public Worker()
        {
            _executor = new TaskExecutor();
            CreateObservables();
        }

        private void CreateObservables()
        {
            _obsStarted =
                Observable.FromEventPattern<OperationStartedEventArgs>(handler => _executor.OperationStarted += handler,
                                                                       handler => _executor.OperationStarted -= handler)
                          .Select(ep => new OperationStarted(ep.EventArgs.OperationId));
            _obsCompleted =
                Observable.FromEventPattern<OperationCompletedEventArgs>(handler => _executor.OperationCompleted += handler,
                                                                         handler => _executor.OperationCompleted -= handler)
                          .Select(ep => new OperationCompleted(ep.EventArgs.Result, ep.EventArgs.OperationId));
        }

        /// <summary>
        /// Executes an operation
        /// </summary>
        /// <param name="operation">operation to be executed by worker</param>
        /// <param name="arg">argument to supply to operation</param>
        /// <returns>observable sequence containing notifications about the operation's progress</returns>
        public IObservable<OperationProgress> Execute(Func<object, object> operation, object arg)
        {
            Guid id = new Guid();
            var result = new ReplaySubject<OperationProgress>();
            _obsStarted.ForOperation(id).Take(1)
                .Merge(_obsCompleted.ForOperation(id).Take(1))
                .Subscribe(result);

            Operation op = new Operation(operation, arg);
            _executor.Execute(op, id);

            return result;
        }
    }
}
