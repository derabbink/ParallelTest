using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading;
using Parallel.Worker.Interface.Events;

namespace Parallel.Worker.Interface
{
    public class Progress : IProgress
    {
        public event EventHandler<ProgressEventArgs> ProgressChanged;

        public void OnProgress()
        {
            ProgressChanged.Raise(this, new ProgressEventArgs());
        }
    }

    public class Progress<TValue> : Progress, IProgress<TValue>
        where TValue : class
    {
        public new event EventHandler<ProgressEventArgs<TValue>> ProgressChanged;

        public void OnProgress(TValue value)
        {
            ProgressChanged.Raise(this, new ProgressEventArgs<TValue>(value));
        }
    }
}
