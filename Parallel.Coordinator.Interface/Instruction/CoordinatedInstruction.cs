using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private Func<TArgument, TResult> _instruction;
        private TArgument _argument;
        private Task<TResult> _future;

        public CoordinatedInstruction(IExecutor executor, Func<TArgument, TResult> instruction, TArgument argument)
        {
            _executor = executor;
            _executorGeneric = null;
            _instruction = instruction;
            _argument = argument;
        }

        public CoordinatedInstruction(IExecutor<TArgument, TResult> executor, Func<TArgument, TResult> instruction,
                                      TArgument argument)
        {
            _executor = null;
            _executorGeneric = executor;
            _instruction = instruction;
            _argument = argument;
        }

        private bool Generic { get { return _executor == null; } }

        public Task<CoordinatedInstructionResult<TResult>> Invoke()
        {
            //TODO
            return null;
        }
    }
}
