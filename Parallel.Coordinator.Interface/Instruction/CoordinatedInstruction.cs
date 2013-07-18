using System;
using System.Collections.Generic;
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
        private int _timeout;

        public CoordinatedInstruction(IExecutor executor, Func<CancellationToken, Action, TArgument, TResult> instruction)
            : this(executor, instruction, CoordinatedInstruction.DefaultTimeout)
        {
        }

        public CoordinatedInstruction(IExecutor executor, Func<CancellationToken, Action, TArgument, TResult> instruction, int timeoutMS)
        {
            _executor = executor;
            _executorGeneric = null;
            _instruction = instruction;
            _timeout = timeoutMS;
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
            _timeout = timeoutMS;
        }

        private bool IsGeneric { get { return _executor == null; } }

        /// <summary>
        /// Invokes this instruction under (timeout) supervision
        /// </summary>
        /// <param name="argument"></param>
        /// <returns>A future that IsDone or IsCanceled</returns>
        public Future<TResult> InvokeAndWait(TArgument argument)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            Action doNothing = () => {};
            return InvokeAndWait(cts.Token, doNothing, argument);
        }

        /// <summary>
        /// Invokes this instruction under (timeout) supervision
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="progressListener"></param>
        /// <param name="argument"></param>
        /// <returns>A future that IsDone or IsCanceled</returns>
        public Future<TResult> InvokeAndWait(CancellationToken cancellationToken, Action progressListener, TArgument argument)
        {
            var future = Invoke(argument);
            
            if (cancellationToken.IsCancellationRequested)
                future.Cancel();
            cancellationToken.Register(future.Cancel);
            EventHandler<ProgressEventArgs> progressHandler = (sender, args) => progressListener();
            future.SubscribeProgress(progressHandler);

            if (!TimeOutMonitor.MonitoredWait(future, _timeout))
            {
                future.Cancel();
                //tease out meaningful Exception
                try { future.Wait(); }
                catch (AggregateException e)
                {
                    //avoid encapsulating ACTUAL exception in layers of AggregateException
                    if (e.InnerException is TaskCanceledException)
                        throw new TimeoutException("Instruction timed out, and was canceled because of that.", e.InnerException);
                    else if (e.InnerException is TimeoutException)
                        throw e.InnerException;
                    throw;
                }
                finally { future.UnsubscribeProgress(progressHandler); }
            }
            
            future.UnsubscribeProgress(progressHandler);
            //future IsDone or IsCanceled
            return future;
        }

        private Future<TResult> Invoke(TArgument argument)
        {
            if (IsGeneric)
                return _executorGeneric.Execute(_instruction, argument);
            else
                return _executor.Execute(_instruction, argument);
        }
    }
}
