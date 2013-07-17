using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parallel.Worker.Interface
{
    public interface ICancelable
    {
        void Cancel();
    }
}
