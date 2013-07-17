using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Parallel.Worker.Interface;
using Parallel.Worker.Interface.Events;

namespace Parallel.Coordinator.Interface
{
    public abstract class TimeOutable : ITimeOutable
    {
        public static TimeOutable Compose(IAwaitable awaitable, ICancelable cancelable, IProgressEventSource progressEventSource)
        {
            return new ComposedTimeOutAble(awaitable, cancelable, progressEventSource);
        }

        public static TimeOutable FromFuture(Future future)
        {
            return Compose(future, future, future);
        }

        public static TimeOutable FromFuture<TResult>(Future<TResult> future)
        {
            return Compose(future, future, future);
        }

        public abstract bool Wait(int timeoutMS, CancellationToken cancellationToken);
        public abstract void Cancel();
        public abstract void SubscribeProgress(EventHandler<ProgressEventArgs> handler);
        public abstract void UnsubscribeProgress(EventHandler<ProgressEventArgs> handler);
    }

    internal class ComposedTimeOutAble : TimeOutable
    {
        private readonly IAwaitable _awaitable;
        private readonly ICancelable _cancelable;
        private readonly IProgressEventSource _progressEventSource;

        public ComposedTimeOutAble(IAwaitable awaitable, ICancelable cancelable, IProgressEventSource progressEventSource)
        {
            _awaitable = awaitable;
            _cancelable = cancelable;
            _progressEventSource = progressEventSource;
        }

        public override bool Wait(int timeoutMS, CancellationToken cancellationToken)
        {
            return _awaitable.Wait(timeoutMS, cancellationToken);
        }

        public override void Cancel()
        {
            _cancelable.Cancel();
        }

        public override void SubscribeProgress(EventHandler<ProgressEventArgs> handler)
        {
            _progressEventSource.SubscribeProgress(handler);
        }

        public override void UnsubscribeProgress(EventHandler<ProgressEventArgs> handler)
        {
            _progressEventSource.UnsubscribeProgress(handler);
        }
    }
}
