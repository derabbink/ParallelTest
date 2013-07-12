using System;
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

        protected override Func<CancellationToken, TResult> ApplyArgument(
                Func<CancellationToken, TArgument, TResult> instruction,
                TArgument argument)
        {
            Func<CancellationToken, TResult> wrapped = ct =>
                {
                    bool canceled = false;
                    Future<TResult> wrappedResult = null;
                    ManualResetEventSlim resultBlock = new ManualResetEventSlim(false);

                    Action<Future<TResult>> resultCallback = result =>
                        {
                            //result should be a future that IsDone
                            wrappedResult = result;
                            resultBlock.Set();
                        };
                    Action cancellationCallback = () =>
                        {
                            canceled = true;
                            resultBlock.Set();
                        };
                    Guid operationId = SetupCallbackListeners(_client, resultCallback, cancellationCallback);
                    ct.Register(() => _server.Cancel(operationId));
                    _server.Run(operationId, instruction, argument, _client);

                    resultBlock.Wait();
                    if (canceled)
                        return null;
                    else
                        //unwrap (possible exception) in caller thread, so event-raiser's exception catcher does not interfere
                        return wrappedResult.Unwrap();
                };
            return wrapped;
        }

        private static Guid SetupCallbackListeners(IClient<TResult> callbackChannel, Action<Future<TResult>> resultCallback, Action cancelCallback)
        {
            Guid operationId = new Guid();
            EventHandler<CallbackEventArgs<TResult>> callbackListener = null;
            EventHandler<CancelEventArgs> cancellationListener = null;
            callbackListener = (sender, args) =>
                {
                    if (args.OperationId == operationId)
                    {
                        callbackChannel.UnsubscribeCallbackEvent(callbackListener);
                        callbackChannel.UnsubscribeCancellationEvent(cancellationListener);
                        resultCallback(args.Result);
                    }
                };
            cancellationListener = (sender, args) =>
                {
                    if (args.OperationId == operationId)
                    {
                        callbackChannel.UnsubscribeCallbackEvent(callbackListener);
                        callbackChannel.UnsubscribeCancellationEvent(cancellationListener);
                        cancelCallback();
                    }
                };
            callbackChannel.SubscribeCallbackEvent(callbackListener);
            callbackChannel.SubscribeCancellationEvent(cancellationListener);
            return operationId;
        }
    }
}
