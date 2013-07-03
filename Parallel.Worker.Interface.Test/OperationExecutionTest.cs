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

        #region Setup

        [SetUp]
        public void SetUp()
        {
            CreateOperationFunc();
            CreateOperationArg();
        }

        private void CreateOperationFunc()
        {
            _operationFunc = _ => null;
        }

        private void CreateOperationArg()
        {
            _operationArg = null;
        }

        #endregion

        #region Test Cases

        [Test]
        public void ExecuteNormal()
        {
            Operation op = new Operation(_operationFunc, _operationArg);
            op.Execute();
            var result = op.Execute();
            Assert.That(result.State, Is.EqualTo(OperationResultState.Success));
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
            Operation op = new Operation(operationFunc, _operationArg);
            var result = op.Execute();
            Assert.That(result.State, Is.EqualTo(OperationResultState.Error));
        }



        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Expected Error")]
        public void ExecutionExceptionIsCorrect()
        {
            var expectedError = "Expected Error";
            Func<object, object> operationFunc = _ =>
            {
                ThrowException(expectedError);
                return null;
            };
            Operation op = new Operation(operationFunc, _operationArg);
            var result = op.Execute();
            Assert.That(result.State, Is.EqualTo(OperationResultState.Error));
            throw result.Exception;
        }

        #endregion

        #region Teardown

        [TearDown]
        public void Teardown()
        {
            DestroyOperationFunc();
            DestroyOperationArg();
        }

        private void DestroyOperationFunc()
        {
            _operationFunc = null;
        }

        private void DestroyOperationArg()
        {
            _operationArg = null;
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
