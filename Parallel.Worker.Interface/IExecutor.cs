using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker.Interface
{
    public interface IExecutor
    {
        Future<TResult> Execute<TArgument, TResult>(Func<CancellationToken, Action, TArgument, TResult> instruction, TArgument argument)
            where TArgument : class
            where TResult : class;
    }

    public interface IExecutor<TArgument, TResult>
        where TArgument : class
        where TResult : class
    {
        Future<TResult> Execute(Func<CancellationToken, Action, TArgument, TResult> instruction, TArgument argument);
    }
}
