using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parallel.Worker.Interface.Events.SingleChannelCallback;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker.Interface.Communication.SingleChannelCallback
{
    public interface IClient<TResult>
        where TResult : class
    {
        void OnCallback(Guid operationId, Future<TResult> result);

        void OnCancel(Guid operationId);

        void SubscribeCallbackEvent(EventHandler<CallbackEventArgs<TResult>> handler);
        void UnsubscribeCallbackEvent(EventHandler<CallbackEventArgs<TResult>> handler);

        void SubscribeCancellationEvent(EventHandler<CancelEventArgs> handler);
        void UnsubscribeCancellationEvent(EventHandler<CancelEventArgs> handler);
    }
}
