using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parallel.Worker.Communication.SingleChannelCallback;

namespace Parallel.Worker
{
    public class SingleChannelCallbackTaskExecutor<TArgument, TResult> : SingleChannelCallbackExecutor<TArgument, TResult>
        where TArgument : class
        where TResult : class
    {
        public SingleChannelCallbackTaskExecutor(IClient<TResult> client, IServer<TArgument, TResult> server) : base(client, server)
        {
        }

        protected override Interface.Future<TResult> CreateFuture()
        {
            return TaskExecutor.CreateFutureGeneric<TResult>();
        }

        protected override void CompleteFuture(Interface.Future<TResult> future, Interface.Instruction.SafeInstruction<TArgument, TResult> safeInstruction)
        {
            Task.Factory.StartNew(() => base.CompleteFuture(future, safeInstruction));
        }
    }
}
