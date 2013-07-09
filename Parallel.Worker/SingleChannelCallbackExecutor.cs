using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parallel.Worker.Communication.SingleChannelCallback;
using Parallel.Worker.Events.SingleChannelCallback;
using Parallel.Worker.Interface;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker
{
    public class SingleChannelCallbackExecutor<TArgument, TResult> : Executor<TArgument, TResult>
        where TArgument : class
        where TResult : class
    {
        protected IClient<TResult> _client;
        protected IServer<TArgument, TResult> _server;

        public SingleChannelCallbackExecutor(IClient<TResult> client, IServer<TArgument, TResult> server)
        {
            _client = client;
            _server = server;
        }

        protected override void CompleteFuture(Future<TResult> future,
                                              SafeInstruction<TArgument, TResult> safeInstruction)
        {
            Guid operationId = SetupCallbackListeners(future);
            future.SetExecuting();
            _server.Run(operationId, safeInstruction, _client);
        }

        protected virtual Guid SetupCallbackListeners(Future<TResult> future)
        {
            Guid operationId = new Guid();
            EventHandler<CallbackEventArgs<TResult>> callbackListener = null;
            callbackListener = (sender, args) =>
            {
                if (args.OperationId == operationId)
                {
                    _client.UnsubscribeCallbackEvent(callbackListener);
                    future.SetCompleted(args.Result);
                }
            };
            _client.SubscribeCallbackEvent(callbackListener);
            return operationId;
        }
    }
}
