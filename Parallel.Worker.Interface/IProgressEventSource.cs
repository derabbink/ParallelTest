using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parallel.Worker.Interface.Events;

namespace Parallel.Worker.Interface
{
    public interface IProgressEventSource
    {
        void SubscribeProgress(EventHandler<ProgressEventArgs> handler);

        void UnsubscribeProgress(EventHandler<ProgressEventArgs> handler);
    }

    public interface IProgressEventSource<TProgress>
        where TProgress : class
    {
        void SubscribeProgress(EventHandler<ProgressEventArgs<TProgress>> handler);

        void UnsubscribeProgress(EventHandler<ProgressEventArgs<TProgress>> handler);
    }
}
