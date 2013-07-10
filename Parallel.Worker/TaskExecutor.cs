using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parallel.Worker.Interface;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker
{
    public class TaskExecutor : Executor
    {
        protected override Future<TResult> CreateFuture<TResult>()
        {
            return CreateFutureGeneric<TResult>();
        }

        internal static Future<TResult> CreateFutureGeneric<TResult>()
            where TResult : class
        {
            return new Future<TResult>();
        }

        /// <summary>
        /// Invokes an instruction and completes the corresponding future accordingly, until completion.
        /// Process is run as a new Task.
        /// </summary>
        /// <typeparam name="TArgument"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="future"></param>
        /// <param name="safeInstruction"></param>
        protected override void CompleteFuture<TArgument, TResult>(Future<TResult> future,
                                                                   SafeInstruction<TArgument, TResult> safeInstruction)
        {
            CompleteFutureGeneric(future, safeInstruction, base.CompleteFuture);
        }

        /// <summary>
        /// Used by both the generic and non-generic class
        /// </summary>
        /// <typeparam name="TArgument"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="future"></param>
        /// <param name="safeInstruction"></param>
        internal static void CompleteFutureGeneric<TArgument, TResult>(Future<TResult> future,
                                                                       SafeInstruction<TArgument, TResult> safeInstruction,
                                                                       Action<Future<TResult>, SafeInstruction<TArgument, TResult>> completeFuture)
            where TArgument : class
            where TResult : class
        {
            Task.Factory.StartNew(() => completeFuture(future, safeInstruction));
        }
    }

    public class TaskExecutor<TArgument, TResult> : Executor<TArgument, TResult>
        where TArgument : class
        where TResult : class
    {
        protected override Future<TResult> CreateFuture()
        {
            return TaskExecutor.CreateFutureGeneric<TResult>();
        }

        protected override void CompleteFuture(Future<TResult> future,
                                               SafeInstruction<TArgument, TResult> safeInstruction)
        {
            TaskExecutor.CompleteFutureGeneric(future, safeInstruction, base.CompleteFuture);
        }
    }
}
