using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Parallel.Worker.Interface;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker
{
    public class TaskExecutor : Executor<CancellationTokenSource>
    {
        protected override CancellationTokenSource CreateFutureCompanion()
        {
            return CreateFutureCompanionGeneric();
        }

        internal static CancellationTokenSource CreateFutureCompanionGeneric()
        {
            return new CancellationTokenSource();
        }

        protected override Future<TResult> CreateFuture<TResult>(CancellationTokenSource companion)
        {
            return CreateFutureGeneric<TResult>(companion);
        }

        internal static Future<TResult> CreateFutureGeneric<TResult>(CancellationTokenSource companion)
            where TResult : class
        {
            Action cancel = companion.Cancel;
            return new Future<TResult>(cancel);
        }

        /// <summary>
        /// Invokes an instruction and completes the corresponding future accordingly, until completion.
        /// Process is run as a new Task.
        /// </summary>
        /// <typeparam name="TArgument"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="future"></param>
        /// <param name="companion"></param>
        /// <param name="safeInstruction"></param>
        protected override void CompleteFuture<TArgument, TResult>(Future<TResult> future,
                                                                   CancellationTokenSource companion,
                                                                   SafeInstruction<TArgument, TResult> safeInstruction)
        {
            CompleteFutureGeneric(future, companion, safeInstruction, base.CompleteFuture);
        }

        /// <summary>
        /// Used by both the generic and non-generic class
        /// </summary>
        /// <typeparam name="TArgument"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="future"></param>
        /// <param name="companion"></param>
        /// <param name="safeInstruction"></param>
        /// <param name="completeFuture"></param>
        internal static void CompleteFutureGeneric<TArgument, TResult>(Future<TResult> future,
                                                                       CancellationTokenSource companion,
                                                                       SafeInstruction<TArgument, TResult> safeInstruction,
                                                                       Action<Future<TResult>, CancellationTokenSource, SafeInstruction<TArgument, TResult>> completeFuture)
            where TArgument : class
            where TResult : class
        {
            Task.Factory.StartNew(() => completeFuture(future, companion, safeInstruction), companion.Token);
        }
    }

    public class TaskExecutor<TArgument, TResult> : Executor<TArgument, TResult, CancellationTokenSource>
        where TArgument : class
        where TResult : class
    {
        protected override CancellationTokenSource CreateFutureCompanion()
        {
            return TaskExecutor.CreateFutureCompanionGeneric();
        }

        protected override Future<TResult> CreateFuture(CancellationTokenSource companion)
        {
            return TaskExecutor.CreateFutureGeneric<TResult>(companion);
        }

        protected override void CompleteFuture(Future<TResult> future,
                                               CancellationTokenSource companion,
                                               SafeInstruction<TArgument, TResult> safeInstruction)
        {
            TaskExecutor.CompleteFutureGeneric(future, companion, safeInstruction, base.CompleteFuture);
        }
    }
}
