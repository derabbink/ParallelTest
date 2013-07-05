using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parallel.Worker.Interface.Instruction
{
    /// <summary>
    /// Represents the result of an instruction
    /// wraps a result or an exeption alike
    /// </summary>
    public class SafeInstructionResult<TResult> where TResult : class
    {
        public enum ResultState
        {
            Succeeded,
            Failed
        }

        private SafeInstructionResult(ResultState state, TResult value, Exception exception)
        {
            State = state;
            Value = value;
            Exception = exception;
        }

        internal static SafeInstructionResult<TResult> Succeeded(TResult value)
        {
            return new SafeInstructionResult<TResult>(ResultState.Succeeded, value, null);
        }

        internal static SafeInstructionResult<TResult> Failed(Exception error)
        {
            return new SafeInstructionResult<TResult>(ResultState.Failed, null, error);
        }

        public ResultState State { get; private set; }

        public TResult Value { get; private set; }

        public Exception Exception { get; private set; }
    }
}
