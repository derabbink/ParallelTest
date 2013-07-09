﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parallel.Worker.Events.SingleChannelCallback;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker.Communication.SingleChannelCallback
{
    public interface IClient<TResult>
        where TResult : class
    {
        void DoCallback(Guid operationId, SafeInstructionResult<TResult> result);

        void SubscribeCallbackEvent(EventHandler<CallbackEventArgs<TResult>> handler);

        void UnsubscribeCallbackEvent(EventHandler<CallbackEventArgs<TResult>> handler);
    }
}
