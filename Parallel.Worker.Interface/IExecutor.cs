using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker.Interface
{
    public interface IExecutor
    {
        Task<SafeInstructionResult<TResult>> Execute<TArgument, TResult>(Func<TArgument, TResult> instruction, TArgument argument)
            where TArgument : class
            where TResult : class;
    }

    public interface IExecutor<TArgument, TResult>
        where TArgument : class
        where TResult : class
    {
        Task<SafeInstructionResult<TResult>> Execute(Func<TArgument, TResult> instruction, TArgument argument);
    }
}
