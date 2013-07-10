using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parallel.Worker.Interface
{
    public interface IExecutor
    {
        Future<TResult> Execute<TArgument, TResult>(Func<TArgument, TResult> instruction, TArgument argument)
            where TArgument : class
            where TResult : class;
    }

    public interface IExecutor<TArgument, TResult>
        where TArgument : class
        where TResult : class
    {
        Future<TResult> Execute(Func<TArgument, TResult> instruction, TArgument argument);
    }
}
