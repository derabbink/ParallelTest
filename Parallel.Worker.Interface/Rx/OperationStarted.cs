using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parallel.Worker.Interface.Rx
{
    /// <summary>
    /// Rx message for informing about an operation's start
    /// </summary>
    public class OperationStarted : OperationProgress
    {
        public OperationStarted(Guid operationId) : base(operationId)
        {
        }
    }
}
