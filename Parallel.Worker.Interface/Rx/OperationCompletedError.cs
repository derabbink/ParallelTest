using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parallel.Worker.Interface.Rx
{
    /// <summary>
    /// Rx message for operations completed with error
    /// </summary>
    internal class OperationCompletedError : OperationProgress
    {
        /// <summary>
        /// throws <paramref name="exception"/>
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="operationId"></param>
        internal OperationCompletedError(Exception exception, Guid operationId) : base(operationId)
        {
            throw exception;
        }
    }
}
