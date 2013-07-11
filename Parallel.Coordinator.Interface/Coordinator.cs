using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parallel.Worker.Interface;

namespace Parallel.Coordinator.Interface
{
    public class Coordinator
    {
        public TResult Do<TArgument, TResult>(IExecutor executor, Func<TArgument, TResult> operation, TArgument argument)
            where TArgument : class
            where TResult : class
        {
            Future<TResult> f = executor.Execute(operation, argument);
            f.Wait();
            return f.Result;
        }
    }
}
