using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Executes operations, and wraps them in observable sequences
    /// </summary>
    public class Worker
    {
        private Executor _executor;
        private IObservable<OperationStarted> _obsStarted;
        private IObservable<OperationCompleted> _obsCompleted;

        public Worker()
        {
            _executor = new TaskExecutor();
            CreateObservables();
        }

        /// <summary>
        /// wraps notification events in observables to closely resemble
        /// wcf callback architecture (which we'll explore later)
        /// </summary>
        private void CreateObservables()
        {
            var started =
                Observable.FromEventPattern<OperationStartedEventArgs>(handler => _executor.OperationStarted += handler,
                                                                       handler => _executor.OperationStarted -= handler)
                          .Select(ep => new OperationStarted(ep.EventArgs.OperationId)).Publish();
            started.Connect();
            _obsStarted = started;

            var completed =
                Observable.FromEventPattern<OperationCompletedEventArgs>(handler => _executor.OperationCompleted += handler,
                                                                         handler => _executor.OperationCompleted -= handler)
                          .Select(ep => new OperationCompleted(ep.EventArgs.Result, ep.EventArgs.OperationId)).Publish();
            completed.Connect();
            _obsCompleted = completed;
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
            var result = _obsStarted.ForOperation(id).Take(1)
                .MergeOperation(_obsCompleted.ForOperation(id).Take(1))
                .Replay();
            result.Connect();

            Operation op = new Operation(operation, arg);
            _executor.Execute(op, id);

            return result;
        }
    }
}
