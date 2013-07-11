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

        protected SafeInstructionResult(ResultState state, Exception exception)
        {
            State = state;
            Exception = exception;
        }

        internal static SafeInstructionResult Succeeded()
        {
            return new SafeInstructionResult(ResultState.Succeeded, null);
        }

        internal static SafeInstructionResult Failed(Exception exception)
        {
            return new SafeInstructionResult(ResultState.Failed, exception);
        }

        public ResultState State { get; private set; }

        public Exception Exception { get; private set; }

        public void Unwrap()
        {
            if (State == ResultState.Failed)
                throw Exception;
        }
    }

    /// <summary>
    /// Represents the result of an instruction
    /// wraps a result or an exeption alike
    /// </summary>
    public class SafeInstructionResult<TResult> : SafeInstructionResult
        where TResult : class
    {
        protected SafeInstructionResult(ResultState state, TResult value, Exception exception) : base(state, exception)
        {
            Value = value;
        }

        internal static SafeInstructionResult<TResult> Succeeded(TResult value)
        {
            return new SafeInstructionResult<TResult>(ResultState.Succeeded, value, null);
        }

        public TResult Value { get; private set; }

        public new TResult Unwrap()
        {
            base.Unwrap();
            return Value;
        }
    }
}
