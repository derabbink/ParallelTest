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
    /// <summary>
    /// Executes instructions who's result is returend through raising a (shared) callback event.
    /// callback event listeners pick up the incoming result and complete the associated future.
    /// Executes instructions sequentially.
    /// </summary>
    /// <typeparam name="TArgument"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public class SingleChannelCallbackExecutor<TArgument, TResult> : SingleChannelCallbackExecutor<TArgument, TResult, object>
        where TArgument : class
        where TResult : class
    {
        public SingleChannelCallbackExecutor(IClient<TResult> client, IServer<TArgument, TResult> server) : base(client, server)
        {
            _client = client;
            _server = server;
        }
    }

    /// <summary>
    /// Executes instructions who's result is returend through raising a (shared) callback event.
    /// callback event listeners pick up the incoming result and complete the associated future.
    /// Executes instructions sequentially.
    /// </summary>
    /// <typeparam name="TArgument"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TFutureCompanion"></typeparam>
    [Obsolete("Only used by Executor implementers", false)]
    public class SingleChannelCallbackExecutor<TArgument, TResult, TFutureCompanion> : Executor<TArgument, TResult, TFutureCompanion>
        where TArgument : class
        where TResult : class
        where TFutureCompanion : class
    {
        protected IClient<TResult> _client;
        protected IServer<TArgument, TResult> _server;

        public SingleChannelCallbackExecutor(IClient<TResult> client, IServer<TArgument, TResult> server)
        {
            _client = client;
            _server = server;
        }

        protected override void CompleteFuture(Future<TResult> future,
                                               TFutureCompanion companion,
                                               SafeInstruction<TArgument, TResult> safeInstruction)
        {
            Guid operationId = SetupCallbackListeners(future, companion);
            future.SetExecuting();
            _server.Run(operationId, safeInstruction, _client);
        }

        protected virtual Guid SetupCallbackListeners(Future<TResult> future, TFutureCompanion companion)
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
