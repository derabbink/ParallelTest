using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Parallel.Worker.Interface
{
    public interface IAwaitable
    {
        /// <summary>
        /// Blocks until all work is done.
        /// </summary>
        /// <param name="timeoutMS">number of miliseconds after which to stop waiting</param>
        /// <param name="cancellationToken">cancellation token telling when to stop waiting</param>
        /// <returns>true if operation completed within timeOut, otherwise false</returns>
        /// <exception cref="OperationCanceledException">if the wait was cancelled due to the cancellation token</exception>
        bool Wait(int timeoutMS, CancellationToken cancellationToken);
    }
}
