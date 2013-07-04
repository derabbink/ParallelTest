using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Parallel.Worker.Interface.Parallel
{
    internal class TaskExecutor : Executor
    {
        internal override void Execute(Operation operation, Guid id)
        {
            Task task = Task.Factory.StartNew(() => ExecuteWithEvents(operation, id));
        }
    }
}
