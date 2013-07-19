using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Parallel.Worker.Interface;
using Parallel.Worker.Interface.Events;

namespace Parallel.Coordinator.Interface.Instruction
{
    public static class CoordinatedInstruction
    {
        /// <summary>
        /// A number of miliseconds
        /// </summary>
        internal static int DefaultTimeout = 1000;
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
        
        public CoordinatedInstruction(IExecutor executor, Func<CancellationToken, Action, TArgument, TResult> instruction)
            : this(executor, instruction, CoordinatedInstruction.DefaultTimeout)
        {
        }

        public CoordinatedInstruction(IExecutor executor, Func<CancellationToken, Action, TArgument, TResult> instruction, int timeoutMS)
        {
            _executor = executor;
            _executorGeneric = null;
            _instruction = instruction;
            Timeout = timeoutMS;
        }

        public CoordinatedInstruction(IExecutor<TArgument, TResult> executor, Func<CancellationToken, Action, TArgument, TResult> instruction)
            : this(executor, instruction, CoordinatedInstruction.DefaultTimeout)
        {
        }

        public CoordinatedInstruction(IExecutor<TArgument, TResult> executor, Func<CancellationToken, Action, TArgument, TResult> instruction, int timeoutMS)
        {
            _executor = null;
            _executorGeneric = executor;
            _instruction = instruction;
            Timeout = timeoutMS;
        }

        public int Timeout { get; private set; }

        private bool IsGeneric { get { return _executor == null; } }

        /// <summary>
        /// Invokes this instruction under (timeout) supervision
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="argument"></param>
        /// <returns>A future that IsDone or IsCanceled</returns>
        public Future<TResult> InvokeAndWait(CancellationToken cancellationToken, TArgument argument)
        {
            Action doNothing = () => {};
            return InvokeAndWait(cancellationToken, doNothing, argument);
        }

        /// <summary>
        /// Invokes this instruction under (timeout) supervision
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="progressListener"></param>
        /// <param name="argument"></param>
        /// <returns>A future that IsDone or IsCanceled</returns>
        /// <exception cref="AggregateException">Originating from the resulting Future's Wait()</exception>
        public Future<TResult> InvokeAndWait(CancellationToken cancellationToken, Action progressListener, TArgument argument)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var future = Invoke(cancellationToken, argument);
            cancellationToken.Register(future.Cancel);

            EventHandler<ProgressEventArgs> progressHandler = (sender, args) => progressListener();
            future.SubscribeProgress(progressHandler);

            if (!TimeOutMonitor.MonitoredWait(future, Timeout))
                future.Cancel();

            try { future.Wait(); }
            finally { future.UnsubscribeProgress(progressHandler); }
            
            //future IsDone or IsCanceled
            return future;
        }

        /// <summary>
        /// </summary>
        /// <param name="cancellationToken">since executor.execute might block, we pass in a cancellation token</param>
        /// <param name="argument"></param>
        /// <returns></returns>
        private Future<TResult> Invoke(CancellationToken cancellationToken, TArgument argument)
        {
            if (IsGeneric)
                return _executorGeneric.Execute(cancellationToken, _instruction, argument);
            else
                return _executor.Execute(cancellationToken, _instruction, argument);
        }
    }
}
