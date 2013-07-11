using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        /// Executes the instruction and returns a future/task representing the result
        /// </summary>
        /// <typeparam name="TArgument"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="instruction"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public Task<SafeInstructionResult<TResult>> Execute<TArgument, TResult>(Func<TArgument, TResult> instruction, TArgument argument)
            where TArgument : class
            where TResult : class
        {
            return ExecuteGeneric(instruction, argument, CreateSafeInstruction, CompleteFuture);
        }

        internal static Task<SafeInstructionResult<TResult>> ExecuteGeneric<TArgument, TResult>(
                Func<TArgument, TResult> instruction,
                TArgument argument,
                Func<Func<TArgument, TResult>, TArgument, SafeInstruction<TArgument, TResult>> createSafeInstruction,
                Action<Task<SafeInstructionResult<TResult>>> completeFuture)
            where TArgument : class
            where TResult : class
        {
            SafeInstruction<TArgument, TResult> safeInstruction = createSafeInstruction(instruction, argument);
            Task<SafeInstructionResult<TResult>> future = new Task<SafeInstructionResult<TResult>>(safeInstruction.Invoke);
            completeFuture(future);
            return future;
        }

        protected virtual SafeInstruction<TArgument, TResult> CreateSafeInstruction<TArgument, TResult>(Func<TArgument, TResult> instruction, TArgument argument)
            where TArgument : class
            where TResult : class
        {
            return CreateSafeInstructionGeneric(instruction, argument);
        }

        internal static SafeInstruction<TArgument, TResult> CreateSafeInstructionGeneric<TArgument, TResult>(Func<TArgument, TResult> instruction, TArgument argument)
            where TArgument : class
            where TResult : class
        {
            return new SafeInstruction<TArgument, TResult>(instruction, argument);
        }

        /// <summary>
        /// method that causes the future to complete
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="future"></param>
        protected virtual void CompleteFuture<TResult>(Task<SafeInstructionResult<TResult>> future)
            where TResult : class
        {
            CompleteFutureGeneric(future);
        }

        /// <summary>
        /// Used by both the generic and non-generic class
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="future"></param>
        internal static void CompleteFutureGeneric<TResult>(Task<SafeInstructionResult<TResult>> future)
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
        public Task<SafeInstructionResult<TResult>> Execute(Func<TArgument, TResult> instruction, TArgument argument)
        {
            return Executor.ExecuteGeneric(instruction, argument, CreateSafeInstruction, CompleteFuture);
        }

        protected virtual SafeInstruction<TArgument, TResult> CreateSafeInstruction(Func<TArgument, TResult> instruction, TArgument argument)
        {
            return Executor.CreateSafeInstructionGeneric(instruction, argument);
        }

        protected virtual void CompleteFuture(Task<SafeInstructionResult<TResult>> future)
        {
            Executor.CompleteFutureGeneric(future);
        }
    }
}
