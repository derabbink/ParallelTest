﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker.Interface
{
    /// <summary>
    /// Future that does not contain a computation result
    /// </summary>
    public class Future
    {
        public enum FutureState
        {
            PreExecution,
            Executing,
            Completed
        }

        private readonly ManualResetEvent _completedWaitHandle;

        public Future()
        {
            _completedWaitHandle = new ManualResetEvent(false);
            State = FutureState.PreExecution;
        }

        public FutureState State
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get;
            [MethodImpl(MethodImplOptions.Synchronized)]
            private set;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SetExecuting()
        {
            Contract.Requires<InvalidOperationException>(State == FutureState.PreExecution, "State was not 'PreExecution'");
            State = FutureState.Executing;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SetCompleted()
        {
            Contract.Requires<InvalidOperationException>(State == FutureState.Executing, "State was not 'Executing'");
            State = FutureState.Completed;
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

    /// <summary>
    /// Future that contains a computation result
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public class Future<TResult> : Future where TResult : class
    {
        private SafeInstructionResult<TResult> _result;

        public SafeInstructionResult<TResult> Result
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                Contract.Requires<InvalidOperationException>(State == FutureState.Completed, "State was not 'Completed'");
                return _result;
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            private set
            {
                _result = value;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SetCompleted(SafeInstructionResult<TResult> result)
        {
            Result = result;
            base.SetCompleted();
        }
    }
}