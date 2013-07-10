using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Parallel.Worker.Communication.SingleChannelCallback;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker
{
    /// <summary>
    /// Executes instructions who's result is returend through raising a (shared) callback event.
    /// callback event listeners pick up the incoming result and complete the associated future.
    /// Executes instructions as new tasks.
    /// </summary>
    /// <typeparam name="TArgument"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public class SingleChannelCallbackTaskExecutor<TArgument, TResult> : SingleChannelCallbackExecutor<TArgument, TResult, CancellationTokenSource>
        where TArgument : class
        where TResult : class
    {
        public SingleChannelCallbackTaskExecutor(IClient<TResult> client, IServer<TArgument, TResult> server) : base(client, server)
        {
        }

        protected override Interface.Future<TResult> CreateFuture(CancellationTokenSource companion)
        {
            return TaskExecutor.CreateFutureGeneric<TResult>(companion);
        }

        protected override void CompleteFuture(Interface.Future<TResult> future, CancellationTokenSource companion, SafeInstruction<TArgument, TResult> safeInstruction)
        {
            Task.Factory.StartNew(() => base.CompleteFuture(future, companion, safeInstruction));
        }
    }
}
