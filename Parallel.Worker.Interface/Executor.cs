using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker.Interface
{
    public class Executor
    {
        /// <summary>
        /// Takes an instruction and returns a Future representing its result.
        /// This default implementation is sequential, i.e. the returned future is already completed.
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
            Future<TResult> future = new Future<TResult>();
            SafeInstruction<TArgument, TResult> safeInstruction = new SafeInstruction<TArgument, TResult>(instruction,
                                                                                                          argument);
            CompleteFuture(future, safeInstruction);
            return future;
        }

        /// <summary>
        /// Invokes and instruction and updates the corresponding future accordingly, until completion.
        /// </summary>
        /// <typeparam name="TArgument"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="future"></param>
        /// <param name="safeInstruction"></param>
        protected virtual void CompleteFuture<TArgument, TResult>(Future<TResult> future,
                                                                  SafeInstruction<TArgument, TResult> safeInstruction)
            where TArgument : class
            where TResult : class
        {
            future.SetExecuting();
            SafeInstructionResult<TResult> result = safeInstruction.Invoke();
            future.SetCompleted(result);
        }
    }
}
