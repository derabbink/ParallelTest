using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parallel.Worker.Interface
{
    public class OperationResult
    {
        private OperationResult(OperationResultState state, object value, Exception exception)
        {
            State = state;
            Value = value;
            Exception = exception;
        }

        internal static OperationResult CreateSuccessful(object value)
        {
            return new OperationResult(OperationResultState.Success, value, null);
        }

        internal static OperationResult CreateFailed(Exception error)
        {
            return new OperationResult(OperationResultState.Error, null, error);
        }

        public OperationResultState State { get; private set; }

        public object Value { get; private set; }

        public Exception Exception { get; private set; }
    }
}
