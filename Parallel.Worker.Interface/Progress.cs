using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading;
using Parallel.Worker.Interface.Events;

namespace Parallel.Worker.Interface
{
    /// <summary>
    /// Default implementation used only as a proxy object
    /// </summary>
    public class Progress : IProgress
    {
        public event EventHandler<ProgressEventArgs> ProgressChanged;

        public void Report()
        {
            ProgressChanged.Raise(this, new ProgressEventArgs());
        }
    }

    /// <summary>
    /// Default implementation used only as a proxy object
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class Progress<TValue> : Progress, IProgress<TValue>
        where TValue : class
    {
        public new event EventHandler<ProgressEventArgs<TValue>> ProgressChanged;

        //hide method
        private new void Report()
        {
            throw new NotImplementedException();
        }

        public void Report(TValue value)
        {
            ProgressChanged.Raise(this, new ProgressEventArgs<TValue>(value));
        }
    }
}
