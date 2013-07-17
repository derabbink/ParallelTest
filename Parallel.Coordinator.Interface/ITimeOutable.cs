using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parallel.Worker.Interface;

namespace Parallel.Coordinator.Interface
{
    public interface ITimeOutable : IAwaitable, ICancelable, IProgressEventSource
    {
    }

    public interface ITimeOutable<TProgress> : IAwaitable, ICancelable, IProgressEventSource<TProgress>
        where TProgress : class
    {
    }
}
