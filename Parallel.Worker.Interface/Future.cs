using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker.Interface
{
    public class Future<TResult> where TResult : class
    {
        private readonly AutoResetEvent _completedWaitHandle;

        public enum FutureState
        {
            PreExecution,
            Executing,
            Completed
        }

        public Future()
        {
            _completedWaitHandle = new AutoResetEvent(false);
            State = FutureState.PreExecution;
        }

        public FutureState State
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get;
            [MethodImpl(MethodImplOptions.Synchronized)]
            private set;
        }

        private SafeInstructionResult<TResult> _result;

        public SafeInstructionResult<TResult> Result
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                Contract.Requires<InvalidOperationException>(State == FutureState.Completed, string.Format("State was not {0}, but {1}.", FutureState.Completed, State));
                return _result;
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            private set
            {
                Contract.Requires<InvalidOperationException>(State == FutureState.Completed, string.Format("State was not {0}, but {1}.", FutureState.Completed, State));
                _result = value;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SetExecuting()
        {
            Contract.Requires<InvalidOperationException>(State == FutureState.Completed, string.Format("State was not {0}, but {1}.", FutureState.PreExecution, State));
            State = FutureState.Executing;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SetCompleted(SafeInstructionResult<TResult> result)
        {
            Contract.Requires<InvalidOperationException>(State == FutureState.Executing, string.Format("State was not {0}, but {1}.", FutureState.Executing, State));
            State = FutureState.Completed;
            Result = result;
            _completedWaitHandle.Set();
        }

        /// <summary>
        /// Blocks until <see cref="State"/> is Completed
        /// </summary>
        public void Wait()
        {
            _completedWaitHandle.WaitOne();
        }

        /// <summary>
        /// Cancels any running execution. Never fails.
        /// </summary>
        public void Cancel()
        {
            if (State == FutureState.Executing)
                throw new NotImplementedException();
        }
    }
}
