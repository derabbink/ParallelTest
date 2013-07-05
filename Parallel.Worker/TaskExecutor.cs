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
            Task.Factory.StartNew(() =>
                {
                    future.SetExecuting();
                    var result = safeInstruction.Invoke();
                    future.SetCompleted(result);
                });
        }
    }
}
