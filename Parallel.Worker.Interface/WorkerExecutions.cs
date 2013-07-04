using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parallel.Worker.Interface.Rx;
using Parallel.Worker.Interface.Util;

namespace Parallel.Worker.Interface
{
    public static class WorkerExecutions
    {
        /// <summary>
        /// Executes an operation
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        public static IObservable<OperationProgress> Execute(this Worker worker, Func<object> operation)
        {
            return worker.Execute(OperationWrapper.Wrap(operation), null);
        }

        /// <summary>
        /// Executes an operation
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="operation"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static IObservable<OperationProgress> Execute(this Worker worker, Action<object> operation, object arg)
        {
            return worker.Execute(OperationWrapper.Wrap(operation), arg);
        }

        /// <summary>
        /// Executes an operation
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        public static IObservable<OperationProgress> Execute(this Worker worker, Action operation)
        {
            return worker.Execute(OperationWrapper.Wrap(operation), null);
        }
    }
}
