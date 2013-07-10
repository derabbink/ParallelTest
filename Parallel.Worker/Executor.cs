using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parallel.Worker.Interface;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker
{
    /// <summary>
    /// Default executor implementation. Executes instructions sequentially.
    /// Can deal with different argument and return types, each Execute
    /// Also contains static default implementation for virtual methods
    /// </summary>
    public class Executor : Executor<object>
    {
        internal static Future<TResult> ExecuteGeneric<TArgument, TResult, TFutureCompanion>(Func<TArgument, TResult> instruction,
                                                                                             TArgument argument,
                                                                                             Func<TFutureCompanion> createFutureCompanion,
                                                                                             Func<TFutureCompanion, Future<TResult>> createFuture, 
                                                                                             Action<Future<TResult>, TFutureCompanion, SafeInstruction<TArgument, TResult>> completeFuture)
            where TArgument : class
            where TResult : class
        {
            TFutureCompanion companion = createFutureCompanion();
            Future<TResult> future = createFuture(companion);
            SafeInstruction<TArgument, TResult> safeInstruction = new SafeInstruction<TArgument, TResult>(instruction, argument);
            completeFuture(future, companion, safeInstruction);
            return future;
        }

        internal static TFutureCompanion CreateFutureCompanionGeneric<TFutureCompanion>()
            where TFutureCompanion : class
        {
            return null;
        }

        internal static Future<TResult> CreateFutureGeneric<TResult>()
            where TResult : class
        {
            return new Future<TResult>();
        }

        internal static void CompleteFutureGeneric<TArgument, TResult>(Future<TResult> future,
                                                                       SafeInstruction<TArgument, TResult> safeInstruction)
            where TArgument : class
            where TResult : class
        {
            future.SetExecuting();
            SafeInstructionResult<TResult> result = safeInstruction.Invoke();
            future.SetCompleted(result);
        }
    }

    /// <summary>
    /// Base implementation. Executes instructions sequentially.
    /// Can deal with different argument and return types, each Execute.
    /// </summary>
    /// <typeparam name="TFutureCompanion"></typeparam>
    [Obsolete("Only used by Executor implementers", false)]
    public class Executor<TFutureCompanion> : IExecutor
        where TFutureCompanion : class
    {
        /// <summary>
        /// Executes the instruction and returns a completed future.
        /// </summary>
        /// <typeparam name="TArgument"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="instruction"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public Future<TResult> Execute<TArgument, TResult>(Func<TArgument, TResult> instruction, TArgument argument)
            where TArgument : class
            where TResult : class
        {
            return Executor.ExecuteGeneric(instruction, argument, CreateFutureCompanion, CreateFuture<TResult>, CompleteFuture);
        }

        /// <summary>
        /// object created here will be passed into CreateFuture and CompleteFuture
        /// </summary>
        /// <returns></returns>
        protected virtual TFutureCompanion CreateFutureCompanion()
        {
            return Executor.CreateFutureCompanionGeneric<TFutureCompanion>();
        }

        protected virtual Future<TResult> CreateFuture<TResult>(TFutureCompanion companion)
            where TResult : class
        {
            return Executor.CreateFutureGeneric<TResult>();
        }

        protected virtual void CompleteFuture<TArgument, TResult>(Future<TResult> future,
                                                                  TFutureCompanion companion,
                                                                  SafeInstruction<TArgument, TResult> safeInstruction)
            where TArgument : class
            where TResult : class
        {
            Executor.CompleteFutureGeneric(future, safeInstruction);
        }
    }

    /// <summary>
    /// Base implementation that can only deal with the same types of arguments and return statements.
    /// Executes instructions sequentially.
    /// </summary>
    /// <typeparam name="TArgument"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public class Executor<TArgument, TResult> : Executor<TArgument, TResult, object>, IExecutor<TArgument, TResult>
        where TArgument : class
        where TResult : class
    {
        protected override object CreateFutureCompanion()
        {
            return Executor.CreateFutureCompanionGeneric<object>();
        }

        protected override Future<TResult> CreateFuture(object companion)
        {
            return Executor.CreateFutureGeneric<TResult>();
        }
        
        protected override void CompleteFuture(Future<TResult> future,
                                               object companion,
                                               SafeInstruction<TArgument, TResult> safeInstruction)
        {
            Executor.CompleteFutureGeneric(future, safeInstruction);
        }
    }

    /// <summary>
    /// Base implementation that can only deal with the same types of arguments and return statements.
    /// Executes instructions sequentially.
    /// </summary>
    /// <typeparam name="TArgument"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TFutureCompanion"></typeparam>
    [Obsolete("Only used by Executor implementers", false)]
    public class Executor<TArgument, TResult, TFutureCompanion> : IExecutor<TArgument, TResult>
        where TArgument : class
        where TResult : class
        where TFutureCompanion : class
    {
        /// <summary>
        /// Executes the instruction and returns a completed future.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public Future<TResult> Execute(Func<TArgument, TResult> instruction, TArgument argument)
        {
            return Executor.ExecuteGeneric(instruction, argument, CreateFutureCompanion, CreateFuture, CompleteFuture);
        }

        /// <summary>
        /// object created here will be passed into CreateFuture and CompleteFuture
        /// </summary>
        /// <returns></returns>
        protected virtual TFutureCompanion CreateFutureCompanion()
        {
            return Executor.CreateFutureCompanionGeneric<TFutureCompanion>();
        }

        protected virtual Future<TResult> CreateFuture(TFutureCompanion companion)
        {
            return Executor.CreateFutureGeneric<TResult>();
        }

        protected virtual void CompleteFuture(Future<TResult> future,
                                              TFutureCompanion companion,
                                              SafeInstruction<TArgument, TResult> safeInstruction)
        {
            Executor.CompleteFutureGeneric(future, safeInstruction);
        }
    }
}
