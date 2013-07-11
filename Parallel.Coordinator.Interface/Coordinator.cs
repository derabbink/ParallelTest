using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parallel.Worker.Interface;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Coordinator.Interface
{
    public class Coordinator
    {
        public SafeInstructionResult<TResult> Do<TArgument, TResult>(IExecutor executor, Func<TArgument, TResult> operation, TArgument argument)
            where TArgument : class
            where TResult : class
        {
            Task<SafeInstructionResult<TResult>> f = executor.Execute(operation, argument);
            f.Wait();
            return f.Result;
        }
    }
}
