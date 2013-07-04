using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Parallel.Worker.Interface.Events;
using Parallel.Worker.Interface.Util;

namespace Parallel.Worker.Interface
{
    /// <summary>
    /// Wraps a Func's execution and contains potential errors
    /// </summary>
    internal class Operation
    {
        private readonly Func<object, object> _operation;
        private readonly object _arg;
        
        internal Operation(Func<object, object> operation, object arg)
        {
            Contract.Requires<ArgumentNullException>(operation != null, "operation must not be null");

            _operation = operation;
            _arg = arg;
        }

        /// <summary>
        /// Executes the internal operation method
        /// and wraps the result
        /// </summary>
        internal OperationResult Execute()
        {
            return ExecuteContained();
        }

        /// <summary>
        /// Executes internal operation safely
        /// contains all possible exceptions
        /// </summary>
        private OperationResult ExecuteContained()
        {
            try
            {
                object value = _operation(_arg);
                return OperationResult.CreateSuccessful(value);
            }
            catch (Exception e)
            {
                return OperationResult.CreateFailed(e);
            }
        }
    }
}
