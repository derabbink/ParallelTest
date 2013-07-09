using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Parallel.Worker.Interface.Instruction
{
    /// <summary>
    /// Represents an instruction/a delegate and wraps it's execution
    /// to contain any potential exceptions
    /// </summary>
    public class SafeInstruction<TArgument, TResult>
        where TArgument : class
        where TResult : class
    {
        private readonly Func<TArgument, TResult> _instruction;
        private readonly TArgument _argument;

        public SafeInstruction(Func<TArgument, TResult> instruction, TArgument argument)
        {
            Contract.Requires<ArgumentNullException>(instruction != null, "operation must not be null");

            _instruction = instruction;
            _argument = argument;
        }

        public SafeInstructionResult<TResult> Invoke()
        {
            try
            {
                TResult result = _instruction(_argument);
                return SafeInstructionResult<TResult>.Succeeded(result);
            }
            catch (Exception exception)
            {
                return SafeInstructionResult<TResult>.Failed(exception);
            }
        }
    }
}
