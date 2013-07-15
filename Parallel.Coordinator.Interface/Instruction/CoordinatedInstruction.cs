using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Parallel.Worker.Interface;

namespace Parallel.Coordinator.Interface.Instruction
{
    /// <summary>
    /// Represents an instruction to be executed on an IExecutor.
    /// Contains potential cancellation/timeout errors
    /// </summary>
    public class CoordinatedInstruction<TArgument, TResult>
        where TArgument : class
        where TResult : class
    {
        private IExecutor _executor;
        private IExecutor<TArgument, TResult> _executorGeneric;
        private Func<CancellationToken, TArgument, TResult> _instruction;
        private Task<TResult> _future;

        public CoordinatedInstruction(IExecutor executor, Func<CancellationToken, TArgument, TResult> instruction)
        {
            _executor = executor;
            _executorGeneric = null;
            _instruction = instruction;
        }

        public CoordinatedInstruction(IExecutor<TArgument, TResult> executor, Func<CancellationToken, TArgument, TResult> instruction)
        {
            _executor = null;
            _executorGeneric = executor;
            _instruction = instruction;
        }

        private bool IsGeneric { get { return _executor == null; } }

        public Future<TResult> Invoke(TArgument argument)
        {
            if (IsGeneric)
                return _executorGeneric.Execute(_instruction, argument);
            else
                return _executor.Execute(_instruction, argument);
        }
    }
}
