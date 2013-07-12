using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Parallel.Worker.Interface;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Coordinator.Interface
{
    public class Coordinator
    {
        public Future<TResult> Do<TArgument, TResult>(IExecutor executor, Func<CancellationToken, TArgument, TResult> operation, TArgument argument)
            where TArgument : class
            where TResult : class
        {
            Future<TResult> f = executor.Execute(operation, argument);
            f.Wait();
            return f;
        }
    }
}
