using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Parallel.Worker.Interface.Events;

namespace Parallel.Worker.Interface.Test
{
    [TestFixture]
    public class OperationCreationTest
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
        public void ValidInternalInstruction()
        {
            Operation op = new Operation(_operationFunc, _operationArg);
            Assert.Pass();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException), ExpectedMessage = "Precondition failed: operation != null  operation must not be null\r\nParameter name: operation must not be null")]
        public void InvalidInternalInstruction()
        {
            Operation op = new Operation(null, _operationArg);
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
