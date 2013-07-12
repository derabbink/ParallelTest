using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Parallel.Worker.Interface;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker
{
    /// <summary>
    /// Default executor implementation. Executes instructions sequentially.
    /// Can deal with different argument and return types, each Execute
    /// Also contains static default implementation for virtual methods
    /// </summary>
    public class Executor : IExecutor
    {
        /// <summary>
        /// Executes the instruction and returns a future representing the result
        /// </summary>
        /// <typeparam name="TArgument"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="instruction"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public Future<TResult> Execute<TArgument, TResult>(Func<CancellationToken, TArgument, TResult> instruction, TArgument argument)
            where TArgument : class
            where TResult : class
        {
            return ExecuteGeneric(instruction, argument, ApplyArgument, CompleteFuture);
        }

        internal static Future<TResult> ExecuteGeneric<TArgument, TResult>(
                Func<CancellationToken, TArgument, TResult> instruction,
                TArgument argument,
                Func<Func<CancellationToken, TArgument, TResult>, TArgument, Func<CancellationToken, TResult>> applyArgument,
                Action<Future<TResult>> completeFuture)
            where TArgument : class
            where TResult : class
        {
            Func<CancellationToken, TResult> safeInstruction = ct => instruction(ct, argument);
            Future<TResult> future = Future<TResult>.Create(safeInstruction);
            completeFuture(future);
            return future;
        }

        protected virtual Func<CancellationToken, TResult> ApplyArgument<TArgument, TResult>(
                Func<CancellationToken, TArgument, TResult> instruction,
                TArgument argument)
            where TArgument : class
            where TResult : class
        {
            return ApplyArgumentGeneric(instruction, argument);
        }

        internal static Func<CancellationToken, TResult> ApplyArgumentGeneric<TArgument, TResult>(
                Func<CancellationToken, TArgument, TResult> instruction,
                TArgument argument)
            where TArgument : class
            where TResult : class
        {
            return ct => instruction(ct, argument);
        }

        /// <summary>
        /// method that causes the future to complete
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="future"></param>
        protected virtual void CompleteFuture<TResult>(Future<TResult> future)
            where TResult : class
        {
            CompleteFutureGeneric(future);
        }

        /// <summary>
        /// Used by both the generic and non-generic class
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="future"></param>
        internal static void CompleteFutureGeneric<TResult>(Future<TResult> future)
            where TResult : class
        {
            future.RunSynchronously();
        }
    }

    /// <summary>
    /// Base implementation that can only deal with the same types of arguments and return statements.
    /// Executes instructions sequentially.
    /// </summary>
    /// <typeparam name="TArgument"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public class Executor<TArgument, TResult> : IExecutor<TArgument, TResult>
        where TArgument : class
        where TResult : class
    {
        /// <summary>
        /// Executes the instruction and returns a completed future.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public Future<TResult> Execute(Func<CancellationToken, TArgument, TResult> instruction, TArgument argument)
        {
            return Executor.ExecuteGeneric(instruction, argument, ApplyArgument, CompleteFuture);
        }

        protected virtual Func<CancellationToken, TResult> ApplyArgument(
                Func<CancellationToken, TArgument, TResult> instruction,
                TArgument argument)
        {
            return Executor.ApplyArgumentGeneric(instruction, argument);
        }

        protected virtual void CompleteFuture(Future<TResult> future)
        {
            Executor.CompleteFutureGeneric(future);
        }
    }
}
