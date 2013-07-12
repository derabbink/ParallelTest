using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Parallel.Worker.Events;
using Parallel.Worker.Interface;
using Parallel.Worker.Interface.Communication.SingleChannelCallback;
using Parallel.Worker.Interface.Events.SingleChannelCallback;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker.Communication.SingleChannelCallback
{
    public class Channel<TArgument, TResult> : IClient<TResult>, IServer<TArgument, TResult>
        where TArgument : class
        where TResult : class
    {
        private event EventHandler<CallbackEventArgs<TResult>> ResultCallback;
        private event EventHandler<CancelEventArgs> CancelCallback;
        private IDictionary<Guid, CancellationTokenSource> _cancellationTokenSources; 

        public Channel()
        {
            _cancellationTokenSources = new ConcurrentDictionary<Guid, CancellationTokenSource>();
        }

        public void Run(Guid operationId, Func<CancellationToken, TArgument, TResult> instruction, TArgument argument, IClient<TResult> callback)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            _cancellationTokenSources.Add(operationId, cts);

            Func<CancellationToken, TResult> applied = Executor.ApplyArgumentGeneric(instruction, argument);
            Future<TResult> result = Future<TResult>.Create(applied);
            result.RunSynchronously();
            if (result.IsDone)
                callback.DoCallback(operationId, result);
            else if (result.IsCanceled)
                callback.ConfirmCancellation(operationId);

            _cancellationTokenSources.Remove(operationId);
        }

        public void Cancel(Guid operationId)
        {
            CancellationTokenSource cts;
            if (_cancellationTokenSources.TryGetValue(operationId, out cts))
                cts.Cancel();
        }

        public void DoCallback(Guid operationId, Future<TResult> result)
        {
            ResultCallback.Raise(this, new CallbackEventArgs<TResult>(operationId, result));
        }

        public void ConfirmCancellation(Guid operationId)
        {
            CancelCallback.Raise(this, new CancelEventArgs(operationId));
        }

        public void SubscribeCallbackEvent(EventHandler<CallbackEventArgs<TResult>> handler)
        {
            ResultCallback += handler;
        }
        public void UnsubscribeCallbackEvent(EventHandler<CallbackEventArgs<TResult>> handler)
        {
            ResultCallback -= handler;
        }

        public void SubscribeCancellationEvent(EventHandler<CancelEventArgs> handler)
        {
            CancelCallback += handler;
        }
        public void UnsubscribeCancellationEvent(EventHandler<CancelEventArgs> handler)
        {
            CancelCallback -= handler;
        }
    }
}
