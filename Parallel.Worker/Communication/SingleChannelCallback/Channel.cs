using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parallel.Worker.Events;
using Parallel.Worker.Interface.Communication.SingleChannelCallback;
using Parallel.Worker.Interface.Events.SingleChannelCallback;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker.Communication.SingleChannelCallback
{
    public class Channel<TArgument, TResult> : IClient<TResult>, IServer<TArgument, TResult>
        where TArgument : class
        where TResult : class
    {
        private event EventHandler<CallbackEventArgs<TResult>> Callback;

        public void Run(Guid operationId, SafeInstruction<TArgument, TResult> operation, IClient<TResult> callback)
        {
            var result = operation.Invoke();
            callback.DoCallback(operationId, result);
        }

        public void DoCallback(Guid operationId, SafeInstructionResult<TResult> result)
        {
            Callback.Raise(this, new CallbackEventArgs<TResult>(operationId, result));
        }

        public void SubscribeCallbackEvent(EventHandler<CallbackEventArgs<TResult>> handler)
        {
            Callback += handler;
        }

        public void UnsubscribeCallbackEvent(EventHandler<CallbackEventArgs<TResult>> handler)
        {
            Callback -= handler;
        }
    }
}
