﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Parallel.Worker.Interface;

namespace Parallel.Coordinator.Interface.Instruction
{
    public static class CoordinatedInstruction
    {
        internal static TimeSpan DefaultTimeout = TimeSpan.FromSeconds(1);
    }

    /// <summary>
    /// Represents an instruction to be executed on an IExecutor.
    /// Contains/captures potential cancellation/timeout errors
    /// </summary>
    public class CoordinatedInstruction<TArgument, TResult>
        where TArgument : class
        where TResult : class
    {
        private IExecutor _executor;
        private IExecutor<TArgument, TResult> _executorGeneric;
        private Func<CancellationToken, Action, TArgument, TResult> _instruction;
        private TimeSpan _timeout;

        public CoordinatedInstruction(IExecutor executor, Func<CancellationToken, Action, TArgument, TResult> instruction)
            : this(executor, instruction, CoordinatedInstruction.DefaultTimeout)
        {
        }

        public CoordinatedInstruction(IExecutor executor, Func<CancellationToken, Action, TArgument, TResult> instruction, TimeSpan timeout)
        {
            _executor = executor;
            _executorGeneric = null;
            _instruction = instruction;
            _timeout = timeout;
        }

        public CoordinatedInstruction(IExecutor<TArgument, TResult> executor, Func<CancellationToken, Action, TArgument, TResult> instruction)
            : this(executor, instruction, CoordinatedInstruction.DefaultTimeout)
        {
        }

        public CoordinatedInstruction(IExecutor<TArgument, TResult> executor, Func<CancellationToken, Action, TArgument, TResult> instruction, TimeSpan timeout)
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
