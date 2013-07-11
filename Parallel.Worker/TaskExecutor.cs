using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Parallel.Worker.Interface;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker
{
    public class TaskExecutor : Executor
    {
        /// <summary>
        /// completes a future in parallel.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="future"></param>
        protected override void CompleteFuture<TResult>(Future<SafeInstructionResult<TResult>> future)
        {
            CompleteFutureGeneric(future);
        }

        internal new static void CompleteFutureGeneric<TResult>(Future<SafeInstructionResult<TResult>> future)
            where TResult : class
        {
            future.Start();
        }
    }

    public class TaskExecutor<TArgument, TResult> : Executor<TArgument, TResult>
        where TArgument : class
        where TResult : class
    {
        /// <summary>
        /// completes a future in parallel
        /// </summary>
        /// <param name="future"></param>
        protected override void CompleteFuture(Future<SafeInstructionResult<TResult>> future)
        {
            TaskExecutor.CompleteFutureGeneric(future);
        }
    }
}
