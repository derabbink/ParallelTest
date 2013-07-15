using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parallel.Worker.Interface.Events
{
    /// <summary>
    /// For progress reporting without any value: Heartbeat reporting
    /// </summary>
    public class ProgressEventArgs : EventArgs
    {
        
    }

    /// <summary>
    /// For progress reporting with a value
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class ProgressEventArgs<TValue> : EventArgs
        where TValue : class
    {
        public ProgressEventArgs(TValue value)
        {
            Value = value;
        }

        public TValue Value { get; private set; }
    }
}
