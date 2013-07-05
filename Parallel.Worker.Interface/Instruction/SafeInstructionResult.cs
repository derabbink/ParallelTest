using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parallel.Worker.Interface.Instruction
{
    public class SafeInstructionResult
    {
        public enum ResultState
        {
            Succeeded,
            Failed
        }

        protected SafeInstructionResult(ResultState state)
        {
            State = state;
        }

        internal static SafeInstructionResult Succeeded()
        {
            return new SafeInstructionResult(ResultState.Succeeded);
        }

        internal static SafeInstructionResult Failed()
        {
            return new SafeInstructionResult(ResultState.Failed);
        }

        public ResultState State { get; private set; }
    }

    /// <summary>
    /// Represents the result of an instruction
    /// wraps a result or an exeption alike
    /// </summary>
    public class SafeInstructionResult<TResult> : SafeInstructionResult where TResult : class
    {
        

        protected SafeInstructionResult(ResultState state, TResult value, Exception exception) : base(state)
        {
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

        public TResult Value { get; private set; }

        public Exception Exception { get; private set; }
    }
}
