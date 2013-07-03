using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Parallel.Worker.Interface.Util;

namespace Parallel.Worker.Interface.Test.Util
{
    [TestFixture]
    public class OperationWrapperTest
    {
        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "FuncNoArguments")]
        public void ExecutionWrappedFuncNoArguments()
        {
            string expectedError = "FuncNoArguments";
            Func<object> innerOperation = () =>
                {
                    ThrowException(expectedError);
                    return null;
                };
            var wrappedOperation = OperationWrapper.Wrap(innerOperation);
            wrappedOperation(null);
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "ActionNoArguments")]
        public void ExecutionWrappedActionNoArguments()
        {
            string expectedError = "ActionNoArguments";
            Action innerOperation = () => ThrowException(expectedError);
            var wrappedOperation = OperationWrapper.Wrap(innerOperation);
            wrappedOperation(null);
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "ActionWithArgument")]
        public void ExecutionWrappedActionWithArgument()
        {
            string expectedError = "ActionWithArgument";
            Action<object> innerOperation = arg => ThrowException(expectedError);
            var wrappedOperation = OperationWrapper.Wrap(innerOperation);
            wrappedOperation(null);
        }

        /// <summary>
        /// used to verify execution of inner (wrapped) methods
        /// </summary>
        /// <param name="msg"></param>
        private void ThrowException(string msg)
        {
            throw new Exception(msg);
        }
    }
}
