using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Parallel.Worker;
using Parallel.Worker.Interface;

namespace Parallel.Coordinator.Interface.Instruction
{
    /// <summary>
    /// Represents an instruction to be executed by the Coordinator,
    /// not by a remote/worker's IExecutor
    /// </summary>
    public class LocalInstruction<TArgument, TResult> : CoordinatedInstruction<TArgument, TResult>
        where TArgument : class
        where TResult : class
    {
        public LocalInstruction(Func<CancellationToken, IProgress, TArgument, TResult> instruction)
            : base(new TaskExecutor<TArgument, TResult>(), instruction) //generics are optional
        {
        }
    }
}
