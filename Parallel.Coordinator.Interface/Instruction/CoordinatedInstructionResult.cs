using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parallel.Coordinator.Interface.Exceptions;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Coordinator.Interface.Instruction
{
    public class CoordinatedInstructionResult
    {
        public enum ResultState
        {
            Completed,
            Cancelled,
        }

        protected CoordinatedInstructionResult(ResultState state, SafeInstructionResult result,
                                               CancellationException exception)
        {
            State = state;
            Result = result;
            Exception = exception;
        }

        public static CoordinatedInstructionResult Completed(SafeInstructionResult result)
        {
            return new CoordinatedInstructionResult(ResultState.Completed, result, null);
        }

        public static CoordinatedInstructionResult Cancelled(CancellationException exception)
        {
            return new CoordinatedInstructionResult(ResultState.Cancelled, null, exception);
        }

        public ResultState State { get; private set; }

        public SafeInstructionResult Result { get; protected set; }

        public CancellationException Exception { get; private set; }
    }

    public class CoordinatedInstructionResult<TResult> : CoordinatedInstructionResult
        where TResult : class
    {
        protected CoordinatedInstructionResult(ResultState state, SafeInstructionResult result,
                                               CancellationException exception) : base(state, result, exception) {}

        public static CoordinatedInstructionResult<TResult> Completed(SafeInstructionResult<TResult> result)
        {
            return new CoordinatedInstructionResult<TResult>(ResultState.Completed, result, null);
        }

        public new static CoordinatedInstructionResult<TResult> Cancelled(CancellationException exception)
        {
            return new CoordinatedInstructionResult<TResult>(ResultState.Cancelled, null, exception);
        }

        public new SafeInstructionResult<TResult> Result {
            get { return base.Result as SafeInstructionResult<TResult>; }
            protected set { base.Result = value; }
        }
    }
}
