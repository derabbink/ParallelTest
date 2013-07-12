using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parallel.Coordinator.Interface.Exceptions;
using Parallel.Worker.Interface;
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

        protected CoordinatedInstructionResult(ResultState state, Future result,
                                               CancellationException exception)
        {
            State = state;
            Result = result;
            Exception = exception;
        }

        public static CoordinatedInstructionResult Completed(Future result)
        {
            return new CoordinatedInstructionResult(ResultState.Completed, result, null);
        }

        public static CoordinatedInstructionResult Cancelled(CancellationException exception)
        {
            return new CoordinatedInstructionResult(ResultState.Cancelled, null, exception);
        }

        public ResultState State { get; private set; }

        public Future Result { get; protected set; }

        public CancellationException Exception { get; private set; }
    }

    public class CoordinatedInstructionResult<TResult>
        where TResult : class
    {
        protected CoordinatedInstructionResult(CoordinatedInstructionResult.ResultState state, Future<TResult> result,
                                               CancellationException exception)
        {
            State = state;
            Result = result;
            Exception = exception;
        }

        public static CoordinatedInstructionResult<TResult> Completed(Future<TResult> result)
        {
            return new CoordinatedInstructionResult<TResult>(CoordinatedInstructionResult.ResultState.Completed, result, null);
        }

        public new static CoordinatedInstructionResult<TResult> Cancelled(CancellationException exception)
        {
            return new CoordinatedInstructionResult<TResult>(CoordinatedInstructionResult.ResultState.Cancelled, null, exception);
        }

        public CoordinatedInstructionResult.ResultState State { get; private set; }

        public new Future<TResult> Result { get; protected set; }

        public CancellationException Exception { get; private set; }
    }
}
