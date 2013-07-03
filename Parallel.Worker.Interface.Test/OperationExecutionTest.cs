using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Parallel.Worker.Interface.Events;

namespace Parallel.Worker.Interface.Test
{
    [TestFixture]
    public class OperationExecutionTest
    {
        private Func<object, object> _operationFunc;
        private object _operationArg;
        private Guid _operationId;
        private EventHandler<OperationStartedEventArgs> _operationStartedEventHandler;
        private EventHandler<OperationCompletedEventArgs> _operationCompletedEventHandler;

        #region Setup

        [SetUp]
        public void SetUp()
        {
            CreateOperationFunc();
            CreateOperationArg();
            CreateOperationId();
            CreateOperationStartedListener();
            CreateOperationCompletedListener();
        }

        private void CreateOperationFunc()
        {
            _operationFunc = _ => null;
        }

        private void CreateOperationArg()
        {
            _operationArg = null;
        }

        private void CreateOperationId()
        {
            _operationId = new Guid();
        }

        private void CreateOperationStartedListener()
        {
            _operationStartedEventHandler = (sender, args) => { };
        }

        private void CreateOperationCompletedListener()
        {
            _operationCompletedEventHandler = (sender, args) => { };
        }

        #endregion

        #region Test Cases

        [Test]
        public void ExecuteNormal()
        {
            Operation op = Operation.CreateWithListeners(_operationFunc, _operationArg, _operationId,
                                                         _operationStartedEventHandler, _operationCompletedEventHandler);
            op.Execute();
            Assert.Pass();
        }

        [Test]
        public void ExecutionExceptionIsContained()
        {
            var unexpectedError = "unexpected error";
            Func<object, object> operationFunc = _ =>
                {
                    ThrowException(unexpectedError);
                    return null;
                };
            Operation op = Operation.CreateWithListeners(operationFunc, _operationArg, _operationId,
                                                         _operationStartedEventHandler, _operationCompletedEventHandler);
            op.Execute();
            Assert.Pass();
        }

        #endregion

        #region Teardown

        [TearDown]
        public void Teardown()
        {
            DestroyOperationFunc();
            DestroyOperationArg();
            DestroyOperationId();
            DestroyOperationStartedListener();
            DestroyOperationCompletedListener();
        }

        private void DestroyOperationFunc()
        {
            _operationFunc = null;
        }

        private void DestroyOperationArg()
        {
            _operationArg = null;
        }

        private void DestroyOperationId()
        {
            //nothing to do here
        }

        private void DestroyOperationStartedListener()
        {
            _operationStartedEventHandler = null;
        }

        private void DestroyOperationCompletedListener()
        {
            _operationCompletedEventHandler = null;
        }

        #endregion

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
