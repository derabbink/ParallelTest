﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Parallel.Worker.Communication.SingleChannelCallback;
using Parallel.Worker.Interface;
using Parallel.Worker.Interface.Communication.SingleChannelCallback;
using Parallel.Worker.Interface.Events.SingleChannelCallback;
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

        protected override Func<CancellationToken, Action, TResult> ApplyArgument(
                Func<CancellationToken, Action, TArgument, TResult> instruction,
                TArgument argument)
        {
            Func<CancellationToken, Action, TResult> wrapped = (ct, p) =>
                {
                    bool canceled = false;
                    Future<TResult> wrappedResult = null;
                    ManualResetEvent resultBlock = new ManualResetEvent(false);

                    Action<Future<TResult>> resultCallback = result =>
                        {
                            //result should be a future that IsDone
                            wrappedResult = result;
                            resultBlock.Set();
                        };
                    Action progressCallback = p;
                    Action cancellationCallback = () =>
                        {
                            canceled = true;
                            resultBlock.Set();
                        };
                    Guid operationId = SetupCallbackListeners(_client, resultCallback, progressCallback, cancellationCallback);
                    ct.Register(() => _server.Cancel(operationId));
                    _server.Run(operationId, instruction, argument, _client);

                    resultBlock.WaitOne();
                    if (canceled)
                        return null;
                    else
                        //unwrap (possible exception) in caller thread, so event-raiser's exception catcher does not interfere
                        return wrappedResult.Unwrap();
                };
            return wrapped;
        }

        private static Guid SetupCallbackListeners(IClient<TResult> callbackChannel, Action<Future<TResult>> resultCallback, Action progressCallback, Action cancelCallback)
        {
            Guid operationId = new Guid();
            EventHandler<CallbackEventArgs<TResult>> callbackListener = null;
            EventHandler<CallbackProgressEventArgs> progressListener = null;
            EventHandler<CancelEventArgs> cancellationListener = null;
            callbackListener = (sender, args) =>
                {
                    if (args.OperationId == operationId)
                    {
                        callbackChannel.UnsubscribeCallbackEvent(callbackListener);
                        callbackChannel.UnsubscribeProgressEvent(progressListener);
                        callbackChannel.UnsubscribeCancellationEvent(cancellationListener);
                        resultCallback(args.Result);
                    }
                };
            progressListener = (sender, args) =>
            {
                if (args.OperationId == operationId)
                {
                    progressCallback();
                }
            };
            cancellationListener = (sender, args) =>
                {
                    if (args.OperationId == operationId)
                    {
                        callbackChannel.UnsubscribeCallbackEvent(callbackListener);
                        callbackChannel.UnsubscribeProgressEvent(progressListener);
                        callbackChannel.UnsubscribeCancellationEvent(cancellationListener);
                        cancelCallback();
                    }
                };
            callbackChannel.SubscribeCallbackEvent(callbackListener);
            callbackChannel.SubscribeProgressEvent(progressListener);
            callbackChannel.SubscribeCancellationEvent(cancellationListener);
            return operationId;
        }
    }
}
