using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parallel.Worker.Interface.Events;

namespace Parallel.Worker.Interface
{
    public interface IProgress
    {
        void OnProgress();
    }

    public interface IProgress<TValue>
        where TValue : class
    {
        void OnProgress(TValue value);
    }
}
