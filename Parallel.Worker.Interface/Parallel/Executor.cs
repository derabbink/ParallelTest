using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parallel.Worker.Interface.Parallel
{
    internal interface IExecutor
    {
        void Execute(Operation operation);
    }
}
